using Safe_file_storage.Models.Interfaces;
using Safe_file_storage.Models.Services;
using Safe_file_storage.Models.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safe_file_storage.Models
{
    public class FileBrowserFactory
    {
        public static FileBrowserModel CreateInstanceFromFile(
            string filePath,
            int fileSize,
            ILntfsConfiguration lntfsConfiguration,
            IAesConfigureation cryptoConfigureation)
        {
            ICryptoService cryptoService = new AesCryptoService(cryptoConfigureation);
            IFileSystemStream fileSystemStream = new FileFileSystemStream(filePath, fileSize);
            IFileSystemService fileSystemService = new LntfsSecureFileSystemService(fileSystemStream, lntfsConfiguration, cryptoService);

            return new FileBrowserModel(fileSystemService);
        }
    }
}
