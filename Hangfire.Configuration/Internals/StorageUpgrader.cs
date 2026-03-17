using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;

namespace Hangfire.Configuration.Internals;

internal class StorageUpgrader(
	ISchemaInstaller installer,
	ConfigurationStorage storage,
	Options options)
{
	public void Upgrade(UpgradeStorage command)
	{
		var exceptions = new List<Exception>();

		try
		{
			var connectionString = setCredentials(command, options.ConfigurationOptions().ConnectionString);
			installer.InstallHangfireConfigurationSchema(connectionString);
		}
		catch (Exception e)
		{
			exceptions.Add(e);
		}

		var configurations = storage.ReadConfigurations();

		var exs = configurations
			.Where(x => !string.IsNullOrEmpty(x.ConnectionString))
			.Where(x =>
			{
				var hasSchema = x.ConnectionString.ToDbSelector().PickDialect(true, true, false);
				return hasSchema;
			})
			.Select(x =>
			{
				var schemaName = x.AppliedSchemaName();
				var connectionString = setCredentials(command, x.ConnectionString);

				try
				{
					installer.InstallHangfireStorageSchema(schemaName, connectionString);
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

	private static string setCredentials(UpgradeStorage command, string connectionString) =>
		connectionString
			.SetCredentials(
				command.SchemaUpgraderUser == null,
				command.SchemaUpgraderUser,
				command.SchemaUpgraderPassword
			);
}