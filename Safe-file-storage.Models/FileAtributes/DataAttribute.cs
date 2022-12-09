using Safe_file_storage.Models.Abstract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace Safe_file_storage.Models.FileAtributes
{
    public class DataAttribute : FileAttribute
    {
        byte[] _data;


        public DataAttribute(MemoryStream stream)
        {
            stream.Position = 0;
            _data = stream.ToArray();
        }

        public DataAttribute(byte[] data)
        {
            _data = data;
        }
        public DataAttribute()
        { }
        public override MemoryStream GetDataAsStream()
        {
            MemoryStream res = new MemoryStream();
            if (_data is null)
            {
                return res;
            }
            res.Write(_data);

            return res;
        }
    }
}
