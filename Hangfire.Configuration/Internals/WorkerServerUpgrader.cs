using System.Linq;

namespace Hangfire.Configuration.Internals;

internal class WorkerServerUpgrader
{
	private readonly ISchemaInstaller _installer;
	private readonly IConfigurationStorage _storage;
	private readonly Options _options;

	public WorkerServerUpgrader(
		ISchemaInstaller installer,
		IConfigurationStorage storage,
		Options options)
	{
		_installer = installer;
		_storage = storage;
		_options = options;
	}

	public void Upgrade(UpgradeWorkerServers command)
	{
		_installer.InstallHangfireConfigurationSchema(_options.ConfigurationOptions().ConnectionString);
		var configurations = _storage.ReadConfigurations();
		configurations
			.Where(x => !string.IsNullOrEmpty(x.ConnectionString))
			.Where(x => x.ConnectionString.ToDbVendorSelector().SelectDialect(true, true, false))
			.ForEach(x =>
			{
				var schemaName = x.SchemaName ?? x.ConnectionString.ToDbVendorSelector()
					.SelectDialect(DefaultSchemaName.SqlServer(), DefaultSchemaName.Postgres());

				var connectionString = x.ConnectionString;
				if (command.SchemaUpgraderUser != null)
					connectionString = connectionString.SetUserNameAndPassword(command.SchemaUpgraderUser, command.SchemaUpgraderPassword);

				_installer.InstallHangfireStorageSchema(schemaName, connectionString);
			});
	}
}