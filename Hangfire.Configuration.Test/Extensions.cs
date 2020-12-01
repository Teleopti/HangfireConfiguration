using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;

namespace Hangfire.Configuration.Test
{
    public static class Extensions
    {
        public static T Copy<T>(this T instance)
        {
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(instance));
        }

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
        
        internal static T[] AsArray<T>(this T g)
        {
            return new[] { g };
        }
        
        public static DateTime Utc(this string dateTimeString)
        {
            return DateTime.SpecifyKind(DateTime.Parse(dateTimeString, CultureInfo.GetCultureInfo("sv-SE")), DateTimeKind.Utc);
        }

    }
}