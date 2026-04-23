using System;
using Hangfire.Common;
using Hangfire.Server;

namespace ConsoleSample;

public class SampleBackgroundProcess : IBackgroundProcess
{
	public void Execute(BackgroundProcessContext context)
	{
		Console.WriteLine("Background process tick!");
		context.StoppingToken.Wait(TimeSpan.FromSeconds(20));
	}
}