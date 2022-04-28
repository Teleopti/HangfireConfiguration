using System.Reflection;
using Hangfire.SqlServer;

namespace Hangfire.Configuration.Providers;

internal class SqlServerStorageProvider : IStorageProvider
{
	public object CopyOptions(object options)
	{
		return (options as SqlServerStorageOptions).DeepCopy();
	}

	public object NewOptions()
	{
		return new SqlServerStorageOptions();
	}

	public bool OptionsIsSuitable(object options)
	{
		return options is SqlServerStorageOptions;
	}

	public string DefaultSchemaName()
	{
		return typeof(SqlServerStorageOptions).Assembly.GetType("Hangfire.SqlServer.Constants")
			.GetField("DefaultSchema", BindingFlags.Static | BindingFlags.Public)
			?.GetValue(null) as string;
	}

	public void AssignSchemaName(object options, string schemaName)
	{
		(options as SqlServerStorageOptions).SchemaName = schemaName;
	}

	public JobStorage NewStorage(string connectionString, object options)
	{
		return new SqlServerStorage(connectionString, (SqlServerStorageOptions) options);
	}
}