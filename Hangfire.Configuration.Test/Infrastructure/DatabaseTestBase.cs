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
	protected readonly string DefaultSchemaName;
	private readonly bool _isSqlServer;

	public DatabaseTestBase(string connectionString)
	{
		try
		{
			new SqlConnectionStringBuilder(connectionString);
			_isSqlServer = true;
			DefaultSchemaName = "HangFire";
		}
		catch (Exception)
		{
			_isSqlServer = false;
			DefaultSchemaName = "hangfire";
		}
		ConnectionString = connectionString;
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