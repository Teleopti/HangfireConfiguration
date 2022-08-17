namespace Hangfire.Configuration;

public class WorkerBalancerOptions
{
	public int DefaultGoalWorkerCount { get; set; } = 10;
	public int MaximumGoalWorkerCount { get; set; } = 100;
	public int MinimumWorkerCount { get; set; } = 1;

	public int? MinimumServerCount { get; set; }
	public bool UseServerCountSampling { get; set; } = true;
}