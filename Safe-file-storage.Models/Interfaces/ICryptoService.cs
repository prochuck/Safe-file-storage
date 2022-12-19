using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Safe_file_storage.Models.Interfaces
{
    public interface ICryptoService : IDisposable
    {
        /// <summary>
        /// Размер блока шифрования.
        /// </summary>
        public int BlockSize { get; }
        /// <summary>
        /// Создать CryptoStram для шифрования.
        /// </summary>
        public CryptoStream CreateEncryptionStream(Stream stream, CryptoStreamMode mode, bool leavOpen);
        /// <summary>
        /// Создать CryptoStram для шифрования с вектором инициализаии, который будет получен хэшированием строки.
        /// </summary>
        public CryptoStream CreateEncryptionStream(Stream stream, CryptoStreamMode mode, bool leavOpen, string IV);

        /// <summary>
        /// Создать CryptoStram для расшифрования.
        /// </summary>
        public CryptoStream CreateDecryptionStream(Stream stream, CryptoStreamMode mode, bool leavOpen);

        /// <summary>
        /// Создать CryptoStram для расшифрования с вектором инициализаии, который будет получен хэшированием строки.
        /// </summary>
        public CryptoStream CreateDecryptionStream(Stream stream, CryptoStreamMode mode, bool leavOpen, string IV);

        /// <summary>
        /// Зашифровать данные с использованием вектора инициализации.
        /// </summary>
        public byte[] EncryptBlock(byte[] data, byte[] IV);
        /// <summary>
        /// Зашифровать данные с использованием вектора инициализации, который будет получен хэшированием строки.
        /// </summary>
        public byte[] EncryptBlock(byte[] data, string IV);
    }
}
