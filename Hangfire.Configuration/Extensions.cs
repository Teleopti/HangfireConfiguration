using System;
using System.Collections.Generic;
using System.Linq;

namespace Hangfire.Configuration
{
    public static class Extensions
    {
        public static bool IsEmpty<T>(this IEnumerable<T> source)
        {
            return !source.Any();
        }
        
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> act)
        {
            foreach (T item in source)
            {
                act(item);
            }

            return source;
        }
        
    }
}