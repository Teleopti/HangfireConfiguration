using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Hangfire.Server;

namespace Hangfire.Configuration.Test.Integration;

public static class WorkerEmulation
{
	public static void SingleIteration(
		JobStorage storage,
		IEnumerable<string> queues = null
	)
	{
		// will hang if nothing to work with
		// if (NumberOfEnqueuedJobs() > 0)
		var worker = queues != null ? new Worker(queues.ToArray()) : new Worker();
		worker.Execute(
			new BackgroundProcessContext(
				"fake server",
				storage,
				new Dictionary<string, object>(),
				Guid.NewGuid(),
				new CancellationToken(),
				new CancellationToken(),
				new CancellationToken()
			));
	}
}