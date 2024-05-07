using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace SemC
{
    class Buffer
    {
        private string identifire;
        private List<Blok> items;
        private int capacity;

        // Konstruktor pro inicializaci kapacity bufferu
        public Buffer(int capacity, string identifire)
        {
            this.capacity = capacity;
            items = new List<Blok>(capacity);
            this.identifire = identifire;
        }

        // Metoda pro přidání prvku do bufferu
        public void Add(Blok item)
        {
            if (items.Count >= capacity)
                throw new InvalidOperationException("Buffer je plný.");
            items.Add(item);
        }


        // Metoda pro vyčištění bufferu
        public void Clear()
        {
            items.Clear();
        }

        // Metoda pro zjištění počtu prvků v bufferu
        public int Count => items.Count;

        public bool isFull()
        {
            return items.Count == capacity;
        }

        public byte[] ConvertListToArray()
        {
            List<byte> result = new List<byte>();

            foreach (Blok byteArray in items)
            {
                result.AddRange(byteArray.ConvertListToArray());
            }

            return result.ToArray();
        }

        public string ToString()
        {
            return identifire;
        }

    }

}
