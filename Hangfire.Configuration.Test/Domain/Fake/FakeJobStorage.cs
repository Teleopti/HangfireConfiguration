using Hangfire.PostgreSql;
using Hangfire.SqlServer;
using Hangfire.Storage;

namespace Hangfire.Configuration.Test.Domain.Fake;

public class FakeJobStorage(
	string connectionString,
	object options,
	FakeMonitoringApi monitoringApi) :
	JobStorage
{
	public string ConnectionString { get; } = connectionString;
	public SqlServerStorageOptions SqlServerOptions => Options as SqlServerStorageOptions;
	public PostgreSqlStorageOptions PostgresOptions => Options as PostgreSqlStorageOptions;

	public object Options { get; } = options;
	private readonly IMonitoringApi _monitoringApi = monitoringApi;

	public override IMonitoringApi GetMonitoringApi() => _monitoringApi;

	public override IStorageConnection GetConnection() =>
		throw new System.NotImplementedException("Hangfire is faked!");
}