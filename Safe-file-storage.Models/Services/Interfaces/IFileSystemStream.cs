using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safe_file_storage.Models.Services.Interfaces
{
    public interface IFileSystemStream : IDisposable
    {
        /// <summary>
        /// Поток с которым будет работать сервис файловой системы
        /// </summary>
        Stream LntfsStream { get; }
    }
}
