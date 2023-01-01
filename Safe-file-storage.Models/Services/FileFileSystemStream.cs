using Safe_file_storage.Models.Interfaces;
using Safe_file_storage.Models.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safe_file_storage.Models.Services
{
    internal class FileFileSystemStream : IFileSystemStream
    {
      

        public FileFileSystemStream(string filePath, int fileSize)
        {
            if (!File.Exists(filePath))
            {
                File.WriteAllBytes(filePath, new byte[fileSize]);
            }
            LntfsStream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite);

        }

        public Stream LntfsStream { get; private set; }

        public void Dispose()
        {
            LntfsStream.Dispose();
        }
    }
}
