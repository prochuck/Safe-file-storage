using Safe_file_storage.Interfaces;
using Safe_file_storage.Models.FileAtributes;
using Safe_file_storage.Models.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safe_file_storage.Models.Services
{
    internal class LntfsFileWorker : IFileWorker, IDisposable
    {
        const int _mftRecordNo = 0;
        const int _bitMapRecordNo = 2;


        static Dictionary<int, Type> _fileAttributesId = new Dictionary<int, Type>
        {
            {1,typeof(FileNameAttribute)},
            {2,typeof(HistoryAttribute)},
            {3,typeof(DirectoryAttribute)},
            {4,typeof(DataAttribute)},
            {5,typeof(BitMapAttribute)}

        };



        FileStream _fileStream;
        BitMapAttribute _mftBitMap;

        /// <summary>
        /// Битмап файла $BITMAP. 
        /// </summary>
        BitMapAttribute _BitMapBitMap;

        ILntfsConfiguration _configuration;
        public LntfsFileWorker(ILntfsConfiguration configuration)
        {
            _configuration = configuration;

            if (!File.Exists(_configuration.FilePath))
            {
                throw new FileNotFoundException(null, _configuration.FilePath);
            }

            _fileStream = File.Open(configuration.FilePath, FileMode.Open);

            _mftBitMap = ReadFileAttribute<BitMapAttribute>(_mftRecordNo);
            _BitMapBitMap = ReadFileAttribute<BitMapAttribute>(_bitMapRecordNo);

        }

        public void Dispose()
        {
            _fileStream.Dispose();
        }

        public void ExportFile(int fileMFTRecordId, string targetFileName)
        {
            throw new NotImplementedException();
        }

        public FileModel ImportFile(string targetFileName)
        {
            throw new NotImplementedException();
        }

        public IFileAttribute ReadFileAttribute<IFileAttribute>(int fileMFTRecordId)
        {
            throw new NotImplementedException();
        }

        public FileModel ReadFileHeader(int fileMFTRecordId)
        {
            throw new NotImplementedException();
        }

        public FileModel ReadFileHeaderAndAttributes(int fileMFTRecordId)
        {
            throw new NotImplementedException();
        }

        public void WriteFile(FileModel file)
        {
            WriteFileHeader(file);


        }

        void WriteFileHeader(FileModel file)
        {
            _fileStream.Position = file.MFTRecordNo * _configuration.MFTRecordSize;

            using (BinaryWriter writer = new BinaryWriter(_fileStream, Encoding.Default, true))
            {
                writer.Write(file.MFTRecordNo);
                writer.Write(file.ParentDirectory.MFTRecordNo);
                writer.Write(file.IsDirectory);

                // У каждого файла кроме MFT и BITMAP одновременно может быть тольк 3 атрибута.
                writer.Write(3);
            }
        }

        void WriteAttribute(int mftNo, int offset, IFileAttribute attribute)
        {
            int position = mftNo * _configuration.MFTRecordSize + offset;
            if (position > (mftNo + 1) * _configuration.MFTRecordSize)
            {
                throw new ArgumentOutOfRangeException("Попытка записи в соседнюю MFT запись");
            }
            _fileStream.Position = mftNo * _configuration.MFTRecordSize + offset;

            MemoryStream attributeMemoryStream = attribute.GetDataAsStream();



            List<DataRun> dataRuns = ReadDataRuns(mftNo, offset);

            int sizeInClusters = (int)(attributeMemoryStream.Length / _configuration.ClusterSize +
               attributeMemoryStream.Length % _configuration.ClusterSize == 0 ? 0 : 1);

            int allocatedSize = dataRuns.Sum(e => e.size);

            // Уравнение выделеного и реального размера в кластерах для нерезидентного атрибута.
            if (allocatedSize < sizeInClusters)
            {
                dataRuns.AddRange(_BitMapBitMap.GetSpace(sizeInClusters - allocatedSize));
            }
            else if (allocatedSize > sizeInClusters)
            {
                int pointer = dataRuns.Count - 1;
                int sizeToFree = allocatedSize - sizeInClusters;

                while (allocatedSize > sizeInClusters)
                {
                    if (dataRuns[pointer].size< sizeToFree)
                    {
                        sizeToFree-=dataRuns[pointer].size;
                        _BitMapBitMap.FreeSpace(dataRuns[pointer]);
                        dataRuns.RemoveAt(pointer);
                    }
                    else
                    {
                        _BitMapBitMap.FreeSpace(dataRuns[pointer],sizeToFree);
                        DataRun dataRun = dataRuns[pointer];
                        dataRun.size -= sizeToFree;
                        dataRuns[pointer] = dataRun;
                        sizeToFree = 0;
                    }
                }
            }

            using (BinaryWriter writer = new BinaryWriter(_fileStream, Encoding.Default, true))
            {
                writer.Write(_fileAttributesId.Where(e => e.Value == attribute.GetType()).First().Key);
            }

            foreach (var item in dataRuns)
            {
                _fileStream.Position = item.start * _configuration.ClusterSize;
                attributeMemoryStream.CopyTo(_fileStream, item.size);
            }

        }

        List<DataRun> ReadDataRuns(int mftNo, int offset)
        {
            _fileStream.Position = mftNo * _configuration.MFTRecordSize + offset + 4;

            List<DataRun> dataRuns = new List<DataRun>();
            using (BinaryReader reader = new BinaryReader(_fileStream))
            {
                int dataRunsCount = reader.ReadInt32();
                for (int i = 0; i < dataRunsCount; i++)
                {
                    int dataRunStart = reader.ReadInt32();
                    int dataRunSize = reader.ReadInt32();
                    dataRuns.Add(new DataRun() { start = dataRunStart, size = dataRunSize });
                }
            }
            return dataRuns;
        }

        /// Структура файла
        /// 0  | MFTRecordNo
        /// 4  | Parent Directory MFTRecordNo
        /// 8  | Is Directory
        /// 9  | Attibute Count
        /// 13 | Padding(size 1)
        /// 14 | Attribute № 1 id (from _fileAttributesId)
        /// 18 | DataRuns count
        /// 24 | DataRun № 1 start
        /// 28 | DataRun № 1 size
        /// ...
        /// 14 +  N * _configuration.AttributeHeaderSize | Attribute № N 
    }
}
