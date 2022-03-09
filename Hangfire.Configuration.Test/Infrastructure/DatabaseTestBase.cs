using NUnit.Framework;

namespace Hangfire.Configuration.Test.Infrastructure;

[Parallelizable(ParallelScope.None)]
[CleanDatabase]
[CleanDatabasePostgres]
[TestFixture(ConnectionUtils.DefaultConnectionStringTemplate)]
[TestFixture(ConnectionUtilsPostgres.DefaultConnectionStringTemplate)]
public class DatabaseTestBase
{
	protected readonly string ConnectionString;
	protected readonly string DefaultSchemaName;

	public DatabaseTestBase(string connectionString)
	{
		ConnectionString = connectionString;
		DefaultSchemaName = new ConnectionStringDialectSelector(ConnectionString)
			.SelectDialect(() => "HangFire", () => "hangfire");
	}
}