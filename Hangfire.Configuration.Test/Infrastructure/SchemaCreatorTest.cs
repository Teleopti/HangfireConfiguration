using System;
using System.Data.SqlClient;
using Dapper;
using Npgsql;
using NUnit.Framework;

namespace Hangfire.Configuration.Test.Infrastructure
{
    [Parallelizable(ParallelScope.None)]
    [CleanDatabase]
    public class SchemaCreatorTest
    {
        [Test]
        public void ShouldConnect()
        {
            var creator = new HangfireSchemaCreator();
            
            creator.TryConnect(ConnectionUtils.GetConnectionString());
        }

        [Test]
        public void ShouldThrowSqlExceptionWhenNoDatabaseSqlServer()
        {
            var creator = new HangfireSchemaCreator();
            
            Assert.Throws<SqlException>(() => creator.TryConnect(@"Server=.\;Database=DoesNotExist;Trusted_Connection=True;"));
        }

        [Test]
        [Ignore("No db yet")]
        public void ShouldThrowSqlExceptionWhenNoDatabasePostgreSql()
        {
	        var creator = new HangfireSchemaCreator();

	        Assert.Throws<PostgresException>(() => creator.TryConnect(@"User ID=postgres;Password=postgres;Host=localhost;Database=""DoesNotExist"";CommandTimeout=30;Pooling=false;"));
        }

		[Test]
        public void ShouldCreateSchema()
        {
            var creator = new HangfireSchemaCreator();
            
            creator.CreateHangfireSchema("hangfiretestschema", ConnectionUtils.GetConnectionString());

            var dialectSelector = new ConnectionStringDialectSelector(ConnectionUtils.GetConnectionString());
            using (var conn = dialectSelector.GetConnection())
            {
	            Assert.AreEqual("hangfiretestschema", conn.ExecuteScalar<string>("SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = 'hangfiretestschema'"));
            }
        }

        [Test]
        public void ShouldCreateSchemaWithDefaultSchema()
        {
	        var creator = new HangfireSchemaCreator();

	        creator.CreateHangfireSchema("", ConnectionUtils.GetConnectionString());

	        var dialectSelector = new ConnectionStringDialectSelector(ConnectionUtils.GetConnectionString());
	        using (var conn = dialectSelector.GetConnection())
	        {
		        if (dialectSelector.IsPostgreSql())
		        {
			        Assert.AreEqual("hangfire", conn.ExecuteScalar<string>("SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = 'hangfire'"));
				}
		        else
		        {
			        Assert.AreEqual("HangFire", conn.ExecuteScalar<string>("SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = 'HangFire'"));
				}
	        }
        }

		[Test]
        public void ShouldThrowOnCreateWhenInvalidConnectionString()
        {
            var creator = new HangfireSchemaCreator();
            
            Assert.Throws<Exception>(() => creator.CreateHangfireSchema("HangfireTestSchema", "InvalidConnectionString"));
        }
        
        [Test]
        public void ShouldIndicateThatSchemaExists()
        {
            var creator = new HangfireSchemaCreator();
            creator.CreateHangfireSchema("schema", ConnectionUtils.GetConnectionString());
            
            var result = creator.SchemaExists("schema", ConnectionUtils.GetConnectionString());
            
            Assert.True(result);
        }
        
        [Test]
        public void ShouldIndicateThatSchemaDoesNotExists()
        {
            var creator = new HangfireSchemaCreator();
            var result = creator.SchemaExists("nonExistingSchema", ConnectionUtils.GetConnectionString());
            Assert.False(result);
        }
    }
}