using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace Safe_file_storage.Models.Interfaces
{
    public interface ILntfsConfiguration
    {
        int MFTRecordSize { get; }
        /// <summary>
        /// Размер кластера байтах.
        /// </summary>
        int ClusterSize { get; }
        /// <summary>
        /// Размер заголовка атрибута.
        /// </summary>
        int AttributeHeaderSize { get; }
        /// <summary>
        /// Размер зоны mft.
        /// </summary>
        int MFTZoneSize { get; }
        /// <summary>
        /// Размер зоны mft.
        /// </summary>

    }
}
