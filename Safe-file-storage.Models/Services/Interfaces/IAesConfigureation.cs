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
        /// <summary>
        /// Пароль, из которого будет сгенерирован ключь.
        /// </summary>
        string Password { get; }
        /// <summary>
        /// Соль для пароля.
        /// </summary>
        byte[] PasswordSalt { get; }
        /// <summary>
        /// Вектор инициализации.
        /// </summary>
        byte[] IV { get; }

    }
}
