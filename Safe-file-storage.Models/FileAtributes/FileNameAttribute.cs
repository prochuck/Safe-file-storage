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
    public class FileNameAttribute : FileAttribute
    {
        public string Name { get; internal set; }
        public long Size { get; internal set; }
        public string Extention { get; internal set; }




        public FileNameAttribute(MemoryStream stream)
        {
            stream.Position = 0;
            using (BinaryReader reader = new BinaryReader(stream,Encoding.UTF8))
            {
                Name = reader.ReadString();
                Size = reader.ReadInt64();
                Extention = reader.ReadString();
            }
        }

        public FileNameAttribute(string name, long size, string extention)
        {
            Name = name;
            Size = size;
            Extention = extention;
        }

        public override MemoryStream GetDataAsStream()
        {
            MemoryStream memoryStream = new MemoryStream();
            using (BinaryWriter writer = new BinaryWriter(memoryStream, Encoding.UTF8,true))
            {
                writer.Write(Name);
                writer.Write(Size);
                writer.Write(Extention);
            }
               

            memoryStream.Seek(0, SeekOrigin.Begin);

            return memoryStream;
        }
    }
}
