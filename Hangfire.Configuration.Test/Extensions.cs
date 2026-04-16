using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;

namespace Hangfire.Configuration.Test;

public static class Extensions
{
    public static void Times(this int times, Action action)
    {
        Enumerable.Range(0, times).ForEach(_ => action());
    }
        
    public static void Times(this int times, Action<int> action)
    {
        Enumerable.Range(0, times).ForEach(action);
    }

    internal static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> act)
    {
        foreach (var item in source)
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

    public static FormUrlEncodedContent FormContent(object data)
    {
        var properties = data.GetType().GetProperties();
        var keyValues = properties
            .Select(x => new KeyValuePair<string, string>(x.Name, x.GetValue(data).ToString()))
            .ToArray();
        return new FormUrlEncodedContent(keyValues);
    }

}