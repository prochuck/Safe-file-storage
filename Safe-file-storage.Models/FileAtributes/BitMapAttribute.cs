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

        BitArray _bitMap;

        public BitMapAttribute(int size)
        {
            Size = size;
            SpaceLeft = size;
            _bitMap = new BitArray(size);

        }

        public int Size { get; }
        public int SpaceLeft { get; private set; }




        /// <summary>
        /// Запрос на выделение области в битовой карте.
        /// </summary>
        /// <param name="size">Размер запрашиваемой области</param>
        /// <returns>Массив отрезков, выделеных на запрос</returns>
        internal DataRun[] GetSpace(int size)
        {
            if (SpaceLeft < size)
            {
                return null;
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

            return res.ToArray();
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

        public MemoryStream GetDataAsStream()
        {
            byte[] res = new byte[_bitMap.Length / 8 + (_bitMap.Length % 8 == 0 ? 0 : 1)];
            _bitMap.CopyTo(res, 0);

            return new MemoryStream(res);
        }
    }
    internal struct DataRun
    {
        public int start;
        public int size;
    }
}
