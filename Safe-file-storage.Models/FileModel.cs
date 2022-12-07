using Safe_file_storage.Interfaces;
using Safe_file_storage.Models.FileAtributes;
using Safe_file_storage.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safe_file_storage.Models
{
    public class FileModel
    {

        
        public FileModel(int MFTRecordId, FileModel parentDirectory, FileNameAttribute fileNameAttribute, HistoryAttribute historyAttribute, DirectoryAttribute directoryAttribute)
        {
            this.MFTRecordId = MFTRecordId;
            this.ParentDirectory = parentDirectory;
            this.FileNameAttribute = fileNameAttribute;
            this.HistoryAttribute = historyAttribute;
            this.DirectoryAttribute = directoryAttribute;
            IsDirectory = true;
        }
        public FileModel(int MFTRecordId, FileModel parentDirectory, FileNameAttribute fileNameAttribute, HistoryAttribute historyAttribute, DataAttribute dataAttribute)
        {
            this.MFTRecordId = MFTRecordId;
            this.ParentDirectory = parentDirectory;
            this.FileNameAttribute = fileNameAttribute;
            this.HistoryAttribute = historyAttribute;
            this.DataAttribute = dataAttribute;
        }

        public FileModel(int MFTRecordId)
        {
            this.MFTRecordId = MFTRecordId;
        }

        /// <summary>
        /// Номер записи в MFT
        /// </summary>
        public int MFTRecordId { get; }
        public FileModel ParentDirectory { get; internal set; }
        public bool IsDirectory { get; }
        public FileNameAttribute FileNameAttribute { get; }
        public HistoryAttribute HistoryAttribute { get; }

        /// <summary>
        /// Атрибут директории. null если IsDirectory = false.
        /// </summary>
        public DirectoryAttribute? DirectoryAttribute { get; }
        /// <summary>
        /// Атрибут данных файла. null если IsDirectory = true. 
        /// </summary>
        public DataAttribute? DataAttribute { get; }
    }



}
