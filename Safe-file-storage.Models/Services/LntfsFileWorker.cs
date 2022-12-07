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



        }
    }
}
