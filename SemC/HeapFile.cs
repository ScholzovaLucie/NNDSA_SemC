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
            Queue<Buffer> buffers = new Queue<Buffer>();
            for (int i = 0; i < bufferCount; i++)
            {
                buffers.Enqueue(new Buffer(bufferSize, "buffer" + i));
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            for (int i = 0; i < totalRecords; i++)
            {
                Buffer currentBuffer = buffers.Dequeue();
                buffers.Enqueue(currentBuffer);

                while (!currentBuffer.isFull())
                {
                    Blok blok = new Blok(i);

                    for (int j = 0; j < blok.GetSize(); j++)
                    {
                        blok.Add(generator.Next());

                    }
                    currentBuffer.Add(blok);
                }

                Console.WriteLine(currentBuffer.ToString());

                await Write(currentBuffer);
            }

            stopwatch.Stop();
            Console.WriteLine($"Zápis trvání: {stopwatch.ElapsedMilliseconds} ms");

            foreach (var buffer in buffers)
            {
                await FlushBufferAsync(buffer);
            }
        }

        private async Task Write(Buffer currentBuffer)
        {
            using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, true);

            byte[] toWrite = currentBuffer.ConvertListToArray();
            await fs.WriteAsync(toWrite, 0, toWrite.Length);
            currentBuffer.Clear();
        }

        private async Task FlushBufferAsync(Buffer buffer)
        {
            if (buffer.Count > 0)
            {
                buffer.Clear();
            }
        }

        public async Task ReadRecordsAsync(int bufferCount)
        {
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, true);


            Queue<Buffer> buffers = new Queue<Buffer>();
            for (int i = 0; i < bufferCount; i++)
            {
                buffers.Enqueue(new Buffer(bufferSize, "buffer" + i));
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            int bytesRead, totalReadSize = 0;
            byte[] byteData = new byte[bufferSize];

            while ((bytesRead = await fs.ReadAsync(byteData, 0, bufferSize)) > 0)
            {
                Buffer currentBuffer = buffers.Dequeue();
                buffers.Enqueue(currentBuffer);
                currentBuffer.Clear();

                currentBuffer.AddRange(byteData.Take(bytesRead).ToArray());

                totalReadSize += bytesRead;
            }

            stopwatch.Stop();
            Console.WriteLine($"Načtení trvání: {stopwatch.ElapsedMilliseconds} ms");
        }

        
    }
}
