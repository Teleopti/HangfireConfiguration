namespace Hangfire.Configuration.Internals;

internal class SqlServerCreateServerConfiguration : CreateServerConfigurationRelationalDb
{
	public SqlServerCreateServerConfiguration(IConfigurationStorage storage, IHangfireSchemaCreator creator) : base(storage, creator)
	{
	}

	protected override string CreateStorageConnectionString(CreateServerConfiguration config)
	{
		return config.StorageConnectionString ?? $"Data Source={config.Server};Initial Catalog={config.Database};User ID={config.User};Password={config.Password}";
	}

	protected override string CreateCreatorConnectionString(CreateServerConfiguration config)
	{
		return config.SchemaCreatorConnectionString ??
		       $"Data Source={config.Server};Initial Catalog={config.Database};User ID={config.SchemaCreatorUser};Password={config.SchemaCreatorPassword}";
	}
}