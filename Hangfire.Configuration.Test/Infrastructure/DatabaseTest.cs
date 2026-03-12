using DbAgnostic;
using NUnit.Framework;

namespace Hangfire.Configuration.Test.Infrastructure;

[Parallelizable(ParallelScope.None)]
[TestFixture(ConnectionStrings.SqlServer)]
[TestFixture(ConnectionStrings.Postgres)]
public abstract class DatabaseTest(string connectionString)
{
	protected readonly string ConnectionString = connectionString;

	[SetUp]
	public void Setup()
	{
		ConnectionString.PickAction(
			() => DatabaseTestSetup.SetupSqlServer(ConnectionString),
			() => DatabaseTestSetup.SetupPostgres(ConnectionString));
	}
}