using System;
using System.Data.SqlClient;
using Dapper;
using Npgsql;
using Xunit;

namespace Hangfire.Configuration.Test.Infrastructure
{
    [Collection("NotParallel")]
    public class SchemaCreatorTest
    {
        [Fact, CleanDatabase]
        public void ShouldConnect()
        {
            var creator = new HangfireSchemaCreator();
            
            creator.TryConnect(ConnectionUtils.GetConnectionString());
        }

        [Fact]
        public void ShouldThrowSqlExceptionWhenNoDatabaseSqlServer()
        {
            var creator = new HangfireSchemaCreator();
            
            Assert.ThrowsAny<SqlException>(() => creator.TryConnect(@"Server=.\;Database=DoesNotExist;Trusted_Connection=True;"));
        }

        [Fact(Skip = "No db yet")]
        public void ShouldThrowSqlExceptionWhenNoDatabasePostgreSql()
        {
	        var creator = new HangfireSchemaCreator();

	        Assert.ThrowsAny<PostgresException>(() => creator.TryConnect(@"User ID=postgres;Password=postgres;Host=localhost;Database=""DoesNotExist"";CommandTimeout=30;Pooling=false;"));
        }

		[Fact, CleanDatabase]
        public void ShouldCreateSchema()
        {
            var creator = new HangfireSchemaCreator();
            
            creator.CreateHangfireSchema("hangfiretestschema", ConnectionUtils.GetConnectionString());

            var dialectSelector = new ConnectionStringDialectSelector(ConnectionUtils.GetConnectionString());
            using (var conn = dialectSelector.GetConnection())
            {
	            Assert.Equal("hangfiretestschema", conn.ExecuteScalar<string>("SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = 'hangfiretestschema'"));
            }
        }

        [Fact, CleanDatabase]
        public void ShouldCreateSchemaWithDefaultSchema()
        {
	        var creator = new HangfireSchemaCreator();

	        creator.CreateHangfireSchema("", ConnectionUtils.GetConnectionString());

	        var dialectSelector = new ConnectionStringDialectSelector(ConnectionUtils.GetConnectionString());
	        using (var conn = dialectSelector.GetConnection())
	        {
		        if (dialectSelector.IsPostgreSql())
		        {
			        Assert.Equal("hangfire", conn.ExecuteScalar<string>("SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = 'hangfire'"));
				}
		        else
		        {
			        Assert.Equal("HangFire", conn.ExecuteScalar<string>("SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = 'HangFire'"));
				}
	        }
        }

		[Fact, CleanDatabase]
        public void ShouldThrowOnCreateWhenInvalidConnectionString()
        {
            var creator = new HangfireSchemaCreator();
            
            Assert.ThrowsAny<Exception>(() => creator.CreateHangfireSchema("HangfireTestSchema", "InvalidConnectionString"));
        }
        
        [Fact, CleanDatabase]
        public void ShouldIndicateThatSchemaExists()
        {
            var creator = new HangfireSchemaCreator();
            creator.CreateHangfireSchema("schema", ConnectionUtils.GetConnectionString());
            
            var result = creator.SchemaExists("schema", ConnectionUtils.GetConnectionString());
            
            Assert.True(result);
        }
        
        [Fact, CleanDatabase]
        public void ShouldIndicateThatSchemaDoesNotExists()
        {
            var creator = new HangfireSchemaCreator();
            var result = creator.SchemaExists("nonExistingSchema", ConnectionUtils.GetConnectionString());
            Assert.False(result);
        }
    }
}