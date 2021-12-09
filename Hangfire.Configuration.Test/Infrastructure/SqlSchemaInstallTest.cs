using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Xunit;
using Xunit.Abstractions;

namespace Hangfire.Configuration.Test.Infrastructure
{
    [Collection("NotParallel")]
    public class SqlSchemaInstallTest : XunitContextBase
    {
        public SqlSchemaInstallTest(ITestOutputHelper output) : base(output)
        {
        }

        [FactSkipPostgreSql, CleanDatabase(schemaVersion: 1)]
        public void ShouldInstallSchemaVersion1()
        {
            Assert.Equal(1, version());
        }

        [FactSkipPostgreSql, CleanDatabase(schemaVersion: 2)]
        public void ShouldInstallSchemaVersion2()
        {
            Assert.Equal(2, version());
        }

        [FactSkipPostgreSql, CleanDatabase(schemaVersion: 3)]
        public void ShouldInstallSchemaVersion3()
        {
            Assert.Equal(3, version());
        }

        [Fact, CleanDatabase(schemaVersion: 0)]
        public void ShouldUpgradeFrom0ToLatest()
        {
            install();
            Assert.Equal(SqlServerObjectsInstaller.SchemaVersion, version());
        }

        [FactSkipPostgreSql, CleanDatabase(schemaVersion: 1)]
        public void ShouldUpgradeFrom1ToLatest()
        {
            using (var c = new SqlConnection(ConnectionUtils.GetConnectionString()))
                c.Execute("INSERT INTO HangfireConfiguration.Configuration ([Key], Value) VALUES ('GoalWorkerCount', 52)");

            install();

            Assert.Equal(52, read().Single().GoalWorkerCount);
            Assert.Equal(SqlServerObjectsInstaller.SchemaVersion, version());
        }

        [FactSkipPostgreSql, CleanDatabase(schemaVersion: 2)]
        public void ShouldUpgradeFrom2ToLatest()
        {
            Assert.Equal(2, version());
            using (var c = new SqlConnection(ConnectionUtils.GetConnectionString()))
                c.Execute(@"
INSERT INTO 
    HangfireConfiguration.Configuration 
    (ConnectionString, SchemaName, GoalWorkerCount, Active) 
    VALUES (@ConnectionString, @SchemaName, @GoalWorkerCount, @Active)
", new values {GoalWorkerCount = 99});

            install();

            Assert.Equal(99, read().Single().GoalWorkerCount);
            Assert.Equal(SqlServerObjectsInstaller.SchemaVersion, version());
        }

        [FactSkipPostgreSql, CleanDatabase(schemaVersion: 2)]
        public void ShouldUpgradeFrom2To3()
        {
            Assert.Equal(2, version());
            using (var c = new SqlConnection(ConnectionUtils.GetConnectionString()))
            {
                c.Execute(@"INSERT INTO HangfireConfiguration.Configuration (ConnectionString) VALUES (@ConnectionString)", new values());
                c.Execute(@"INSERT INTO HangfireConfiguration.Configuration (ConnectionString) VALUES (@ConnectionString)", new values());
            }

            install(3);

            Assert.Equal(DefaultConfigurationName.Name(), read().First().Name);
            Assert.Equal(3, version());
        }

        private void install(int? schemaVersion = null)
        {
            using (var c = new ConnectionStringDialectSelector(ConnectionUtils.GetConnectionString()).GetConnection())
            {
                if (schemaVersion.HasValue)
                    SqlServerObjectsInstaller.Install(c, schemaVersion.Value);
                else
                    SqlServerObjectsInstaller.Install(c);
            }
        }

        private IEnumerable<values> read()
        {
			using (var c = new ConnectionStringDialectSelector(ConnectionUtils.GetConnectionString()).GetConnection())
				return c.Query<values>("SELECT * FROM hangfireconfiguration.configuration");
        }

        private int version()
        {
	        var sqlDialectSelector = new ConnectionStringDialectSelector(ConnectionUtils.GetConnectionString());

			using (var c = sqlDialectSelector.GetConnection())
				return sqlDialectSelector.SelectDialect(
					() => c.Query<int>("SELECT Version FROM HangfireConfiguration.[Schema]").Single(), 
					() => c.Query<int>("SELECT version FROM hangfireconfiguration.schema").Single());
        }

        private class values
        {
            public int Id { get; set; }

            public string Key { get; set; }
            public string Value { get; set; }

            public string Name { get; set; }
            public string ConnectionString { get; set; }
            public string SchemaName { get; set; }
            public int? GoalWorkerCount { get; set; }
            public int? Active { get; set; }
        }
    }
}