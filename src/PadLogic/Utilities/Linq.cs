using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PadLogic.Utilities
{
    public static class PadLinq
    {
        public static T MinBy<T, TResult>(this IEnumerable<T> e, Func<T, TResult> f) where TResult : IComparable
        {
            if (e.Count() == 0)
                throw new IndexOutOfRangeException("Enumerable has no elements!");

            T minElement = e.First();
            TResult min = f(minElement);

            foreach (T elem in e.Skip(1))
            {
                TResult cur = f(elem);
                if (cur.CompareTo(min) < 0)
                {
                    min = cur;
                    minElement = elem;
                }
            }

            return minElement;
        }
    }
}
