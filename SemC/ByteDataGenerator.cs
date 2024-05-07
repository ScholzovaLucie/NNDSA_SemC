using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemB.Generator
{
    public class ByteDataGenerator: IDataGenerator<byte>
    {
        Random rng = new Random();
        public ByteDataGenerator()
        {

        }

        public byte Next()
        {
            return (byte)rng.Next();
        }
    }
}
