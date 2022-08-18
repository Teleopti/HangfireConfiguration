using System.Reflection;
using Hangfire.SqlServer;

namespace Hangfire.Configuration.Providers;

internal class SqlServerStorageProvider : IStorageProvider
{
	public object CopyOptions(object options) => (options as SqlServerStorageOptions).DeepCopy();
	public object NewOptions() => new SqlServerStorageOptions();
	public bool OptionsIsSuitable(object options) => options is SqlServerStorageOptions;
	public string DefaultSchemaName() =>
		typeof(SqlServerStorageOptions).Assembly.GetType("Hangfire.SqlServer.Constants")
			.GetField("DefaultSchema", BindingFlags.Static | BindingFlags.Public)
			?.GetValue(null) as string;
	public void AssignSchemaName(object options, string schemaName) => (options as SqlServerStorageOptions).SchemaName = schemaName;
	public bool WorkerBalancerEnabledDefault() => true;
	
	public JobStorage NewStorage(string connectionString, object options) =>
		new SqlServerStorage(connectionString, (SqlServerStorageOptions) options);
}