using System.Data.SqlClient;
using System.Linq;
using Xunit;

namespace Hangfire.Configuration.Test.Infrastructure
{
    [Collection("Infrastructure")]
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
    }
}