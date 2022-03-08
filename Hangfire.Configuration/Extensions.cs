using System;
using System.Collections.Generic;

namespace Hangfire.Configuration
{
    internal static class Extensions
    {
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