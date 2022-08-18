using System;
using Hangfire.Configuration.Providers;

namespace Hangfire.Configuration.Internals;

internal class SqlDialectsServerConfigurationCreator
{
	private readonly IConfigurationStorage _storage;
	private readonly ISchemaInstaller _installer;

	public SqlDialectsServerConfigurationCreator(
		IConfigurationStorage storage,
		ISchemaInstaller installer)
	{
		_storage = storage;
		_installer = installer;
	}

	public void Create(
		string storageConnectionString,
		string creatorConnectionString,
		string schemaName,
		string name)
	{
		var provider = storageConnectionString.GetProvider();
		
		schemaName ??= provider.DefaultSchemaName();
		
		_installer.TryConnect(storageConnectionString);

		_installer.TryConnect(creatorConnectionString);

		if (_installer.HangfireStorageSchemaExists(schemaName, creatorConnectionString))
			throw new Exception("Schema already exists.");

		_installer.InstallHangfireStorageSchema(schemaName, creatorConnectionString);

		_storage.WriteConfiguration(new StoredConfiguration
		{
			Name = name,
			ConnectionString = storageConnectionString,
			SchemaName = schemaName,
			Active = false,
			WorkerBalancerEnabled = provider.WorkerBalancerEnabledDefault()
		});
	}
}