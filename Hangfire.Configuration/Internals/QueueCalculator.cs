using System.Linq;

namespace Hangfire.Configuration.Internals;

internal class QueueCalculator
{
	private readonly Options _options;

	internal QueueCalculator(Options options)
	{
		_options = options;
	}

	internal string[] CalculateAppliedQueues(
		ContainerConfiguration container,
		ContainerConfiguration[] containers)
	{
		var serverQueues = _options.ServerOptions()?.Queues ?? [];
		var containerQueues = container.Queues ?? [];
		var isDefault = container.Tag == null || container.Tag == DefaultContainerTag.Tag();
		if (isDefault)
		{
			var otherContainerQueues = containers
				.Where(c => c != container)
				.Where(c => c.Queues != null)
				.SelectMany(c => c.Queues)
				.ToArray();

			var unclaimed = serverQueues
				.Where(q => !otherContainerQueues.Contains(q))
				.ToArray();

			var included = containerQueues.Union(unclaimed);

			return serverQueues
				.Intersect(included)
				.ToArray();
		}
		else
		{
			if (containerQueues.Length > 0)
				return serverQueues
					.Intersect(containerQueues)
					.ToArray();
		}

		return [];
	}
}
