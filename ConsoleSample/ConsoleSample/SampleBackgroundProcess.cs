using System;
using Hangfire.Common;
using Hangfire.Server;

namespace ConsoleSample;

public class SampleBackgroundProcess : IBackgroundProcess
{
	public void Execute(BackgroundProcessContext context)
	{
		Console.WriteLine("20 second tick!");
		context.StoppingToken.Wait(TimeSpan.FromSeconds(20));
	}
}