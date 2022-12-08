using Safe_file_storage.Models.Abstract;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Safe_file_storage.Models.FileAtributes
{
    public class HistoryAttribute : FileAttribute
    {
        public HistoryAttribute()
        {
            _historyRecords = new List<HistoryRecord>();
            _historyRecords.Add(new HistoryRecord(DateTime.Now, HistoryRecordAction.Created));
        }

        public HistoryAttribute(MemoryStream stream)
        {
            stream.Position = 0;

            _historyRecords = new List<HistoryRecord>();

            using (BinaryReader reader = new BinaryReader(stream))
            {
                while (stream.Position < reader.BaseStream.Length)
                {
                    long time = reader.ReadInt64();
                    HistoryRecordAction action = (HistoryRecordAction)reader.ReadInt32();
                    _historyRecords.Add(new HistoryRecord(DateTime.FromBinary(time), action));
                }
            }
        }

        List<HistoryRecord> _historyRecords;
        public ReadOnlyCollection<HistoryRecord> HistoryRecords { get { return new ReadOnlyCollection<HistoryRecord>(_historyRecords); } }

        internal void AddHistoryRecord(HistoryRecord record)
        {
            _historyRecords.Add(record);
        }

        public override MemoryStream GetDataAsStream()
        {
            MemoryStream memoryStream = new MemoryStream();

            foreach (var item in HistoryRecords)
            {
                memoryStream.Write(BitConverter.GetBytes(item.Time.Ticks));
                memoryStream.Write(BitConverter.GetBytes((int)item.Action));
            }

            memoryStream.Seek(0, SeekOrigin.Begin);

            return memoryStream;
        }
    }
    public struct HistoryRecord
    {
        public HistoryRecord(DateTime time, HistoryRecordAction action)
        {
            Time = time;
            Action = action;
        }

        public DateTime Time { get; }
        public HistoryRecordAction Action { get; }


        public override string ToString()
        {
            return Action.ToString() + " " + Time.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss");
        }

    }
    public enum HistoryRecordAction
    {
        Created, Changed,
    }
}
