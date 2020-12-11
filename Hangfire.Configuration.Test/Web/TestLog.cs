using System;
using System.Threading;

namespace Hangfire.Configuration.Test.Web
{
	public static class TestLog
	{
		private static readonly object Lock = new object();
		
		public static void WriteLine(string s)
		{
			lock (Lock)
				System.IO.File.AppendAllLines("testlog.log", $"{DateTime.UtcNow} {Thread.CurrentThread.ManagedThreadId} {s}".AsArray());
		}
	}
}