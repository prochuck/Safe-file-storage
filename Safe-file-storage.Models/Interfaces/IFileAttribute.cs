using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safe_file_storage.Interfaces
{
    public interface IFileAttribute
    {
        public MemoryStream GetDataAsStream();
    }
}
