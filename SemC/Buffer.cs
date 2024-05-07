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
        private List<byte> items;
        private int capacity;

        // Konstruktor pro inicializaci kapacity bufferu
        public Buffer(int capacity)
        {
            this.capacity = capacity;
            items = new List<byte>(capacity);
        }

        // Metoda pro přidání prvku do bufferu
        public void Add(byte item)
        {
            if (items.Count >= capacity)
                throw new InvalidOperationException("Buffer je plný.");
            items.Add(item);
        }

        public void AddRange(byte[] data)
        {
            if (items.Count + data.Length > capacity)
                throw new InvalidOperationException("Přidání dat překročí kapacitu bufferu.");
            items.AddRange(data);
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

        // Metoda pro získání obsahu bufferu jako pole
        public byte[] ToArray()
        {
            return items.ToArray();
        }


    }

}
