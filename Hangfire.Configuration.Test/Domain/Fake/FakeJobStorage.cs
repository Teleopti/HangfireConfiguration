using Hangfire.PostgreSql;
using Hangfire.SqlServer;
using Hangfire.Storage;

namespace Hangfire.Configuration.Test.Domain.Fake
{
	public class FakeJobStorage : JobStorage
	{
		public string ConnectionString { get; }
		public SqlServerStorageOptions SqlServerOptions { get; }
		public PostgreSqlStorageOptions PostgresOptions { get; }
		private readonly IMonitoringApi _monitoringApi;

		public FakeJobStorage(string connectionString, object options, FakeMonitoringApi monitoringApi)
		{
			ConnectionString = connectionString;
			if (options is SqlServerStorageOptions sqlServer)
				SqlServerOptions = sqlServer;
			if (options is PostgreSqlStorageOptions postgres)
				PostgresOptions = postgres;
			_monitoringApi = monitoringApi;
		}

		public override IMonitoringApi GetMonitoringApi() => _monitoringApi;

		public override IStorageConnection GetConnection()
		{
			throw new System.NotImplementedException();
		}
	}
}