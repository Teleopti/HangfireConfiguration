namespace Hangfire.Configuration;

public class StoredConfiguration
{
	public int? Id { get; set; }
	public string Name { get; set; }
	public string ConnectionString { get; set; }
	public string SchemaName { get; set; }
	public bool? Active { get; set; }
	
	public bool? WorkerBalancerEnabled { get; set; }
	public int? GoalWorkerCount { get; set; }
	public int? MaxWorkersPerServer { get; set; }

	internal bool IsActive() => Active.GetValueOrDefault();
	internal bool WorkerBalancerIsEnabled() => WorkerBalancerEnabled ?? true;
}
