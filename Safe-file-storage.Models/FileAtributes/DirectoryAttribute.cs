using Safe_file_storage.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Safe_file_storage.Models.FileAtributes
{
    public class DirectoryAttribute : IFileAttribute
    {
      
        internal List<FileModel> Files { get; }

        public DirectoryAttribute()
        {
            Files = new List<FileModel>();
        }

        public MemoryStream GetDataAsStream()
        {
            MemoryStream memoryStream = new MemoryStream();

            foreach (var item in Files)
            {
                memoryStream.Write(BitConverter.GetBytes(item.MFTRecordId));
            }

            memoryStream.Seek(0, SeekOrigin.Begin);

            return memoryStream;
        }
    }
}
