using System.Data.SqlClient;

namespace Hangfire.Configuration.Internals;

internal class SqlServerCreateServerConfiguration : CreateServerConfigurationRelationalDb
{
	public SqlServerCreateServerConfiguration(IConfigurationStorage storage, IHangfireSchemaCreator creator) : base(storage, creator)
	{
	}

	protected override string CreateStorageConnectionString(CreateServerConfiguration config)
	{
		return new SqlConnectionStringBuilder
		{
			DataSource = config.Server ?? "",
			InitialCatalog = config.Database ?? "",
			UserID = config.User ?? "",
			Password = config.Password ?? "",
		}.ToString();
	}

	protected override string CreateCreatorConnectionString(CreateServerConfiguration config)
	{
		return new SqlConnectionStringBuilder
		{
			DataSource = config.Server ?? "",
			InitialCatalog = config.Database ?? "",
			UserID = config.SchemaCreatorUser ?? "",
			Password = config.SchemaCreatorPassword ?? "",
		}.ToString();
	}
}