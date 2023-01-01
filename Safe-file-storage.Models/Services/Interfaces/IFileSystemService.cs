using Safe_file_storage.Models.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safe_file_storage.Models.Interfaces
{
    public interface IFileSystemService:IDisposable
    {
        /// <summary>
        /// Корневая директория файловой системы.
        /// </summary>
        public FileModel RootDirectory { get; }
        /// <summary>
        /// Прочитать заголовок файла и все его атрибуты.
        /// </summary>
        /// <param name="fileMFTRecordId"></param>
        /// <returns></returns>
        public FileModel ReadFileHeaderAndAttributes(int fileMFTRecordId);
        /// <summary>
        /// Прочитать только заголовок файла.
        /// </summary>
        /// <param name="fileMFTRecordId"></param>
        /// <returns></returns>
        public FileModel ReadFileHeader(int fileMFTRecordId);
        /// <summary>
        /// Прочитать атрибут файла.
        /// </summary>
        /// <typeparam name="T">Тип атрибута, который будет прочитан</typeparam>
        /// <param name="fileMFTRecordId"></param>
        /// <returns></returns>
        public T ReadFileAttribute<T>(int fileMFTRecordId)
            where T : FileAttribute;
        /// <summary>
        /// Экспорт файла из программы.
        /// </summary>
        /// <param name="fileMFTRecordId"></param>
        /// <param name="targetFilePath"></param>
        public void ExportFile(int fileMFTRecordId, string targetFilePath);
        /// <summary>
        /// Импорт файла в программу.
        /// </summary>
        /// <param name="targetFilePath">Путь к импортируемому файлу</param>
        /// <param name="directoryToWriteMFTNo">MFT номер директории, в которую будет добавлен файл</param>
        /// <returns></returns>
        public FileModel ImportFile(string targetFilePath, int directoryToWriteMFTNo);
        /// <summary>
        /// Записать файл.
        /// </summary>
        /// <param name="file"></param>
        public void WriteFile(FileModel file);
        /// <summary>
        /// Создать новую директорию.
        /// </summary>
        /// <param name="directoryName"></param>
        /// <param name="parentDirectory"></param>
        /// <returns></returns>
        public FileModel CreateDirectory(string directoryName, FileModel parentDirectory);
        /// <summary>
        /// Удалить файл.
        /// </summary>
        /// <param name="fileMFTRecordNo"></param>
        public void DeleteFile(int fileMFTRecordNo);


    }
}
