﻿using Safe_file_storage.Models.Abstract;
using Safe_file_storage.Models.FileAtributes;
using Safe_file_storage.Models.Interfaces;
using Safe_file_storage.Models.Services.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Safe_file_storage.Models.Services
{
    internal class LntfsSecureFileSystemService : IFileSystemService, IDisposable
    {
        const int _mftRecordNo = 0;
        const int _dotRecordNo = 1;
        const int _bitMapRecordNo = 2;
        const int _bootRecordNo = 3;

        const int _lastSystemMftNo = 10;

        static Dictionary<int, Type> _fileAttributesId = new Dictionary<int, Type>
        {
            {0,null },
            {1,typeof(FileNameAttribute)},
            {2,typeof(HistoryAttribute)},
            {3,typeof(DirectoryAttribute)},
            {4,typeof(DataAttribute)},
            {5,typeof(BitMapAttribute)}
        };

        static Dictionary<Type, int> _fileAttributeNo = new Dictionary<Type, int>
        {
            {typeof(FileNameAttribute),1},
            {typeof(HistoryAttribute),2},
            {typeof(DirectoryAttribute),0},
            {typeof(DataAttribute),0},
            {typeof(BitMapAttribute),0}
        };

        ICryptoService _cryptoService;

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


        Stream _fileStream;
        BitMapAttribute _mftBitMap;

        /// <summary>
        /// Битмап файла $BITMAP. 
        /// </summary>
        BitMapAttribute _bitMapBitMap;

        ILntfsConfiguration _configuration;
        internal LntfsSecureFileSystemService(IFileSystemStream fileSystemStream, ILntfsConfiguration configuration, ICryptoService crypttoService)
        {
            _configuration = configuration;
            _cryptoService = crypttoService;

            _fileStream = fileSystemStream.LntfsStream;

            if (!IsStreamInitialize())
            {
                Initialize();

                //_fileStream.Flush();
                //  _bitMapBitMap = ReadFileAttribute<BitMapAttribute>(_bitMapRecordNo);
            }
            else
            {
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
            _cryptoService.Dispose();
        }

        public void ExportFile(int fileMFTRecordId, string targetFilePath)
        {
            FileModel file = ReadFileHeader(fileMFTRecordId);
            file.HistoryAttribute = ReadFileAttribute<HistoryAttribute>(file.MFTRecordNo);
            file.HistoryAttribute.AddHistoryRecord(new HistoryRecord(DateTime.Now, HistoryRecordAction.Exported));
            WriteAttribute(file, file.HistoryAttribute);

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

                ReadAttributeToStream<DataAttribute>(fileMFTRecordId, fileToWrite);

                fileToWrite.Close();
            }
        }

        public FileModel ImportFile(string targetFilePath, int directoryToWriteMFTNo)
        {
            long totalSize = 0;
            int totalCount = 0;
            CalcSizeAndCountOfSubFiles(targetFilePath, ref totalSize, ref totalCount);

            if (totalCount> _mftBitMap.SpaceLeft)
            {
                return null;
            }
            if (1+(totalSize / _configuration.ClusterSize) > _bitMapBitMap.SpaceLeft)
            {
                return null;
            }

            FileModel parentDir = ReadFileHeader(directoryToWriteMFTNo);


            FileModel res = ImportSubFiles(targetFilePath, directoryToWriteMFTNo);
            parentDir.DirectoryAttribute = ReadFileAttribute<DirectoryAttribute>(directoryToWriteMFTNo);
            parentDir.DirectoryAttribute.Files.Add(res);
            WriteAttribute(parentDir, parentDir.DirectoryAttribute);
            return res;
        }

        public void CalcSizeAndCountOfSubFiles(string targetFilePath,ref long totalSize,ref int totalCount)
        {
            totalCount++;
            if (File.Exists(targetFilePath))
            {
                totalSize += new FileInfo(targetFilePath).Length;
            }
            else if(Directory.Exists(targetFilePath))
            {
                foreach (var item in Directory.GetFiles(targetFilePath))
                {
                    CalcSizeAndCountOfSubFiles(item,ref totalSize,ref totalCount);
                }
            }
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
                         false,
                       new FileNameAttribute(Path.GetFileNameWithoutExtension(targetFilePath), fileStream.Length, Path.GetExtension(targetFilePath)),
                       new HistoryAttribute(),
                       new DataAttribute()
                   );
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

                WriteAttribute(file, file.FileNameAttribute);
                WriteAttribute(file, file.HistoryAttribute);
                fileStream.Close();
            }
            else if (Directory.Exists(targetFilePath))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(targetFilePath);

                file = new FileModel(
                        _mftBitMap.GetSpace(1).First().start,
                        directoryToWriteMFTNo,
                          false,
                        new FileNameAttribute(directoryInfo.Name, 0, ""),
                        new HistoryAttribute(),
                        new DirectoryAttribute()
                    );
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

            WriteAttribute(_mft, 0, _mftBitMap);
            WriteAttribute(_bitMap, 0, _bitMapBitMap);

            return file;
        }

        public T ReadFileAttribute<T>(int fileMFTRecordNo)
            where T : FileAttribute
        {
            int position = fileMFTRecordNo * _configuration.MFTRecordSize + _cryptoService.BlockSize * (_fileAttributeNo[typeof(T)] + 1);
            bool isFound = false;
            int attributeSize = 0;

            _fileStream.Position = position;

            using (CryptoStream crypto = _cryptoService.CreateDecryptionStream(
                _fileStream,
                CryptoStreamMode.Read,
                true,
                fileMFTRecordNo.ToString()))
            {
                using (BinaryReader reader = new BinaryReader(crypto, Encoding.Default, true))
                {
                    int attributeId = reader.ReadInt32();
                    if (_fileAttributesId[attributeId] is not null && _fileAttributesId[attributeId].Equals(typeof(T)))
                    {
                        attributeSize = reader.ReadInt32();
                        isFound = true;
                    }
                }
            }

            if (!isFound)
            {
                throw new Exception("Attribute not found");
            }


            MemoryStream attributeMemoryStream = new MemoryStream();

            ReadAttributeToStream<T>(fileMFTRecordNo, attributeMemoryStream);
            T Attribute = (T)Activator.CreateInstance(typeof(T), new object[] { attributeMemoryStream });
            return Attribute;
        }

        private void ReadAttributeToStream<T>(int fileMFTRecordNo, Stream targetStream)
            where T : FileAttribute
        {
            int position = fileMFTRecordNo * _configuration.MFTRecordSize + _cryptoService.BlockSize * (_fileAttributeNo[typeof(T)] + 1);
            _fileStream.Position = position;
            int attributeSize = 0;
            using (CryptoStream crypto = _cryptoService.CreateDecryptionStream(
                _fileStream,
                CryptoStreamMode.Read,
                true,
                fileMFTRecordNo.ToString()))
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
            List<DataRun> dataRuns = ReadDataRuns(fileMFTRecordNo, _fileAttributeNo[typeof(T)]);

            byte[] bytes = new byte[attributeSize];

            int sizeLeft = attributeSize;
            foreach (var item in dataRuns)
            {
                _fileStream.Position = _configuration.MFTZoneSize + item.start * _configuration.ClusterSize;
                using (CryptoStream crypto = _cryptoService.CreateDecryptionStream(
                    _fileStream,
                    CryptoStreamMode.Read,
                    true,
                    fileMFTRecordNo.ToString()))
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

        public FileModel CreateDirectory(string directoryName, FileModel parentDirectory)
        {
            FileModel newDirectory = new FileModel(
                _mftBitMap.GetSpace(1).First().start,
                parentDirectory.MFTRecordNo,
                  false,
                new FileNameAttribute(directoryName, 0, ""),
                new HistoryAttribute(),
                new DirectoryAttribute()

                );
            WriteAttribute(_mft, _mftBitMap);
            WriteFile(newDirectory);
            parentDirectory.DirectoryAttribute.Files.Add(newDirectory);
            WriteAttribute(parentDirectory, parentDirectory.DirectoryAttribute);
            return newDirectory;
        }

        public FileModel ReadFileHeader(int fileMFTRecordNo)
        {

            _fileStream.Position = fileMFTRecordNo * _configuration.MFTRecordSize;
            FileModel res;
            using (CryptoStream crypto = _cryptoService.CreateDecryptionStream(
                _fileStream,
                CryptoStreamMode.Read,
                true,
                fileMFTRecordNo.ToString()))
            {
                using (BinaryReader reader = new BinaryReader(crypto, Encoding.Default, true))
                {
                    int mftRecordNo = reader.ReadInt32();
                    int parentDirectoryMftNo = reader.ReadInt32();
                    bool isDirectory = reader.ReadBoolean();
                    res = new FileModel(mftRecordNo, parentDirectoryMftNo, true, isDirectory);
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
                WriteAttribute(file, file.DirectoryAttribute);
            }
            else
            {
                WriteAttribute(file, file.DataAttribute);
            }

            WriteAttribute(file, file.FileNameAttribute);
            WriteAttribute(file, file.HistoryAttribute);
        }
        void WriteFileHeader(FileModel file)
        {
            _fileStream.Position = file.MFTRecordNo * _configuration.MFTRecordSize;
            using (CryptoStream crypto = _cryptoService.CreateEncryptionStream(
                _fileStream,
                CryptoStreamMode.Write,
                true,
                 file.MFTRecordNo.ToString()))
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
        void WriteAttribute(FileModel file, FileAttribute attribute)
        {
            WriteAttribute(file, _fileAttributeNo[attribute.GetType()], attribute);
        }
        void WriteAttribute(FileModel file, int attributeNo, FileAttribute attribute)
        {
            int position = file.MFTRecordNo * _configuration.MFTRecordSize + (attributeNo + 1) * _cryptoService.BlockSize;
            if (position > (file.MFTRecordNo + 1) * _configuration.MFTRecordSize)
            {
                throw new ArgumentOutOfRangeException("Попытка записи в соседнюю MFT запись");
            }

            MemoryStream attributeMemoryStream = attribute.GetDataAsStream();


            List<DataRun> dataRuns;
            int sizeInClusters = (int)(PaddingToBlockSize(attributeMemoryStream.Length) / _configuration.ClusterSize) +
              (PaddingToBlockSize(attributeMemoryStream.Length) % _configuration.ClusterSize == 0 ? 0 : 1);
            if (sizeInClusters == 0)
            {
                sizeInClusters = 1;
            }

            if (file.IsWritten)
            {
                dataRuns = ReadDataRuns(file.MFTRecordNo, attributeNo);
                int allocatedSize = dataRuns.Sum(e => e.size);
                // Уравнение выделеного и реального размера в кластерах для нерезидентного атрибута.
                // Т.К. в системе отсутствует возможность изменения хранимых файлов не сработает
                if (allocatedSize < sizeInClusters)
                {
                    dataRuns.AddRange(_bitMapBitMap.GetSpace(sizeInClusters - allocatedSize));
                }
                else if (allocatedSize > sizeInClusters)
                {
                    int pointer = dataRuns.Count - 1;
                    int sizeToFree = allocatedSize - sizeInClusters;


                    while (sizeToFree != 0)
                    {
                        if (dataRuns[pointer].size < sizeToFree)
                        {
                            sizeToFree -= dataRuns[pointer].size;
                            _bitMapBitMap.FreeSpace(dataRuns[pointer]);
                            dataRuns.RemoveAt(pointer);
                            pointer--;
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
                WriteAttribute(_bitMap, 0, _bitMapBitMap);
            }
            WriteAttributeFromStream(file.MFTRecordNo, attributeNo, _fileAttributesId.Where(e => e.Value == attribute.GetType()).First().Key, attributeMemoryStream, dataRuns);

        }

        private void WriteAttributeFromStream(int mftNo, int attributeNo, int attributeId, Stream attributeStream, List<DataRun> dataRuns)
        {
            _fileStream.Position = mftNo * _configuration.MFTRecordSize + (attributeNo + 1) * _cryptoService.BlockSize;

            using (CryptoStream crypto = _cryptoService.CreateEncryptionStream(
                _fileStream,
                CryptoStreamMode.Write,
                true,
                 mftNo.ToString()))
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
                using (CryptoStream crypto = _cryptoService.CreateEncryptionStream(
                    _fileStream,
                    CryptoStreamMode.Write,
                    true,
                     mftNo.ToString()))
                {
                    CopyPartOfStream(attributeStream, crypto, item.size * _configuration.ClusterSize);
                }
            }
        }
        List<DataRun> ReadDataRuns(int mftNo, int attributeNo)
        {
            _fileStream.Position = mftNo * _configuration.MFTRecordSize + (attributeNo + 1) * _cryptoService.BlockSize;

            List<DataRun> dataRuns = new List<DataRun>();
            using (CryptoStream crypto = _cryptoService.CreateDecryptionStream(
                _fileStream,
                CryptoStreamMode.Read,
                true,
                 mftNo.ToString()))
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

            return value + ((_cryptoService.BlockSize / 8) - value % (_cryptoService.BlockSize / 8));
        }



        public void DeleteFile(int fileMFTRecordNo)
        {
            FileModel fileModel = ReadFileHeader(fileMFTRecordNo);
            FileModel parrentDirectory = ReadFileHeader(fileModel.ParentDirectoryRecordNo);
            parrentDirectory.DirectoryAttribute = ReadFileAttribute<DirectoryAttribute>(fileModel.ParentDirectoryRecordNo);
            parrentDirectory.DirectoryAttribute.Files.RemoveAll(e => e.MFTRecordNo == fileMFTRecordNo);
            WriteAttribute(parrentDirectory, parrentDirectory.DirectoryAttribute);

            DeleteSubFiles(fileMFTRecordNo);

            WriteAttribute(_mft, _mftBitMap);
            WriteAttribute(_bitMap, _bitMapBitMap);
        }

        public void DeleteSubFiles(int fileMFTRecordNo)
        {
            FileModel fileModel = ReadFileHeader(fileMFTRecordNo);
            // Запрет на удаление системных файлов.
            if (fileMFTRecordNo < _lastSystemMftNo)
            {
                return;
            }
            if (!fileModel.IsWritten)
            {
                return;
            }
            if (fileModel.IsDirectory)
            {
                DirectoryAttribute directoryAttribute = ReadFileAttribute<DirectoryAttribute>(fileMFTRecordNo);
                foreach (var item in directoryAttribute.Files)
                {
                    DeleteSubFiles(item.MFTRecordNo);
                }
            }
            DeleteDataRuns(ReadDataRuns(fileMFTRecordNo, 0));
            DeleteDataRuns(ReadDataRuns(fileMFTRecordNo, 1));
            DeleteDataRuns(ReadDataRuns(fileMFTRecordNo, 2));

            _mftBitMap.FreeSpace(new DataRun() { start = fileMFTRecordNo, size = 1 });
            WriteRandomData(fileMFTRecordNo * _configuration.MFTRecordSize, _configuration.MFTRecordSize);
        }



        private void DeleteDataRuns(List<DataRun> dataRuns)
        {
            foreach (var dataRun in dataRuns)
            {
                _bitMapBitMap.FreeSpace(dataRun);
                WriteRandomData(_configuration.MFTZoneSize + dataRun.start * _configuration.ClusterSize, dataRun.size * _configuration.ClusterSize);
            }
        }

        private void WriteRandomData(long start, int size)
        {
            Random random = new Random();
            _fileStream.Position = start;

            using (BinaryWriter writer = new BinaryWriter(_fileStream, Encoding.Default, true))
            {
                long sizeLeft = size;
                while (sizeLeft != 0)
                {
                    byte[] buffer = new byte[1024];
                    random.NextBytes(buffer);
                    _cryptoService.EncryptBlock(buffer, random.NextInt64().ToString());
                    if (sizeLeft >= buffer.Length)
                    {
                        writer.Write(buffer);
                        sizeLeft -= buffer.Length;
                    }
                    else
                    {
                        writer.Write(buffer, 0, (int)sizeLeft);
                        sizeLeft = 0;
                    }
                }
            }
        }


        private bool IsStreamInitialize()
        {
            if (_fileStream.Length != _fileStream.Length)
            {
                return false;
            }
            try
            {
                MemoryStream memoryStream = ReadFileAttribute<DataAttribute>(_bootRecordNo).GetDataAsStream();
                using (BinaryReader reader = new BinaryReader(memoryStream, Encoding.ASCII, true))
                {
                    if (reader.ReadString() != nameof(LntfsSecureFileSystemService))
                        return false;
                    if (reader.ReadInt32() != _configuration.AttributeHeaderSize)
                        return false;
                    if (reader.ReadInt32() != _configuration.MFTZoneSize)
                        return false;
                    if (reader.ReadInt32() != _configuration.ClusterSize)
                        return false;
                    if (reader.ReadInt32() != _configuration.MFTZoneSize)
                        return false;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private void Initialize()
        {
            _fileStream.Position = 0;
            _fileStream.Write(_cryptoService.EncryptBlock(new byte[_fileStream.Length], new Random().NextInt64().ToString()));



            _mftBitMap = new BitMapAttribute(_configuration.MFTZoneSize / _configuration.MFTRecordSize);
            _bitMapBitMap = new BitMapAttribute(((int)_fileStream.Length - _configuration.MFTZoneSize) / _configuration.ClusterSize);

            FileModel _dot = new FileModel(_dotRecordNo, _dotRecordNo, false, new FileNameAttribute(".", 0, ""), new HistoryAttribute(), new DirectoryAttribute());

            _mft = new FileModel(_mftRecordNo, _dotRecordNo, false, false);

            _bitMap = new FileModel(_bitMapRecordNo, _dotRecordNo, false, false);

            WriteFileHeader(_bitMap);
            WriteAttribute(_bitMap, 0, _bitMapBitMap);
            _bitMap.IsWritten = true;

            WriteFileHeader(_mft);
            WriteFile(_dot);

            _mftBitMap.GetSpace(_lastSystemMftNo);

            WriteAttribute(_mft, 0, _mftBitMap);
            _mft.IsWritten = true;


            MemoryStream dataForBootFile = new MemoryStream();
            using (BinaryWriter writer = new BinaryWriter(dataForBootFile, Encoding.ASCII, true))
            {
                writer.Write(nameof(LntfsSecureFileSystemService));
                writer.Write(_configuration.AttributeHeaderSize);
                writer.Write(_configuration.MFTZoneSize);
                writer.Write(_configuration.ClusterSize);
                writer.Write(_configuration.MFTZoneSize);
            }

            FileModel bootFile = new FileModel(_bootRecordNo,
                _dotRecordNo,
                false,
                new FileNameAttribute("boot", 0, ""),
                new HistoryAttribute(),
                new DataAttribute(dataForBootFile)
                );
            WriteFile(bootFile);
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
