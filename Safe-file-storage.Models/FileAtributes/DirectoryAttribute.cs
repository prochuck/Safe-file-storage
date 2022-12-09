using Safe_file_storage.Models.Abstract;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Safe_file_storage.Models.FileAtributes
{
    public class DirectoryAttribute : FileAttribute
    {

        internal List<FileModel> Files { get; }

        public DirectoryAttribute()
        {
            Files = new List<FileModel>();
        }

        public DirectoryAttribute(MemoryStream stream)
        {
            stream.Position = 0;
            Files = new List<FileModel>();
            using (BinaryReader reader = new BinaryReader(stream))
            {
                while (stream.Position < reader.BaseStream.Length)
                {
                    Files.Add(new FileModel(reader.ReadInt32(), reader.ReadInt32(), reader.ReadBoolean()));
                }
            }
        }

        public override MemoryStream GetDataAsStream()
        {
            MemoryStream memoryStream = new MemoryStream();

            foreach (var item in Files)
            {
                memoryStream.Write(BitConverter.GetBytes(item.MFTRecordNo));
                memoryStream.Write(BitConverter.GetBytes(item.ParentDirectoryRecordNo));
                memoryStream.Write(BitConverter.GetBytes(item.IsDirectory));

            }

            memoryStream.Seek(0, SeekOrigin.Begin);

            return memoryStream;
        }
    }
}
