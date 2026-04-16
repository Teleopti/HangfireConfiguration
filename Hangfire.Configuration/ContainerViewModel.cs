namespace Hangfire.Configuration;

public class ContainerViewModel
{
	public string Tag { get; set; }
	public string[] Queues { get; set; }
	public bool WorkerBalancerEnabled { get; set; }
	public int? Workers { get; set; }
	public int? MaxWorkersPerServer { get; set; }
}
