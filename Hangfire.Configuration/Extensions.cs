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
        
    }
}