using Safe_file_storage.Models.Interfaces;
using Safe_file_storage.Models.FileAtributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safe_file_storage.Models
{
    public class FileModel
    {


        public FileModel(int MFTRecordId, int ParentMFTRecordNo,bool isWritten, FileNameAttribute fileNameAttribute, HistoryAttribute historyAttribute, DirectoryAttribute directoryAttribute)
        {
            this.MFTRecordNo = MFTRecordId;
            this.FileNameAttribute = fileNameAttribute;
            this.HistoryAttribute = historyAttribute;
            this.DirectoryAttribute = directoryAttribute;
            ParentDirectoryRecordNo = ParentMFTRecordNo;
            IsDirectory = true;
            IsWritten = isWritten;
        }
        public FileModel(int MFTRecordId, int ParentMFTRecordNo, bool isWritten, FileNameAttribute fileNameAttribute, HistoryAttribute historyAttribute, DataAttribute dataAttribute)
        {
            this.MFTRecordNo = MFTRecordId;
            this.ParentDirectoryRecordNo = ParentMFTRecordNo;
            this.FileNameAttribute = fileNameAttribute;
            this.HistoryAttribute = historyAttribute;
            this.DataAttribute = dataAttribute;
            IsDirectory = false;
            IsWritten = isWritten;
        }

        public FileModel(int MFTRecordId, int ParentMFTRecordNo, bool isWritten,  bool isDirectory)
        {
            this.MFTRecordNo = MFTRecordId;
            IsDirectory = isDirectory;
            ParentDirectoryRecordNo = ParentMFTRecordNo;
            IsWritten = isWritten;
        }

        internal bool IsWritten { get; set; } = true;
        /// <summary>
        /// Номер записи в MFT
        /// </summary>
        public int MFTRecordNo { get; }
        public int ParentDirectoryRecordNo { get; }
        public bool IsDirectory { get; }
        public FileNameAttribute FileNameAttribute { get; internal set; }
        public HistoryAttribute HistoryAttribute { get; internal set; }

        /// <summary>
        /// Атрибут директории. null если IsDirectory = false.
        /// </summary>
        public DirectoryAttribute? DirectoryAttribute { get; internal set; }
        /// <summary>
        /// Атрибут данных файла. null если IsDirectory = true. 
        /// </summary>
        public DataAttribute? DataAttribute { get; }
    }



}
