namespace Hangfire.Configuration;

public class ContainerConfiguration
{
	public string Tag { get; set; }
	public string[] Queues { get; set; }
	public bool? WorkerBalancerEnabled { get; set; }
	public int? GoalWorkerCount { get; set; }
	public int? MaxWorkersPerServer { get; set; }

	internal bool WorkerBalancerIsEnabled() => WorkerBalancerEnabled ?? true;
}
