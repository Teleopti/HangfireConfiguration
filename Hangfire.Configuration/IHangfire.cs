using Hangfire.PostgreSql;
using Hangfire.Pro.Redis;
using Hangfire.Server;
using Hangfire.SqlServer;

namespace Hangfire.Configuration;

public interface IHangfire
{
	void UseHangfireServer(
		JobStorage storage,
		BackgroundJobServerOptions options,
		params IBackgroundProcess[] additionalProcesses);

	JobStorage MakeSqlJobStorage(string connectionString, SqlServerStorageOptions options);
	JobStorage MakeSqlJobStorage(string connectionString, RedisStorageOptions options);
	JobStorage MakeSqlJobStorage(string connectionString, PostgreSqlStorageOptions options);
}