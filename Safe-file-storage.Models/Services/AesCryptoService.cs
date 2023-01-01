using Safe_file_storage.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Safe_file_storage.Models.Services
{
    internal class AesCryptoService : ICryptoService, IDisposable
    {
        Aes _aes;
        IAesConfigureation _configureation;
        HashAlgorithm _hashAlgorithm;

        public AesCryptoService(IAesConfigureation configureation)
        {
            _aes = Aes.Create();
            byte[] hasgedPassword = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(configureation.Password),
               SHA512.HashData(configureation.PasswordSalt),
                100000,
                HashAlgorithmName.SHA256,
                256 / 8);

            _aes.Key = hasgedPassword;

            _aes.IV = MD5.HashData(configureation.IV);
            _aes.Mode = CipherMode.CBC;
            _configureation = configureation;
            _hashAlgorithm = MD5.Create();
        }

        public void Dispose()
        {
            _aes.Dispose();
            _hashAlgorithm.Dispose();
        }

        public int BlockSize
        {
            get
            {
                return _aes.BlockSize;
            }
        }


        public CryptoStream CreateDecryptionStream(Stream stream, CryptoStreamMode mode, bool leavOpen)
        {
            return CreateDecryptionStream(stream, mode, leavOpen, _configureation.IV);
        }

        CryptoStream CreateDecryptionStream(Stream stream, CryptoStreamMode mode, bool leavOpen, byte[] IV)
        {
            CryptoStream res = new CryptoStream(stream, _aes.CreateDecryptor(_aes.Key, IV), mode, leavOpen);
            return res;
        }

        public CryptoStream CreateDecryptionStream(Stream stream, CryptoStreamMode mode, bool leavOpen, string IV)
        {
            return CreateDecryptionStream(stream, mode, leavOpen, _hashAlgorithm.ComputeHash(Encoding.ASCII.GetBytes(IV)));
        }

        public CryptoStream CreateEncryptionStream(Stream stream, CryptoStreamMode mode, bool leavOpen)
        {
            return CreateEncryptionStream(stream, mode, leavOpen, _configureation.IV);
        }

        CryptoStream CreateEncryptionStream(Stream stream, CryptoStreamMode mode, bool leavOpen, byte[] IV)
        {
            CryptoStream res = new CryptoStream(stream, _aes.CreateEncryptor(_aes.Key, IV), mode, leavOpen);
            return res;
        }

        public CryptoStream CreateEncryptionStream(Stream stream, CryptoStreamMode mode, bool leavOpen, string IV)
        {
            return CreateEncryptionStream(stream, mode, leavOpen, _hashAlgorithm.ComputeHash(Encoding.ASCII.GetBytes(IV)));
        }

        public byte[] EncryptBlock(byte[] data, byte[] IV)
        {
            return _aes.CreateEncryptor(_aes.Key, IV).TransformFinalBlock(data, 0, data.Length);
        }

        public byte[] EncryptBlock(byte[] data, string IV)
        {
            return EncryptBlock(data, _hashAlgorithm.ComputeHash(Encoding.ASCII.GetBytes(IV)));
        }

    }
}
