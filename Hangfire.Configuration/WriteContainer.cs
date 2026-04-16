namespace Hangfire.Configuration;

public class WriteContainer
{
	public int ConfigurationId { get; set; }
	public bool WorkerBalancerEnabled { get; set; }
	public int? Workers { get; set; }
	public int? MaxWorkersPerServer { get; set; }
}
