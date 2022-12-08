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


        public FileModel(int MFTRecordId, int ParentMFTRecordNo, FileNameAttribute fileNameAttribute, HistoryAttribute historyAttribute, DirectoryAttribute directoryAttribute)
        {
            this.MFTRecordNo = MFTRecordId;
            this.FileNameAttribute = fileNameAttribute;
            this.HistoryAttribute = historyAttribute;
            this.DirectoryAttribute = directoryAttribute;
            ParentDirectoryRecordNo = ParentMFTRecordNo;
            IsDirectory = true;
        }
        public FileModel(int MFTRecordId, int ParentMFTRecordNo, FileNameAttribute fileNameAttribute, HistoryAttribute historyAttribute, DataAttribute dataAttribute)
        {
            this.MFTRecordNo = MFTRecordId;
            this.ParentDirectoryRecordNo = ParentMFTRecordNo;
            this.FileNameAttribute = fileNameAttribute;
            this.HistoryAttribute = historyAttribute;
            this.DataAttribute = dataAttribute;
            IsDirectory = false;
        }

        public FileModel(int MFTRecordId, int ParentMFTRecordNo, bool isDirectory)
        {
            this.MFTRecordNo = MFTRecordId;
            IsDirectory = isDirectory;
            ParentDirectoryRecordNo = ParentMFTRecordNo;
        }


        internal bool IsWritten { get; set; } = false;
        /// <summary>
        /// Номер записи в MFT
        /// </summary>
        public int MFTRecordNo { get; }
        public int ParentDirectoryRecordNo { get; }
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
