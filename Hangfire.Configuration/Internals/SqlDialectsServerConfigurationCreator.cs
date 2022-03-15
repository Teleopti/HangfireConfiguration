using System;

namespace Hangfire.Configuration.Internals;

internal class SqlDialectsServerConfigurationCreator
{
	private readonly IConfigurationStorage _storage;
	private readonly IHangfireSchemaCreator _creator;

	public SqlDialectsServerConfigurationCreator(
		IConfigurationStorage storage,
		IHangfireSchemaCreator creator)
	{
		_storage = storage;
		_creator = creator;
	}

	public void Create(
		string storageConnectionString,
		string creatorConnectionString,
		string schemaName,
		string name)
	{
		_creator.TryConnect(storageConnectionString);

		_creator.TryConnect(creatorConnectionString);

		schemaName ??= creatorConnectionString.ToDbVendorSelector()
			.SelectDialect(DefaultSchemaName.SqlServer, DefaultSchemaName.Postgres);

		if (_creator.HangfireStorageSchemaExists(schemaName, creatorConnectionString))
			throw new Exception("Schema already exists.");

		_creator.CreateHangfireStorageSchema(schemaName, creatorConnectionString);

		_storage.WriteConfiguration(new StoredConfiguration
		{
			Name = name,
			ConnectionString = storageConnectionString,
			SchemaName = schemaName,
			Active = false
		});
	}
}