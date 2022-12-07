using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Safe_file_storage.Interfaces;

namespace Safe_file_storage.Models.FileAtributes
{
    public class FileNameAttribute : IFileAttribute
    {
        public string Name { get; internal set; }
        public long Size { get; internal set; }
        public string Extention { get; internal set; }

        public FileModel ParentDirectory { get; internal set; }

        public MemoryStream GetDataAsStream()
        {
            MemoryStream memoryStream = new MemoryStream();

            memoryStream.Write(Encoding.ASCII.GetBytes(Name));
            memoryStream.Write(BitConverter.GetBytes(Size));
            memoryStream.Write(Encoding.ASCII.GetBytes(Extention));

            memoryStream.Seek(0, SeekOrigin.Begin);

            return memoryStream;
        }
    }
}
