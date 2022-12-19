using Safe_file_storage.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safe_file_storage.Configurations
{
    internal class LntfsConfiguration : ILntfsConfiguration
    {
        public string FilePath { get; set; }

        public int MFTRecordSize { get; set; }

        public int ClusterSize { get; set; }

        public int AttributeHeaderSize { get; set; }

        public int MFTZoneSize { get; set; }

        public int FileSize { get; set; }
    }
}
