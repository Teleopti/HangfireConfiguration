using System;
using System.Data.SqlClient;
using System.Reflection;
using Dapper;
using Xunit.Sdk;

namespace Hangfire.Configuration.Test.Infrastructure
{
    public class CleanDatabaseAttribute : BeforeAfterTestAttribute
    {
        public override void Before(MethodInfo methodUnderTest)
        {
			closeOpenConnections();
			dropDb();
			createDb();
			createAdminLogin();
			initializeDb();
			initializeHangfireSchema();
			initializeHangfireStorage();        
		}

        private static void closeOpenConnections()
		{
			var closeExistingConnSql =
                $@"if db_id('{ConnectionUtils.GetDatabaseName()}') is not null alter database [{ConnectionUtils.GetDatabaseName()}] set single_user with rollback immediate";

			executeSql(closeExistingConnSql);
		}

		private static void dropDb()
		{
			var dropDatabaseSql =
                $@"if db_id('{ConnectionUtils.GetDatabaseName()}') is not null drop database [{ConnectionUtils.GetDatabaseName()}]";
			
			executeSql(dropDatabaseSql);
		}

		private static void createDb()
		{
			var createDatabaseSql =
                $@"if db_id('{ConnectionUtils.GetDatabaseName()}') is null create database [{ConnectionUtils.GetDatabaseName()}] COLLATE SQL_Latin1_General_CP1_CS_AS";

			executeSql(createDatabaseSql);
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
			executeSql(createLoginSql);
		}

		private static void initializeDb()
		{
			using (var connection = new SqlConnection(
				ConnectionUtils.GetConnectionString()))
			{
				SqlServerObjectsInstaller.Install(connection);
			}
		}

		private static void initializeHangfireSchema()
		{
			using (var connection = new SqlConnection(ConnectionUtils.GetConnectionString()))
			{
				SqlServer.SqlServerObjectsInstaller.Install(connection);
			}
		}

		private static void initializeHangfireStorage()
		{
			GlobalConfiguration
				.Configuration
				.UseSqlServerStorage(ConnectionUtils.GetConnectionString());
		}

		private static void executeSql(string sql)
		{
			using (var connection = new SqlConnection(
				ConnectionUtils.GetMasterConnectionString()))
			{
				connection.Execute(sql);
			}
		}
	}
}
