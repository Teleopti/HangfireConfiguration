namespace Hangfire.Configuration;

public class ViewModel
{
	public int? Id { get; set; }
	public string Name { get; set; }
	public string ConnectionString { get; set; }
	public string SchemaName { get; set; }

	public bool Active { get; set; }

	public ContainerViewModel[] Containers { get; set; } = [new()];

	public string[] AvailableQueues { get; set; } = [];
}