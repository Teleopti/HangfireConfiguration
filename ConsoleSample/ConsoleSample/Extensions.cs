using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleSample
{
    internal static class Extensions
    {
        internal static bool IsEmpty<T>(this IEnumerable<T> source)
        {
            return !source.Any();
        }
        
        internal static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> act)
        {
            foreach (T item in source)
            {
                act(item);
            }

            return source;
        }
        
    }
}