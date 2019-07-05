using Xunit;

namespace Hangfire.Configuration.Test.Infrastructure
{
    public class ConnectionStringTest
    {
        [Fact, CleanDatabase]
        public void ShouldReadEmpty()
        {
            var connectionString = HangfireConfiguration.ReadConnectionString(ConnectionUtils.GetConnectionString());
			
            Assert.Null(connectionString);
        }
        
        [Fact, CleanDatabase]
        public void ShouldSaveConnectionString()
        {
            HangfireConfiguration.SaveConnectionString(ConnectionUtils.GetConnectionString(), "connectionString");
			
            var connectionString = HangfireConfiguration.ReadConnectionString(ConnectionUtils.GetConnectionString());
            Assert.Equal("connectionString", connectionString);
        }
        
        [Fact, CleanDatabase]
        public void ShouldRead()
        {
            HangfireConfiguration.SaveConnectionString(ConnectionUtils.GetConnectionString(), "anotherConnectionString");
			
            var connectionString = HangfireConfiguration.ReadConnectionString(ConnectionUtils.GetConnectionString());
            
            Assert.Equal("anotherConnectionString", connectionString);
        }
    }
}