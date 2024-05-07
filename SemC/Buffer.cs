using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Reflection.Metadata.BlobBuilder;

namespace SemC
{
    class Buffer
    {
        private string identifire;
        private List<Blok> blocks;
        private int capacity;

        // Konstruktor pro inicializaci kapacity bufferu
        public Buffer(int capacity, string identifire)
        {
            this.capacity = capacity;
            blocks = new List<Blok>(capacity);
            this.identifire = identifire;
        }

        // Metoda pro přidání prvku do bufferu
        public void Add(Blok item)
        {
            if (blocks.Count >= capacity)
                throw new InvalidOperationException("Buffer je plný.");
            blocks.Add(item);
        }
        public bool isEmpty()
        {
            return blocks.Count == 0;
        }

        // Metoda pro vyčištění bufferu
        public void Clear()
        {
            blocks.Clear();
        }

        // Metoda pro zjištění počtu prvků v bufferu
        public int Count => blocks.Count;

        public bool isFull()
        {
            return blocks.Count * Blok.GetSize() >= capacity;  // Upraveno dle velikosti bloku definované ve třídě Blok
        }

        public bool IsReady()
        {
            return blocks.Count > 0;
        }

        public byte[] ConvertListToArray()
        {
            List<byte> result = new List<byte>();

            foreach (Blok byteArray in blocks)
            {
                result.AddRange(byteArray.ConvertListToArray());
            }

            return result.ToArray();
        }

        public string ToString()
        {
            return $"Buffer {identifire}: {blocks.Count} blocks";
        }

    }

}
