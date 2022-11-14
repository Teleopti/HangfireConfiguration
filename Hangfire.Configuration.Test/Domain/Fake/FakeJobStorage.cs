using Hangfire.PostgreSql;
using Hangfire.SqlServer;
using Hangfire.Storage;

namespace Hangfire.Configuration.Test.Domain.Fake
{
	public class FakeJobStorage : JobStorage
	{
		public string ConnectionString { get; }
		public SqlServerStorageOptions SqlServerOptions => Options as SqlServerStorageOptions;
		public PostgreSqlStorageOptions PostgresOptions => Options as PostgreSqlStorageOptions;

		public object Options { get; }
		private readonly IMonitoringApi _monitoringApi;

		public FakeJobStorage(string connectionString, object options, FakeMonitoringApi monitoringApi)
		{
			ConnectionString = connectionString;
			Options = options;
			_monitoringApi = monitoringApi;
		}

		public override IMonitoringApi GetMonitoringApi() => _monitoringApi;

		public override IStorageConnection GetConnection()
		{
			throw new System.NotImplementedException("Hangfire is faked!");
		}
	}
}