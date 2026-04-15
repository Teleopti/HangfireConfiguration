using System;
using System.Threading;
using Hangfire;

namespace ConsoleSample;

public class SampleService
{
	private static readonly Random Rand = new();

	public void DoWork(int number)
	{
		int time;
		lock (Rand)
		{
			time = Rand.Next(3);
		}

		Console.WriteLine($"Starting task {number}, estimated {time}s");
		Thread.Sleep(TimeSpan.FromSeconds(time));
		Console.WriteLine($"Finished task {number}");
	}
}