using Safe_file_storage.Models.Abstract;
using Safe_file_storage.Models.FileAtributes;
using Safe_file_storage.Models.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Safe_file_storage.Models.Services
{
    public class LntfsSecureFileWorker : IFileWorker, IDisposable
    {
        const int _mftRecordNo = 0;
        const int _dotRecordNo = 1;
        const int _bitMapRecordNo = 2;


        static Dictionary<int, Type> _fileAttributesId = new Dictionary<int, Type>
        {
            {0,null },
            {1,typeof(FileNameAttribute)},
            {2,typeof(HistoryAttribute)},
            {3,typeof(DirectoryAttribute)},
            {4,typeof(DataAttribute)},
            {5,typeof(BitMapAttribute)}

        };
        SymmetricAlgorithm _encryption;

        FileModel _mft;
        FileModel _bitMap;

        public FileModel RootDirectory
        {
            get
            {
                FileModel res = ReadFileHeader(_dotRecordNo);
                res.DirectoryAttribute = ReadFileAttribute<DirectoryAttribute>(_dotRecordNo);
                return res;
            }
        }


        FileStream _fileStream;
        BitMapAttribute _mftBitMap;

        /// <summary>
        /// Битмап файла $BITMAP. 
        /// </summary>
        BitMapAttribute _bitMapBitMap;

        ILntfsConfiguration _configuration;
        public LntfsSecureFileWorker(ILntfsConfiguration configuration,SymmetricAlgorithm symmetricAlgorithm)
        {
            _configuration = configuration;
            _encryption = symmetricAlgorithm;


         


            if (!File.Exists(_configuration.FilePath))
            {
                File.WriteAllBytes(configuration.FilePath, _encryption.CreateEncryptor().TransformFinalBlock(new byte[configuration.FileSize], 0, configuration.FileSize));


                _fileStream = File.Open(configuration.FilePath, FileMode.Open, FileAccess.ReadWrite);


                _mftBitMap = new BitMapAttribute(configuration.MFTZoneSize / configuration.MFTRecordSize);
                _bitMapBitMap = new BitMapAttribute((configuration.FileSize - configuration.MFTZoneSize) / configuration.ClusterSize);

                FileModel _dot = new FileModel(_dotRecordNo, _dotRecordNo, new FileNameAttribute(".", 0, ""), new HistoryAttribute(), new DirectoryAttribute());
                _dot.IsWritten = false;
                _mft = new FileModel(_mftRecordNo, _dotRecordNo, false);
                _mft.IsWritten = false;
                _bitMap = new FileModel(_bitMapRecordNo, _dotRecordNo, false);
                _bitMap.IsWritten = false;

                WriteFile(_dot);
                WriteFileHeader(_mft);
                WriteFileHeader(_bitMap);

                _mftBitMap.GetSpace(3);

                WriteAttribute(_mft, _mft.MFTRecordNo, 0, _mftBitMap);
                WriteAttribute(_bitMap, _bitMap.MFTRecordNo, 0, _bitMapBitMap);

                //_fileStream.Flush();
                //  _bitMapBitMap = ReadFileAttribute<BitMapAttribute>(_bitMapRecordNo);
            }
            else
            {
                _fileStream = File.Open(configuration.FilePath, FileMode.Open, FileAccess.ReadWrite);

                DirectoryAttribute attribute = ReadFileAttribute<DirectoryAttribute>(1);


                _bitMapBitMap = ReadFileAttribute<BitMapAttribute>(_bitMapRecordNo);
                _mftBitMap = ReadFileAttribute<BitMapAttribute>(_mftRecordNo);

                _mft = ReadFileHeader(_mftRecordNo);
                _bitMap = ReadFileHeader(_bitMapRecordNo);

            }



        }

        public void Dispose()
        {
            _fileStream.Dispose();
        }

        public void ExportFile(int fileMFTRecordId, string targetFilePath)
        {


            FileModel file = ReadFileHeader(fileMFTRecordId);

            if (file.IsDirectory)
            {
                if (Directory.Exists(targetFilePath))
                {
                    throw new Exception("Директория с таким именем уже существует");
                }
                DirectoryInfo createdDirectory = Directory.CreateDirectory(targetFilePath);
                DirectoryAttribute directoryAttribute = ReadFileAttribute<DirectoryAttribute>(fileMFTRecordId);

                foreach (var item in directoryAttribute.Files)
                {
                    FileNameAttribute fileNameAttribute = ReadFileAttribute<FileNameAttribute>(item.MFTRecordNo);
                    if (item.IsDirectory)
                    {
                        ExportFile(item.MFTRecordNo, Path.Combine(createdDirectory.FullName, $"{fileNameAttribute.Name}"));
                    }
                    else
                    {
                        ExportFile(item.MFTRecordNo, Path.Combine(createdDirectory.FullName, $"{fileNameAttribute.Name}{fileNameAttribute.Extention}"));
                    }
                }
            }
            else
            {
                if (File.Exists(targetFilePath))
                {
                    throw new Exception("Файл с таким именем уже существует");
                }

                FileStream fileToWrite = File.Create(targetFilePath);

                ReadAttributeToStream<DataAttribute>(fileMFTRecordId, 0, fileToWrite);

                fileToWrite.Close();
            }
        }

        public FileModel ImportFile(string targetFilePath, int directoryToWriteMFTNo)
        {
            FileModel file = ReadFileHeader(directoryToWriteMFTNo);
            FileModel res = ImportSubFiles(targetFilePath, directoryToWriteMFTNo);
            file.DirectoryAttribute = ReadFileAttribute<DirectoryAttribute>(directoryToWriteMFTNo);
            file.DirectoryAttribute.Files.Add(res);
            WriteAttribute(file, file.MFTRecordNo, 0, file.DirectoryAttribute);
            return res;
        }

        public FileModel ImportSubFiles(string targetFilePath, int directoryToWriteMFTNo)
        {
            FileModel file;

            if (File.Exists(targetFilePath))
            {
                FileStream fileStream = File.OpenRead(targetFilePath);

                file = new FileModel(
                       _mftBitMap.GetSpace(1).First().start,
                       directoryToWriteMFTNo,
                       new FileNameAttribute(Path.GetFileNameWithoutExtension(targetFilePath), fileStream.Length, Path.GetExtension(targetFilePath)),
                       new HistoryAttribute(),
                       new DataAttribute()
                   );
                file.IsWritten = false;

                WriteFileHeader(file);

                // Запись данных файла без занесения их в память целиком.
                List<DataRun> dataRuns = _bitMapBitMap.GetSpace((int)(PaddingToBlockSize(fileStream.Length) / _configuration.ClusterSize) +
                    (PaddingToBlockSize(fileStream.Length) % _configuration.ClusterSize == 0 ? 0 : 1));
                WriteAttributeFromStream(
                    file.MFTRecordNo,
                    0,
                    _fileAttributesId.Where(e => e.Value == typeof(DataAttribute)).First().Key,
                    fileStream,
                    dataRuns);

                WriteAttribute(file, file.MFTRecordNo, 1, file.FileNameAttribute);
                WriteAttribute(file, file.MFTRecordNo, 2, file.HistoryAttribute);
                fileStream.Close();
            }
            else if (Directory.Exists(targetFilePath))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(targetFilePath);

                file = new FileModel(
                        _mftBitMap.GetSpace(1).First().start,
                        directoryToWriteMFTNo,
                        new FileNameAttribute(directoryInfo.Name, 0, ""),
                        new HistoryAttribute(),
                        new DirectoryAttribute()
                    );
                file.IsWritten = false;

                foreach (var item in Directory.GetFiles(targetFilePath))
                {
                    file.DirectoryAttribute!.Files.Add(ImportSubFiles(item, file.MFTRecordNo));
                }
                foreach (var item in Directory.GetDirectories(targetFilePath))
                {

                    file.DirectoryAttribute!.Files.Add(ImportSubFiles(item, file.MFTRecordNo));
                }
                WriteFile(file);
            }
            else
            {
                throw new FileNotFoundException(null, targetFilePath);
            }

            WriteAttribute(_mft, _mft.MFTRecordNo, 0, _mftBitMap);
            WriteAttribute(_bitMap, _bitMap.MFTRecordNo, 0, _bitMapBitMap);

            return file;
        }

        public T ReadFileAttribute<T>(int fileMFTRecordId)
            where T : FileAttribute
        {
            int position = fileMFTRecordId * _configuration.MFTRecordSize + _encryption.BlockSize;
            bool isFound = false;
            int attributeNo = 0;
            int attributeSize = 0;
            while (position + _configuration.AttributeHeaderSize <= (fileMFTRecordId + 1) * _configuration.MFTRecordSize)
            {
                _fileStream.Position = position;

                using (CryptoStream crypto = new CryptoStream(_fileStream, _encryption.CreateDecryptor(), CryptoStreamMode.Read, true))
                {
                    using (BinaryReader reader = new BinaryReader(crypto, Encoding.Default, true))
                    {
                        int attributeId = reader.ReadInt32();
                        if (_fileAttributesId[attributeId] is not null && _fileAttributesId[attributeId].Equals(typeof(T)))
                        {
                            attributeSize = reader.ReadInt32();
                            isFound = true;
                            break;
                        }
                    }
                }

                attributeNo++;
                position += _encryption.BlockSize;
            }

            if (!isFound)
            {
                throw new Exception("Attribute not found");
            }


            MemoryStream attributeMemoryStream = new MemoryStream();

            ReadAttributeToStream<T>(fileMFTRecordId, attributeNo, attributeMemoryStream);

            T Attribute = (T)Activator.CreateInstance(typeof(T), new object[] { attributeMemoryStream });
            return Attribute;
        }

        private void ReadAttributeToStream<T>(int fileMFTRecordId, int attributeNo, Stream targetStream)
            where T : FileAttribute
        {
            int position = fileMFTRecordId * _configuration.MFTRecordSize + _encryption.BlockSize * (attributeNo + 1);
            _fileStream.Position = position;
            int attributeSize = 0;
            using (CryptoStream crypto = new CryptoStream(_fileStream, _encryption.CreateDecryptor(), CryptoStreamMode.Read, true))
            {
                using (BinaryReader reader = new BinaryReader(crypto, Encoding.Default, true))
                {
                    int attributeId = reader.ReadInt32();
                    if (_fileAttributesId[attributeId] is not null && _fileAttributesId[attributeId].Equals(typeof(T)))
                    {
                        attributeSize = reader.ReadInt32();
                    }
                    else
                    {
                        throw new Exception("Wrong attribute id");
                    }
                }
            }
            List<DataRun> dataRuns = ReadDataRuns(fileMFTRecordId, attributeNo);

            byte[] bytes = new byte[attributeSize];

            int sizeLeft = attributeSize;
            foreach (var item in dataRuns)
            {
                _fileStream.Position = _configuration.MFTZoneSize + item.start * _configuration.ClusterSize;
                using (CryptoStream crypto = new CryptoStream(_fileStream, _encryption.CreateDecryptor(), CryptoStreamMode.Read, true))
                {
                    int sizeToRead = item.size * _configuration.ClusterSize;
                    if (item.size * _configuration.ClusterSize < sizeLeft)
                    {
                        CopyPartOfStream(crypto, targetStream, item.size * _configuration.ClusterSize);

                        sizeLeft -= item.size * _configuration.ClusterSize;
                    }
                    else
                    {
                        CopyPartOfStream(crypto, targetStream, sizeLeft);
                        break;
                    }
                }
            }
        }

        public FileModel ReadFileHeader(int fileMFTRecordId)
        {

            _fileStream.Position = fileMFTRecordId * _configuration.MFTRecordSize;
            FileModel res;
            using (CryptoStream crypto = new CryptoStream(_fileStream, _encryption.CreateDecryptor(), CryptoStreamMode.Read, true))
            {
                using (BinaryReader reader = new BinaryReader(crypto, Encoding.Default, true))
                {
                    int mftRecordNo = reader.ReadInt32();
                    int parentDirectoryMftNo = reader.ReadInt32();
                    bool isDirectory = reader.ReadBoolean();
                    res = new FileModel(mftRecordNo, parentDirectoryMftNo, isDirectory);
                }
            }
            return res;

        }

        public FileModel ReadFileHeaderAndAttributes(int fileMFTRecordId)
        {
            throw new NotImplementedException();
        }

        public void WriteFile(FileModel file)
        {
            WriteFileHeader(file);

            if (file.IsDirectory)
            {
                WriteAttribute(file, file.MFTRecordNo, 0, file.DirectoryAttribute);
            }
            else
            {
                WriteAttribute(file, file.MFTRecordNo, 0, file.DataAttribute);
            }

            WriteAttribute(file, file.MFTRecordNo, 1, file.FileNameAttribute);
            WriteAttribute(file, file.MFTRecordNo, 2, file.HistoryAttribute);
        }
        void WriteFileHeader(FileModel file)
        {
            _fileStream.Position = file.MFTRecordNo * _configuration.MFTRecordSize;
            using (CryptoStream crypto = new CryptoStream(_fileStream, _encryption.CreateEncryptor(), CryptoStreamMode.Write, true))
            {
                using (BinaryWriter writer = new BinaryWriter(crypto, Encoding.Default, true))
                {
                    writer.Write(file.MFTRecordNo);
                    writer.Write(file.ParentDirectoryRecordNo);
                    writer.Write(file.IsDirectory);

                    // У каждого файла кроме MFT и BITMAP одновременно может быть тольк 3 атрибута.
                    writer.Write(3);
                }
            }
        }
        void WriteAttribute(FileModel file, int mftNo, int attributeNo, FileAttribute attribute)
        {
            int position = mftNo * _configuration.MFTRecordSize + (attributeNo + 1) * _encryption.BlockSize;
            if (position > (mftNo + 1) * _configuration.MFTRecordSize)
            {
                throw new ArgumentOutOfRangeException("Попытка записи в соседнюю MFT запись");
            }

            MemoryStream attributeMemoryStream = attribute.GetDataAsStream();


            List<DataRun> dataRuns;
            int sizeInClusters = (int)(PaddingToBlockSize(attributeMemoryStream.Length) / _configuration.ClusterSize) +
              (PaddingToBlockSize(attributeMemoryStream.Length) % _configuration.ClusterSize == 0 ? 0 : 1);
            if (file.IsWritten)
            {
                dataRuns = ReadDataRuns(mftNo, attributeNo);
                int allocatedSize = dataRuns.Sum(e => e.size);
                // Уравнение выделеного и реального размера в кластерах для нерезидентного атрибута.
                if (allocatedSize < sizeInClusters)
                {
                    dataRuns.AddRange(_bitMapBitMap.GetSpace(sizeInClusters - allocatedSize));
                }
                else if (allocatedSize > sizeInClusters)
                {
                    int pointer = dataRuns.Count - 1;
                    int sizeToFree = allocatedSize - sizeInClusters;

                    while (allocatedSize > sizeInClusters)
                    {
                        if (dataRuns[pointer].size < sizeToFree)
                        {
                            sizeToFree -= dataRuns[pointer].size;
                            _bitMapBitMap.FreeSpace(dataRuns[pointer]);
                            dataRuns.RemoveAt(pointer);
                        }
                        else
                        {
                            _bitMapBitMap.FreeSpace(dataRuns[pointer], sizeToFree);
                            DataRun dataRun = dataRuns[pointer];
                            dataRun.size -= sizeToFree;
                            dataRuns[pointer] = dataRun;
                            sizeToFree = 0;
                        }
                    }
                }
            }
            else
            {
                dataRuns = _bitMapBitMap.GetSpace(sizeInClusters);
            }
            if (attribute == _bitMapBitMap)
            {
                attributeMemoryStream = attribute.GetDataAsStream();
            }
            else
            {
                WriteAttribute(_bitMap, _bitMap.MFTRecordNo, 0, _bitMapBitMap);
            }
            WriteAttributeFromStream(mftNo, attributeNo, _fileAttributesId.Where(e => e.Value == attribute.GetType()).First().Key, attributeMemoryStream, dataRuns);

        }

        private void WriteAttributeFromStream(int mftNo, int attributeNo, int attributeId, Stream attributeStream, List<DataRun> dataRuns)
        {
            _fileStream.Position = mftNo * _configuration.MFTRecordSize + (attributeNo + 1) * _encryption.BlockSize;
            using (CryptoStream crypto = new CryptoStream(_fileStream, _encryption.CreateEncryptor(), CryptoStreamMode.Write, true))
            {
                using (BinaryWriter writer = new BinaryWriter(crypto, Encoding.Default, true))
                {
                    writer.Write(attributeId);
                    writer.Write(attributeStream.Length);
                    writer.Write(dataRuns.Count);
                    foreach (var item in dataRuns)
                    {
                        writer.Write(item.start);
                        writer.Write(item.size);
                    }
                }
            }
            attributeStream.Position = 0;
            foreach (var item in dataRuns)
            {
                _fileStream.Position = _configuration.MFTZoneSize + item.start * _configuration.ClusterSize;
                using (CryptoStream crypto = new CryptoStream(_fileStream, _encryption.CreateEncryptor(), CryptoStreamMode.Write, true))
                {
                    CopyPartOfStream(attributeStream, crypto, item.size * _configuration.ClusterSize);
                }
            }
        }
        List<DataRun> ReadDataRuns(int mftNo, int attributeNo)
        {
            _fileStream.Position = mftNo * _configuration.MFTRecordSize + (attributeNo + 1) * _encryption.BlockSize;

            List<DataRun> dataRuns = new List<DataRun>();
            using (CryptoStream crypto = new CryptoStream(_fileStream, _encryption.CreateDecryptor(), CryptoStreamMode.Read, true))
            {
                using (BinaryReader reader = new BinaryReader(crypto, Encoding.Default, true))
                {
                    int id = reader.ReadInt32();
                    long len = reader.ReadInt64();
                    int dataRunsCount = reader.ReadInt32();
                    for (int i = 0; i < dataRunsCount; i++)
                    {
                        int dataRunStart = reader.ReadInt32();
                        int dataRunSize = reader.ReadInt32();
                        dataRuns.Add(new DataRun() { start = dataRunStart, size = dataRunSize });
                    }
                }
            }
            return dataRuns;
        }
        void CopyPartOfStream(Stream source, Stream target, int bytesToCopy)
        {
            int bytesToCopyLeft = bytesToCopy;
            byte[] buffer = new byte[64 * 1024];
            while (bytesToCopyLeft != 0)
            {
                int toRead = buffer.Length < bytesToCopyLeft ? buffer.Length : bytesToCopyLeft;

                int readed = source.Read(buffer, 0, toRead);
                if (readed == 0)
                    break;

                target.Write(buffer, 0, readed);

                bytesToCopyLeft -= readed;
            }
            target.Flush();
        }

        long PaddingToBlockSize(long value)
        {
            return value + value % _encryption.BlockSize;
        }

        /// Структура файла
        ///                       0| MFTRecordNo
        ///                       4| Parent Directory MFTRecordNo
        ///                       8| Is Directory
        ///                       9| Attibute Count
        ///                      13| Padding(size 1)
        ///   _encryption.blockSize| Attribute № 1 id (from _fileAttributesId)
        /// _encryption.blockSize+4| Real size of attribute
        /// _encryption.blockSize+8| DataRuns count
        ///_encryption.blockSize+12| DataRun № 1 start
        ///_encryption.blockSize+16| DataRun № 1 size
        /// ...
        ///  N * blockSize | Attribute № N 
    }
}
