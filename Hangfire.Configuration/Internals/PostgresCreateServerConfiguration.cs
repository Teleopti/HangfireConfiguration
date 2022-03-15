using Npgsql;

namespace Hangfire.Configuration.Internals;

internal class PostgresCreateServerConfiguration : CreateServerConfigurationRelationalDb
{
	public PostgresCreateServerConfiguration(IConfigurationStorage storage, IHangfireSchemaCreator creator) : base(storage, creator)
	{
	}

	protected override string CreateStorageConnectionString(CreateServerConfiguration config)
	{
		return new NpgsqlConnectionStringBuilder
		{
			Host = config.Server,
			Database = config.Database,
			Username = config.User,
			Password = config.Password,
		}.ToString();
	}

	protected override string CreateCreatorConnectionString(CreateServerConfiguration config)
	{
		return new NpgsqlConnectionStringBuilder
		{
			Host = config.Server,
			Database = config.Database,
			Username = config.SchemaCreatorUser,
			Password = config.SchemaCreatorPassword,
		}.ToString();
	}
}