using System;
using System.Collections.Generic;
using System.Linq;

namespace Hangfire.Configuration.Test
{
    public static class Extensions
    {
        public static void Times(this int times, Action action)
        {
            Enumerable.Range(0, times).ForEach(i => action());
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