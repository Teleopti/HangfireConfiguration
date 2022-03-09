using NUnit.Framework;

namespace Hangfire.Configuration.Test.Infrastructure;

[Parallelizable(ParallelScope.None)]
[TestFixture(ConnectionStrings.SqlServer)]
[TestFixture(ConnectionStrings.Postgres)]
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

	[SetUp]
	public void Setup()
	{
		DatabaseTestSetup.Setup(ConnectionString);
	}
}