using Safe_file_storage.Models.Abstract;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safe_file_storage.Models.FileAtributes
{
    public class BitMapAttribute : FileAttribute
    {

        BitArray _bitMap;

        public BitMapAttribute(int size)
        {
            Size = size;
            SpaceLeft = size;
            _bitMap = new BitArray(size);

        }

        public BitMapAttribute(MemoryStream stream)
        {
            stream.Position = 0;
            using (BinaryReader reader = new BinaryReader(stream))
            {
                Size=reader.ReadInt32();
                SpaceLeft=reader.ReadInt32();
                _bitMap = new BitArray(reader.ReadBytes(Size));
            }
        }

        public int Size { get; }
        public int SpaceLeft { get; private set; }




        /// <summary>
        /// Запрос на выделение области в битовой карте.
        /// </summary>
        /// <param name="size">Размер запрашиваемой области</param>
        /// <returns>Массив отрезков, выделеных на запрос</returns>
        internal List<DataRun> GetSpace(int size)
        {
            if (SpaceLeft < size)
            {
                throw new Exception("Нет свободного места");
            }

            List<DataRun> res = new List<DataRun>();

            int sizeLeft = size;

            int pointer = 0;
            while (sizeLeft != 0)
            {
                while (_bitMap[pointer] == true)
                {
                    pointer++;
                }
                int dataRunStart = pointer;
                while (_bitMap[pointer] == false && sizeLeft != 0)
                {
                    _bitMap[pointer] = true;
                    SpaceLeft--;
                    sizeLeft--;
                    pointer++;
                }

                res.Add(new DataRun() { start = dataRunStart, size = pointer - dataRunStart });
            }

            return res;
        }

        /// <summary>
        /// Запрос на освобождение области в битовой карте.
        /// </summary>
        /// <param name="dataRun">Освобождаемая область</param>
        internal void FreeSpace(DataRun dataRun)
        {
            int pointer = dataRun.start;
            while (dataRun.size != 0)
            {
                _bitMap[pointer] = false;
                SpaceLeft++;
                dataRun.size--;
                pointer++;
            }
        }
        internal void FreeSpace(DataRun dataRun, int size)
        {
            if (dataRun.size <= size)
            {
                FreeSpace(dataRun);
                return;
            }

            int pointer = dataRun.start + dataRun.size;
            while (size != 0)
            {
                _bitMap[pointer] = false;
                SpaceLeft++;
                size--;
                pointer--;
            }
        }

        public override MemoryStream GetDataAsStream()
        {
            MemoryStream res = new MemoryStream();
            res.Write(BitConverter.GetBytes(Size));
            res.Write(BitConverter.GetBytes(SpaceLeft));
            byte[] buffer = new byte[_bitMap.Length / 8 + (_bitMap.Length % 8 == 0 ? 0 : 1)];
            _bitMap.CopyTo(buffer, 0);
            res.Write(buffer);


            res.Seek(0, SeekOrigin.Begin);
            return res;
        }
    }
    internal struct DataRun
    {
        public int start;
        public int size;
    }
}
