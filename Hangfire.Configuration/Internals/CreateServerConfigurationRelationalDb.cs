using System;

namespace Hangfire.Configuration.Internals;

internal abstract class CreateServerConfigurationRelationalDb : ICreateServerConfiguration
{
	private readonly IConfigurationStorage _storage;
	private readonly IHangfireSchemaCreator _creator;

	protected CreateServerConfigurationRelationalDb(IConfigurationStorage storage,
		IHangfireSchemaCreator creator)
	{
		_storage = storage;
		_creator = creator;
	}

	protected abstract string CreateStorageConnectionString(CreateServerConfiguration config);
	protected abstract string CreateCreatorConnectionString(CreateServerConfiguration config);

	public void Create(CreateServerConfiguration config)
	{
		var storageConnectionString = CreateStorageConnectionString(config);
		var creatorConnectionString = CreateCreatorConnectionString(config);

		_creator.TryConnect(storageConnectionString);
            
		_creator.TryConnect(creatorConnectionString);

		config.SchemaName ??= creatorConnectionString.ToDbVendorSelector()
			.SelectDialect(DefaultSchemaName.SqlServer, DefaultSchemaName.Postgres);
            
		if (_creator.HangfireStorageSchemaExists(config.SchemaName, creatorConnectionString))
			throw new Exception("Schema already exists.");

		_creator.CreateHangfireStorageSchema(config.SchemaName, creatorConnectionString);

		_storage.WriteConfiguration(new StoredConfiguration
		{
			Name = config.Name,
			ConnectionString = storageConnectionString,
			SchemaName = config.SchemaName,
			Active = false
		});
	}
}