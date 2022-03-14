using System.Collections.Generic;

namespace Hangfire.Configuration;

public class ConfigurationOptions
{
	public string ConnectionString { get; set; }

	public bool PrepareSchemaIfNecessary { get; set; }
	public bool AllowNewServerCreation { get; set; }
	public bool AllowMultipleActive { get; set; }

	public IStorageOptionsFactory StorageOptionsFactory { get; set; }

	public IEnumerable<UpdateStorageConfiguration> UpdateConfigurations { get; set; }

	public bool UseWorkerDeterminer { get; set; } = true;
	public WorkerDeterminerOptions WorkerDeterminerOptions { get; } = new();

	public IHangfireConfigurationAuthorizationFilter Authorization { get; set; } = null;
}