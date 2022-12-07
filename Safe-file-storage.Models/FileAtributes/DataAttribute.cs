using Safe_file_storage.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safe_file_storage.Models.FileAtributes
{
    public class DataAttribute : IFileAttribute
    {
        byte[] _data;

        public  MemoryStream GetDataAsStream()
        {
            return new MemoryStream(_data);
        }
    }
}
