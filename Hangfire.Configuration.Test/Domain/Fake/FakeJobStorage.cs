using Hangfire.PostgreSql;
using Hangfire.Pro.Redis;
using Hangfire.SqlServer;
using Hangfire.Storage;

namespace Hangfire.Configuration.Test.Domain.Fake
{
	public class FakeJobStorage : JobStorage
	{
		public string ConnectionString { get; }
		public SqlServerStorageOptions SqlServerOptions { get; }
		public PostgreSqlStorageOptions PostgresOptions { get; }
		public RedisStorageOptions RedisOptions { get; }
		private readonly IMonitoringApi _monitoringApi;

		public FakeJobStorage(string connectionString, object options, FakeMonitoringApi monitoringApi)
		{
			ConnectionString = connectionString;
			if (options is SqlServerStorageOptions sqlServer)
				SqlServerOptions = sqlServer;
			if (options is PostgreSqlStorageOptions postgres)
				PostgresOptions = postgres;
			if (options is RedisStorageOptions redis)
				RedisOptions = redis;
			_monitoringApi = monitoringApi;
		}

		public override IMonitoringApi GetMonitoringApi() => _monitoringApi;

		public override IStorageConnection GetConnection()
		{
			throw new System.NotImplementedException();
		}
	}
}