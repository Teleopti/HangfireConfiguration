using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using Hangfire.Configuration.Providers;

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
			var connectionString = setCredentials(command, _options.ConfigurationOptions().ConnectionString);
			_installer.InstallHangfireConfigurationSchema(connectionString);
		}
		catch (Exception e)
		{
			exceptions.Add(e);
		}

		var configurations = _storage.ReadConfigurations();

		var exs = configurations
			.Where(x => !string.IsNullOrEmpty(x.ConnectionString))
			.Where(x =>
			{
				var hasSchema = x.ConnectionString.ToDbVendorSelector().SelectDialect(true, true, false);
				return hasSchema;
			})
			.Select(x =>
			{
				var schemaName = x.SchemaName ?? x.ConnectionString
					.GetProvider().DefaultSchemaName();

				var connectionString = setCredentials(command, x.ConnectionString);

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

	private static string setCredentials(UpgradeWorkerServers command, string connectionString) =>
		connectionString
			.SetCredentials(
				command.SchemaUpgraderUser == null,
				command.SchemaUpgraderUser,
				command.SchemaUpgraderPassword
			);
}