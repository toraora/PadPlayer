using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PadLogic.Utilities
{
    public class Heap<T> : List<T>
    {
        private int m_size;

        public Heap(int size)
            : base(size)
        {
            m_size = size;
        }

        public void Enqueue(T item)
        {
             
        }
    }
}
