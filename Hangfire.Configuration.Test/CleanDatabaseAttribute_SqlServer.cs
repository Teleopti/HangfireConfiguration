using System;
using System.Data.SqlClient;
using Dapper;
using Hangfire.Configuration.Internals;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Hangfire.Configuration.Test
{
	public class CleanDatabaseAttribute : Attribute, ITestAction
	{
		private readonly int? _schemaVersion;

		public CleanDatabaseAttribute()
		{
		}

		public CleanDatabaseAttribute(int schemaVersion)
		{
			_schemaVersion = schemaVersion;
		}

		public ActionTargets Targets => ActionTargets.Test;

		public void BeforeTest(ITest test)
		{
			closeOpenConnections();
			dropDb();
			createDb();
			createAdminLogin();
			initializeDb();
		}

		public void AfterTest(ITest test)
		{
		}

		private static void closeOpenConnections()
		{
			var closeExistingConnSql = String.Format(
				@"if db_id('{0}') is not null alter database [{0}] set single_user with rollback immediate",
				ConnectionUtils.GetConnectionString().DatabaseName());

			executeOnMaster(closeExistingConnSql);
		}

		private static void dropDb()
		{
			var dropDatabaseSql = String.Format(
				@"if db_id('{0}') is not null drop database [{0}]",
				ConnectionUtils.GetConnectionString().DatabaseName());

			executeOnMaster(dropDatabaseSql);
		}

		private static void createDb()
		{
			var createDatabaseSql = String.Format(
				@"if db_id('{0}') is null create database [{0}] COLLATE SQL_Latin1_General_CP1_CI_AS",
				ConnectionUtils.GetConnectionString().DatabaseName());

			executeOnMaster(createDatabaseSql);
		}

		private void createAdminLogin()
		{
			var login = ConnectionUtils.GetLoginUser();
			var createLoginSql = $@"
IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = N'{login}')
BEGIN
	CREATE LOGIN {login} WITH PASSWORD=N'{ConnectionUtils.GetLoginUserPassword()}', DEFAULT_DATABASE=[master], DEFAULT_LANGUAGE=[us_english], CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF	
	ALTER SERVER ROLE [sysadmin] ADD MEMBER {login}	
	ALTER LOGIN {login} ENABLE
END";

			executeOnMaster(createLoginSql);
		}

		private void initializeDb()
		{
			using (var connection = new SqlConnection(ConnectionUtils.GetConnectionString()))
			{
				if (_schemaVersion.HasValue)
				{
					if (_schemaVersion.Value > 0)
						SqlServerObjectsInstaller.Install(connection, _schemaVersion.Value);
				}
				else
					SqlServerObjectsInstaller.Install(connection);
			}
		}

		private static void executeOnMaster(string sql)
		{
			using (var connection = new SqlConnection(ConnectionUtils.GetConnectionString().PointToMasterDatabase()))
				connection.Execute(sql);
		}
	}
}