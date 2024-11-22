using System;
using DbAgnostic;
using NUnit.Framework;

namespace Hangfire.Configuration.Test.Infrastructure;

[Parallelizable(ParallelScope.None)]
[TestFixture(ConnectionStrings.SqlServer)]
[TestFixture(ConnectionStrings.Postgres)]
public abstract class DatabaseTest
{
	protected readonly string ConnectionString;

	protected DatabaseTest(string connectionString)
	{
		ConnectionString = connectionString;
	}

	[SetUp]
	public void Setup()
	{
		ConnectionString.PickAction(
			() => DatabaseTestSetup.SetupSqlServer(ConnectionString),
			() => DatabaseTestSetup.SetupPostgres(ConnectionString));
	}
}