namespace Hangfire.Configuration;

public class PostgresCreateServerConfiguration : CreateServerConfigurationRelationalDb
{
	public PostgresCreateServerConfiguration(IConfigurationStorage storage, IHangfireSchemaCreator creator) : base(storage, creator)
	{
	}

	protected override string CreateStorageConnectionString(CreateServerConfiguration config)
	{
		return config.StorageConnectionString ?? $@"Host={config.Server};Database=""{config.Database}"";User ID={config.User};Password={config.Password};";
	}

	protected override string CreateCreatorConnectionString(CreateServerConfiguration config)
	{
		return config.SchemaCreatorConnectionString ??
		       $@"Host={config.Server};Database=""{config.Database}"";User ID={config.SchemaCreatorUser};Password={config.SchemaCreatorPassword};";
	}
}