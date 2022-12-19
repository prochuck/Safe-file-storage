using Safe_file_storage.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Safe_file_storage.Configurations
{
    internal class AesCryptoConfiguration : IAesConfigureation
    {
        public string Password { get; set; }

        public byte[] PasswordSalt { get; set; }

        public byte[] IV { get; set; }
    }
}
