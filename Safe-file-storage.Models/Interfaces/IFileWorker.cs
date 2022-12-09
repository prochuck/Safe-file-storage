using Safe_file_storage.Models.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safe_file_storage.Models.Interfaces
{
    public interface IFileWorker
    {
        public FileModel ReadFileHeaderAndAttributes(int fileMFTRecordId);

        public FileModel ReadFileHeader(int fileMFTRecordId);
        public T ReadFileAttribute<T>(int fileMFTRecordId)
            where T : FileAttribute;
        /// <summary>
        /// Экспорт файла из программы.
        /// </summary>
        /// <param name="fileMFTRecordId"></param>
        /// <param name="targetFilePath"></param>
        public void ExportFile(int fileMFTRecordId, string targetFilePath);

        public FileModel ImportFile(string targetFilePath, int directoryToWriteMFTNo);

        public void WriteFile(FileModel file);

        public FileModel RootDirectory { get; }
    }
}
