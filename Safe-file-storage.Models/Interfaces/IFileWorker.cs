using Safe_file_storage.Interfaces;
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
        /// <param name="targetFileName"></param>
        public void ExportFile(int fileMFTRecordId, string targetFileName);

        public FileModel ImportFile(string targetFileName);

        public void WriteFile(FileModel file);

    }
}
