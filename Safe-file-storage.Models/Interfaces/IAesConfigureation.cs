using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Safe_file_storage.Models.Interfaces
{
    public interface IAesConfigureation
    {
        SecureString Password { get; }
        byte[] PasswordSalt { get; }
        /// <summary>
        /// Вектор инициализации. Должен быть длинной 8.
        /// </summary>
        byte[] IV { get; }

    }
}
