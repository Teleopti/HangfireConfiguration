using System;
using System.Threading;
using Hangfire;

namespace ConsoleSample;

public class SampleService
{
	private static readonly Random Rand = new();

	[Queue("critical")]
	public void Random(int number)
	{
		Console.WriteLine("Starting	task: " + number);
		int time;
		lock (Rand)
		{
			time = Rand.Next(10);
		}

		if (time < 5)
		{
			Console.WriteLine("Failed task: " + number);
			throw new Exception();
		}

		Thread.Sleep(TimeSpan.FromSeconds(5 + time));
		Console.WriteLine("Finished task: " + number);
	}
}