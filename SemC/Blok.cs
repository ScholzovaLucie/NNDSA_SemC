using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemC
{
    class Blok
    {
        private const int Size = 1_000;
        public int ID { get; set; }
        public List<byte[]> Data { get; set; }

        public Blok(int id)
        {
            ID = id;
            Data = new List<byte[]>();
        }

        public void Add(byte[] data)
        {
            Data.Add(data);
        }

        public int GetSize()
        {
            return Size;
        }

        public byte[] ConvertListToArray()
        {
            List<byte> result = new List<byte>();

            foreach (byte[] byteArray in Data)
            {
                result.AddRange(byteArray);
            }

            return result.ToArray();
        }

    }
}
