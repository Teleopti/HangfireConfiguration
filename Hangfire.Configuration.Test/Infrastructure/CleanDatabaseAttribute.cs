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
			initializeDb();
			initializeHangfireSchema();
			initializeHangfireStorage();        
		}
		
		private static void closeOpenConnections()
		{
			var closeExistingConnSql = String.Format(
				@"if db_id('{0}') is not null alter database [{0}] set single_user with rollback immediate",
				ConnectionUtils.GetDatabaseName());

			executeSql(closeExistingConnSql);
		}

		private static void dropDb()
		{
			var dropDatabaseSql = String.Format(
				@"if db_id('{0}') is not null drop database [{0}]",
				ConnectionUtils.GetDatabaseName());
			
			executeSql(dropDatabaseSql);
		}

		private static void createDb()
		{
			var createDatabaseSql = String.Format(
				@"if db_id('{0}') is null create database [{0}] COLLATE SQL_Latin1_General_CP1_CS_AS",
				ConnectionUtils.GetDatabaseName());

			executeSql(createDatabaseSql);
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
