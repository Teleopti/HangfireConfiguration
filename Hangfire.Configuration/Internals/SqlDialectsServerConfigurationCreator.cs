using System;
using Hangfire.Configuration.Providers;

namespace Hangfire.Configuration.Internals;

internal class SqlDialectsServerConfigurationCreator(
	ConfigurationStorage storage,
	ISchemaInstaller installer)
{
	public void Create(
		string storageConnectionString,
		string creatorConnectionString,
		string schemaName,
		string name)
	{
		var provider = storageConnectionString.GetProvider();
		
		schemaName ??= provider.DefaultSchemaName();
		
		installer.TryConnect(storageConnectionString);

		installer.TryConnect(creatorConnectionString);

		if (installer.HangfireStorageSchemaExists(schemaName, creatorConnectionString))
			throw new Exception("Schema already exists.");

		installer.InstallHangfireStorageSchema(schemaName, creatorConnectionString);

		storage.WriteConfiguration(new StoredConfiguration
		{
			Name = name,
			ConnectionString = storageConnectionString,
			SchemaName = schemaName,
			Active = false,
			WorkerBalancerEnabled = provider.WorkerBalancerEnabledDefault()
		});
	}
}