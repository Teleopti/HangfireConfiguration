using System;
using System.Threading;

namespace Hangfire.Configuration.Test
{
	public static class TestLog
	{
		private static readonly object Lock = new object();
		
		public static void WriteLine(string s)
		{
			// should probably use log4net or similar to not lock here
			lock (Lock)
				System.IO.File.AppendAllLines("testlog.log", $"{DateTime.UtcNow} {Thread.CurrentThread.ManagedThreadId} {s}".AsArray());
		}
	}
}