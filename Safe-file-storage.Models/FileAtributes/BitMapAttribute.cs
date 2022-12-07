using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safe_file_storage.Models.FileAtributes
{
    public class BitMapAttribute
    {

        public BitArray BitMap { get; set; }

        public int Size { get; }

        public MemoryStream GetDataAsStream()
        {
            byte[] res = new byte[BitMap.Length / 8];

            return new MemoryStream();
        }
    }
}
