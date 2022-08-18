using Hangfire.PostgreSql;

namespace Hangfire.Configuration.Providers;

internal class PostgresStorageProvider : IStorageProvider
{
	public object CopyOptions(object options) => (options as PostgreSqlStorageOptions).DeepCopy();
	public object NewOptions() => new PostgreSqlStorageOptions();
	public bool OptionsIsSuitable(object options) => options is PostgreSqlStorageOptions;
	public string DefaultSchemaName() => new PostgreSqlStorageOptions().SchemaName;
	public void AssignSchemaName(object options, string schemaName) => (options as PostgreSqlStorageOptions).SchemaName = schemaName;
	public bool WorkerBalancerEnabledDefault() => false;

	public JobStorage NewStorage(string connectionString, object options) =>
		new PostgreSqlStorage(connectionString, (PostgreSqlStorageOptions) options);
}