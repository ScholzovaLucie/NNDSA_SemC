using SemB.Generator;
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
        private int blockSize = 100;
        private IDataGenerator<byte> generator;

        public HeapFile(string filePath, int bufferSize, int totalRecords, IDataGenerator<byte> dataGenerator)
        {
            this.filePath = filePath;
            this.bufferSize = bufferSize;
            this.totalRecords = totalRecords;
            this.generator = dataGenerator;
        }

        public async Task WriteRecordsAsync(int bufferCount)
        {
            using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, true);
            Queue<Buffer> buffers = new Queue<Buffer>();
            for (int i = 0; i < bufferCount; i++)
            {
                buffers.Enqueue(new Buffer(bufferSize));
            }

            int totalDataSize = 0;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            for (int i = 0; i < totalRecords; i++)
            {
                byte[] data = Encoding.UTF8.GetBytes($"Record {i}\n");
                Buffer currentBuffer = buffers.Dequeue();
                buffers.Enqueue(currentBuffer);

                if (!currentBuffer.isFull())
                {
                    currentBuffer.Add(generator.Next());
                }

                byte[] toWrite = currentBuffer.ToArray();
                await fs.WriteAsync(toWrite, 0, toWrite.Length);
                totalDataSize += toWrite.Length;
                currentBuffer.Clear();  
            }

            stopwatch.Stop();
            Console.WriteLine($"Zápis trvání: {stopwatch.ElapsedMilliseconds} ms");

            foreach (var buffer in buffers)
            {
                await FlushBufferAsync(buffer, fs);
            }
        }

        private async Task FlushBufferAsync(Buffer buffer, FileStream fs)
        {
            if (buffer.Count > 0)
            {
                byte[] data = buffer.ToArray();
                await fs.WriteAsync(data, 0, data.Length);
                buffer.Clear();
            }
        }

        public async Task ReadRecordsAsync(int bufferCount)
        {
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, true);
            Queue<Buffer> buffers = new Queue<Buffer>();
            for (int i = 0; i < bufferCount; i++)
            {
                buffers.Enqueue(new Buffer(bufferSize));
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
