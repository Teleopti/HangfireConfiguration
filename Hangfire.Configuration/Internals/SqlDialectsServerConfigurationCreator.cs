﻿using System;

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
		_installer.TryConnect(storageConnectionString);

		_installer.TryConnect(creatorConnectionString);

		schemaName ??= creatorConnectionString.ToDbVendorSelector()
			.SelectDialect(DefaultSchemaName.SqlServer, DefaultSchemaName.Postgres);

		if (_installer.HangfireStorageSchemaExists(schemaName, creatorConnectionString))
			throw new Exception("Schema already exists.");

		_installer.InstallHangfireStorageSchema(schemaName, creatorConnectionString);

		_storage.WriteConfiguration(new StoredConfiguration
		{
			Name = name,
			ConnectionString = storageConnectionString,
			SchemaName = schemaName,
			Active = false
		});
	}
}