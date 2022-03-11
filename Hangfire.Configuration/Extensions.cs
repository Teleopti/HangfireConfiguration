using System;
using System.Collections.Generic;
using Newtonsoft.Json;

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
        
	    internal static T DeepCopy<T>(this T obj)
	    {
		    return obj == null ? default : JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(obj));
	    }
    }
}