using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using SemC.Generator;

namespace SemC
{
    class Program
    {
        static async Task Main(string[] args)
        {
            int bufferSize = 2_000;
            int dataSize = 1_000_000;

            string filePath = "heapfile.bin";
            var heapFile = new HeapFile(filePath, bufferSize, dataSize, new ByteDataGenerator());

            Console.WriteLine("");

            Console.WriteLine("Spouštění s jedním bufferem:");
            await heapFile.WriteRecordsAsync(1);  // Write records to the file with single buffer
            await heapFile.ReadRecordsAsync(1);   // Read records from the file with single buffer

            heapFile = new HeapFile(filePath, bufferSize, dataSize, new ByteDataGenerator());

            Console.WriteLine("\nSpouštění se dvěma buffery:");
            await heapFile.WriteRecordsAsync(2);  // Write records to the file with dual buffers
            await heapFile.ReadRecordsAsync(2);
            

           
        }

    }
}
