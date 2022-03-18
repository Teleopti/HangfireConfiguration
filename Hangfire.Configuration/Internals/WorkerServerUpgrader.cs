using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;

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
		var exceptions = new List<Exception>();

		try
		{
			_installer.InstallHangfireConfigurationSchema(_options.ConfigurationOptions().ConnectionString);
		}
		catch (Exception e)
		{
			exceptions.Add(e);
		}

		var configurations = _storage.ReadConfigurations();

		var exs = configurations
			.Where(x => !string.IsNullOrEmpty(x.ConnectionString))
			.Where(x => x.ConnectionString.ToDbVendorSelector().SelectDialect(true, true, false))
			.Select(x =>
			{
				var schemaName = x.SchemaName ?? x.ConnectionString.ToDbVendorSelector()
					.SelectDialect(DefaultSchemaName.SqlServer(), DefaultSchemaName.Postgres());

				var connectionString = x.ConnectionString;
				if (command.SchemaUpgraderUser != null)
					connectionString = connectionString.SetUserNameAndPassword(command.SchemaUpgraderUser, command.SchemaUpgraderPassword);

				try
				{
					_installer.InstallHangfireStorageSchema(schemaName, connectionString);
				}
				catch (Exception e)
				{
					return e;
				}

				return null;
			})
			.Where(x => x != null)
			.ToArray();

		exceptions.AddRange(exs);

		if (exceptions.Count == 1)
			ExceptionDispatchInfo.Capture(exceptions.First()).Throw();
		if (exceptions.Any())
			throw new AggregateException(exceptions);
	}
}