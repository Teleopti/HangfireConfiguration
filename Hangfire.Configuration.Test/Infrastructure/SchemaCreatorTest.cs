using System;
using System.Data.SqlClient;
using Dapper;
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
        public void ShouldThrowSqlExceptionWhenNoDatabase()
        {
            var creator = new HangfireSchemaCreator();
            
            Assert.ThrowsAny<SqlException>(() => creator.TryConnect(@"Server=.\;Database=DoesNotExist;Trusted_Connection=True;"));
        }
        
        [Fact, CleanDatabase]
        public void ShouldCreateSchema()
        {
            var creator = new HangfireSchemaCreator();
            
            creator.CreateHangfireSchema("HangfireTestSchema", ConnectionUtils.GetConnectionString());

            using (var conn = new SqlConnection(ConnectionUtils.GetConnectionString()))
            {
                Assert.Equal("HangfireTestSchema", conn.ExecuteScalar<string>("SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = 'HangfireTestSchema'"));
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