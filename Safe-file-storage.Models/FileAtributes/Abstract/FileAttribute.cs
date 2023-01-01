using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safe_file_storage.Models.Abstract
{
    public abstract class FileAttribute
    {
        /// <summary>
        /// Получить атрибут как поток.
        /// </summary>
        /// <returns></returns>
        public abstract MemoryStream GetDataAsStream();
        public FileAttribute(MemoryStream stream) { }
        protected FileAttribute() { }

    }
}
