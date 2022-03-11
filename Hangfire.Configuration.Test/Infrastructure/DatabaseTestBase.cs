using System;
using System.Data.SqlClient;
using NUnit.Framework;

namespace Hangfire.Configuration.Test.Infrastructure;

[Parallelizable(ParallelScope.None)]
[TestFixture(ConnectionStrings.SqlServer)]
[TestFixture(ConnectionStrings.Postgres)]
public class DatabaseTestBase
{
	protected readonly string ConnectionString;

	public DatabaseTestBase(string connectionString)
	{
		ConnectionString = connectionString;
	}

	[SetUp]
	public void Setup()
	{
		if(isSqlServer())
			DatabaseTestSetup.SetupSqlServer(ConnectionString);
		else
			DatabaseTestSetup.SetupPostgres(ConnectionString);
	}
	
	protected T SelectDialect<T>(Func<T> sqlServer, Func<T> postgres)
	{
		return isSqlServer() ? sqlServer() : postgres();
	}

	private bool isSqlServer()
	{
		return ConnectionString.Equals(ConnectionStrings.SqlServer);
	}
}