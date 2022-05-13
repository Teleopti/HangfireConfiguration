using System;
using System.Collections.Generic;

namespace ConsoleSample
{
    internal static class Extensions
    {
	    internal static void ForEach<T>(this IEnumerable<T> source, Action<T> act)
        {
            foreach (T item in source)
                act(item);
        }
    }
}