using System;
using System.Collections.Generic;

namespace ConsoleSample
{
    internal static class Extensions
    {
        internal static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> act)
        {
            foreach (var item in source)
            {
                act(item);
            }

            return source;
        }
    }
}