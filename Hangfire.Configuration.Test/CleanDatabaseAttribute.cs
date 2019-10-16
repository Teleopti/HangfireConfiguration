using System;
using System.Data.SqlClient;
using System.Reflection;
using System.Threading;
using Dapper;
using Hangfire.Configuration.Test.Infrastructure;
using Xunit.Sdk;

namespace Hangfire.Configuration.Test
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
            using (var connection = new SqlConnection(ConnectionUtils.GetConnectionString()))
                SqlServerObjectsInstaller.Install(connection);
        }

        private static void executeSql(string sql)
        {
            using (var connection = new SqlConnection(ConnectionUtils.GetMasterConnectionString()))
                connection.Execute(sql);
        }
    }
}