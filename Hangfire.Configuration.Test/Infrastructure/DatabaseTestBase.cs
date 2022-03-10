using System;
using NUnit.Framework;

namespace Hangfire.Configuration.Test.Infrastructure;

[Parallelizable(ParallelScope.None)]
[TestFixture(ConnectionStrings.SqlServer)]
[TestFixture(ConnectionStrings.Postgres)]
public class DatabaseTestBase
{
	protected readonly string ConnectionString;
	protected readonly string DefaultSchemaName;
	private bool _isSqlServer;

	public DatabaseTestBase(string connectionString)
	{
		ConnectionString = connectionString;
		DefaultSchemaName = new ConnectionStringDialectSelector(ConnectionString)
			.SelectDialect(() =>
			{
				_isSqlServer = true;
				return "HangFire";
			}, () =>
			{
				_isSqlServer = false;
				return "hangfire";
			});
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