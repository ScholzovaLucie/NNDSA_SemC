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
        // Privátní proměnné pro uložení cesty k souboru, velikosti bufferu, celkového počtu záznamů a generátoru dat
        private string filePath;
        private int bufferSize;
        private int totalRecords;
        private IDataGenerator<byte[]> generator;

        // Konstruktor pro inicializaci třídy HeapFile
        public HeapFile(string filePath, int bufferSize, int totalRecords, IDataGenerator<byte[]> dataGenerator)
        {
            this.filePath = filePath;
            this.bufferSize = bufferSize;
            this.totalRecords = totalRecords;
            this.generator = dataGenerator;
        }

        // Asynchronní metoda pro zápis záznamů s použitím bufferů
        public async Task WriteRecordsAsync(int bufferCount)
        {
            int currentRecord = 0;
            Queue<Buffer> buffers = new Queue<Buffer>();
            // Inicializace bufferů pro zápis
            for (int i = 0; i < bufferCount; i++)
            {
                buffers.Enqueue(new Buffer(bufferSize, "buffer" + i));
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start(); // Start měření času zápisu

            Buffer currentBuffer = buffers.Dequeue();

            while (currentRecord < totalRecords)
            {
                if (currentBuffer.isFull())
                {
                    if (bufferCount > 1)
                        Console.WriteLine(currentBuffer.ToString());
                    await Write(currentBuffer); // Asynchronní zápis plného bufferu
                    buffers.Enqueue(currentBuffer); // Vrácení bufferu do fronty po zápisu
                    currentBuffer = buffers.Dequeue(); // Získání nového bufferu
                    currentBuffer.Clear(); // Vyčištění bufferu pro další použití
                }

                Blok blok = new Blok(currentRecord);
                // Přidání dat do bloku
                for (int j = 0; j < Blok.GetSize(); j++)
                {
                    blok.Add(generator.Next());
                    currentRecord++;
                }
                currentBuffer.Add(blok); // Přidání bloku do bufferu
            }

            // Zajištění zápisu posledního bufferu, pokud není prázdný
            if (!currentBuffer.isEmpty())
            {
                await Write(currentBuffer);
                currentBuffer.Clear();
            }

            stopwatch.Stop(); // Zastavení časovače
            Console.WriteLine($"Zápis trvání: {stopwatch.ElapsedMilliseconds} ms");

            // Vyprázdnění zbývajících bufferů, pokud je to nutné
            foreach (var buffer in buffers)
            {
                if (!buffer.isEmpty())
                {
                    await Write(buffer);
                    buffer.Clear();
                }
            }
        }

        // Asynchronní zápis dat z bufferu do souboru
        private async Task Write(Buffer currentBuffer)
        {
            using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, true);
            byte[] toWrite = currentBuffer.ConvertListToArray();
            await fs.WriteAsync(toWrite, 0, toWrite.Length);
            currentBuffer.Clear();
        }

        // Asynchronní metoda pro čtení záznamů
        public async Task ReadRecordsAsync(int bufferCount)
        {
            Queue<Buffer> buffers = new Queue<Buffer>();
            for (int i = 0; i < bufferCount; i++)
            {
                buffers.Enqueue(new Buffer(bufferSize, "buffer" + i));
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start(); // Start měření času čtení

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

                    ProcessData(dataToProcess); // Zpracování dat

                    readyBuffer.Clear();

                    buffers.Enqueue(readyBuffer);
                }
            }

            stopwatch.Stop(); // Zastavení časovače
            Console.WriteLine($"Čtení trvání: {stopwatch.ElapsedMilliseconds} ms");

            // Vyprázdnění všech zbývajících bufferů
            foreach (var buffer in buffers)
            {
                if (buffer.IsReady())
                {

                    buffer.Clear();
                }
            }
        }

        // Metoda pro zpracování dat
        private void ProcessData(byte[] data)
        {
            var dataString = BitConverter.ToString(data);
            Console.WriteLine($"Data: {dataString}");
        }
    }
}
