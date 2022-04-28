using Hangfire.PostgreSql;

namespace Hangfire.Configuration.Providers;

internal class PostgresStorageProvider : IStorageProvider
{
	public object CopyOptions(object options)
	{
		return (options as PostgreSqlStorageOptions).DeepCopy();
	}

	public object NewOptions()
	{
		return new PostgreSqlStorageOptions();
	}

	public bool OptionsIsSuitable(object options)
	{
		return options is PostgreSqlStorageOptions;
	}

	public string DefaultSchemaName()
	{
		return new PostgreSqlStorageOptions().SchemaName;
	}

	public void AssignSchemaName(object options, string schemaName)
	{
		(options as PostgreSqlStorageOptions).SchemaName = schemaName;
	}

	public JobStorage NewStorage(string connectionString, object options)
	{
		return new PostgreSqlStorage(connectionString, (PostgreSqlStorageOptions) options);
	}
}