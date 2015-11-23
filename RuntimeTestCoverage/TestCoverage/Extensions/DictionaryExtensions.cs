using System.Collections.Generic;

namespace TestCoverage.Extensions
{
    public static class DictionaryExtensions
    {
        public static void Merge<TKey, TValue>(this IDictionary<TKey, TValue> dictionary1,
            IDictionary<TKey, TValue> dictionary2)
        {
            foreach (var key2 in dictionary2.Keys)
            {
                dictionary1[key2] = dictionary2[key2];
            }
        }         
    }
}