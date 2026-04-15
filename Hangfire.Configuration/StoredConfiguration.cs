using System;
using Hangfire.Configuration.Providers;

namespace Hangfire.Configuration;

public class StoredConfiguration
{
	public int? Id { get; set; }
	public string Name { get; set; }
	public string ConnectionString { get; set; }
	public string SchemaName { get; set; }
	public bool? Active { get; set; }
	public DateTime? ShutdownAt { get; set; }

	public ContainerConfiguration[] Containers { get; set; } = [new() {Tag = DefaultContainerTag.Tag()}];

	internal ContainerConfiguration DefaultContainer() => Containers[0];

	internal bool IsActive() => Active.GetValueOrDefault();

	internal string AppliedSchemaName()
	{
		if (SchemaName != null)
			return SchemaName;
		if (ConnectionString != null)
			return ConnectionString.GetProvider().DefaultSchemaName();
		return SchemaName;
	}
}