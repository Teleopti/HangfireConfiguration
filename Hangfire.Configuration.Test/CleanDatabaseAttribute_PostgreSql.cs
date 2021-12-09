using System;
using System.Data.SqlClient;
using System.Reflection;
using Dapper;
using Npgsql;
using Xunit.Sdk;

namespace Hangfire.Configuration.Test
{
    public class CleanDatabaseAttribute : BeforeAfterTestAttribute
    {
        private readonly int? _schemaVersion;

        public CleanDatabaseAttribute()
        {
        }

        public CleanDatabaseAttribute(int schemaVersion)
        {
            _schemaVersion = schemaVersion;
        }

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
            //var closeExistingConnSql = String.Format(
            //    @"if db_id('{0}') is not null alter database [{0}] set single_user with rollback immediate",
            //    ConnectionUtils.GetDatabaseName());

            //executeSql(closeExistingConnSql);
        }

        private static void dropDb()
        {
            var dropDatabaseSql = 
				$"DROP DATABASE IF EXISTS \"{ ConnectionUtils.GetDatabaseName()}\" WITH (FORCE);";

            executeSql(dropDatabaseSql);
        }

        private static void createDb()
        {
            var createDatabaseSql = String.Format(
				@"CREATE DATABASE ""{0}""",
                ConnectionUtils.GetDatabaseName());

            executeSql(createDatabaseSql);
        }

        private void createAdminLogin()
        {
//            var login = ConnectionUtils.GetLoginUser();
//            var createLoginSql = $@"
//IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = N'{login}')
//BEGIN
//	CREATE LOGIN {login} WITH PASSWORD=N'{ConnectionUtils.GetLoginUserPassword()}', DEFAULT_DATABASE=[master], DEFAULT_LANGUAGE=[us_english], CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF	
//	ALTER SERVER ROLE [sysadmin] ADD MEMBER {login}	
//	ALTER LOGIN {login} ENABLE
//END";

//            executeSql(createLoginSql);
        }

        private void initializeDb()
        {
            using (var connection = new NpgsqlConnection(ConnectionUtils.GetConnectionString()))
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

        private static void executeSql(string sql)
        {
            using (var connection = new NpgsqlConnection(ConnectionUtils.GetMasterConnectionString()))
                connection.Execute(sql);
        }
    }
}