using System;
using System.Linq;

namespace Hangfire.Configuration.Test
{
    public static class Extensions
    {
        public static void Times(this int times, Action action)
        {
            Enumerable.Range(0, times).ForEach(i => action());
        }
    }
}