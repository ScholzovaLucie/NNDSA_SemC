using SemC.Generator;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemC
{
    public class HeapFile
    {
        private string filePath;
        private int bufferSize;
        private int totalRecords;
        private IDataGenerator<byte[]> generator;

        public HeapFile(string filePath, int bufferSize, int totalRecords, IDataGenerator<byte[]> dataGenerator)
        {
            this.filePath = filePath;
            this.bufferSize = bufferSize;
            this.totalRecords = totalRecords;
            this.generator = dataGenerator;
        }

        public async Task WriteRecordsAsync(int bufferCount)
        {
            int currentRecord = 0;
            Queue<Buffer> buffers = new Queue<Buffer>();
            for (int i = 0; i < bufferCount; i++)
            {
                buffers.Enqueue(new Buffer(bufferSize, "buffer" + i));
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            Buffer currentBuffer = buffers.Dequeue();

            while (currentRecord < totalRecords)
            {
                if (currentBuffer.isFull())
                {
                    await Write(currentBuffer);
                    buffers.Enqueue(currentBuffer); // Vracíme buffer do fronty po zápisu
                    currentBuffer = buffers.Dequeue(); // Získáme nový buffer na práci
                    currentBuffer.Clear(); // Zajistíme, že buffer je prázdný
                }

                Blok blok = new Blok(currentRecord);
                for (int j = 0; j < Blok.GetSize(); j++)
                {
                    blok.Add(generator.Next());
                    currentRecord++;
                }
                currentBuffer.Add(blok);
            }

            // Zajištění, že poslední buffer je také zapsán
            if (!currentBuffer.isEmpty())
            {
                await Write(currentBuffer);
                currentBuffer.Clear();
            }

            stopwatch.Stop();
            Console.WriteLine($"Zápis trvání: {stopwatch.ElapsedMilliseconds} ms");

            // Flushing buffers if necessary (should be empty if logic is correct)
            foreach (var buffer in buffers)
            {
                if (!buffer.isEmpty())
                {
                    await Write(buffer);
                    buffer.Clear();
                }
            }
        }

        private async Task Write(Buffer currentBuffer)
        {
            using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, true);

            byte[] toWrite = currentBuffer.ConvertListToArray();
            await fs.WriteAsync(toWrite, 0, toWrite.Length);
            currentBuffer.Clear();
        }

        public async Task ReadRecordsAsync(int bufferCount)
        {
            Queue<Buffer> buffers = new Queue<Buffer>();
            for (int i = 0; i < bufferCount; i++)
            {
                buffers.Enqueue(new Buffer(bufferSize, "buffer" + i));
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, true);
            byte[] data = new byte[Blok.GetSize()];
            int bytesRead;
            int recordId = 0;

            while ((bytesRead = await fs.ReadAsync(data, 0, data.Length)) > 0)
            {
                Buffer currentBuffer = buffers.Dequeue();

                Blok blok = new Blok(recordId++);
                blok.Add(data.ToArray());

                currentBuffer.Add(blok);

                buffers.Enqueue(currentBuffer);

                if (buffers.Peek().IsReady())  
                {
                    Buffer readyBuffer = buffers.Dequeue(); 


                    byte[] dataToProcess = readyBuffer.ConvertListToArray(); 

                    ProcessData(dataToProcess); 

                    readyBuffer.Clear();

                    buffers.Enqueue(readyBuffer);
                }
            }

            stopwatch.Stop();
            Console.WriteLine($"Čtení trvání: {stopwatch.ElapsedMilliseconds} ms");

            foreach (var buffer in buffers)
            {
                if (buffer.IsReady())
                {

                    buffer.Clear();
                }
            }

        }

        private void ProcessData(byte[] data)
        {
            var dataString = BitConverter.ToString(data);
            Console.WriteLine($"Data: {dataString}");
        }


    }
}
