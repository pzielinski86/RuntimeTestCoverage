using System.Collections.Concurrent;
using System.Collections.Generic;

namespace TestCoverage.Extensions
{
    public static class ConcurrentExtensions
    {
        public static void AddRange<T>(this ConcurrentBag<T> bag, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                bag.Add(item);
            }
        }

        public static void EnqueueRange<T>(this ConcurrentQueue<T> bag, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                bag.Enqueue(item);
            }
        }
    }
}