using System;
using NUnit.Framework;

namespace Hangfire.Configuration.Test.Infrastructure;

[Parallelizable(ParallelScope.None)]
[TestFixture(ConnectionStrings.SqlServer)]
[TestFixture(ConnectionStrings.Postgres)]
public abstract class DatabaseTestBase
{
	protected readonly string ConnectionString;
	private readonly bool _isSqlServer;

	protected DatabaseTestBase(string connectionString)
	{
		ConnectionString = connectionString;
		_isSqlServer = ConnectionString.Equals(ConnectionStrings.SqlServer);
	}

	[SetUp]
	public void Setup()
	{
		if(_isSqlServer)
			DatabaseTestSetup.SetupSqlServer(ConnectionString);
		else
			DatabaseTestSetup.SetupPostgres(ConnectionString);
	}
	
	protected T SelectDialect<T>(Func<T> sqlServer, Func<T> postgres)
	{
		return _isSqlServer ? sqlServer() : postgres();
	}
}