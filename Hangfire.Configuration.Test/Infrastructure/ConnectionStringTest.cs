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
        
        [Fact, CleanDatabase]
        public void ShouldUpdateWithConnectionString()
        {
            var repository = new ConfigurationRepository(ConnectionUtils.GetConnectionString());
            repository.WriteGoalWorkerCount(1);
            
            HangfireConfiguration.SaveConnectionString(ConnectionUtils.GetConnectionString(), "connectionString");
			
            var connectionString = HangfireConfiguration.ReadConnectionString(ConnectionUtils.GetConnectionString());
            Assert.Equal("connectionString", connectionString);
        }
        
        [Fact, CleanDatabase]
        public void ShouldActivateOnSave()
        {
            HangfireConfiguration.SaveConnectionString(ConnectionUtils.GetConnectionString(), "connectionString");
			
            var repository = new ConfigurationRepository(ConnectionUtils.GetConnectionString());
            var isActive = repository.IsActive();
            Assert.True(isActive);
        }
        
        [Fact, CleanDatabase]
        public void ShouldActivateOnUpdateWithConnectionString()
        {
            var repository = new ConfigurationRepository(ConnectionUtils.GetConnectionString());
            repository.WriteGoalWorkerCount(1);
            
            HangfireConfiguration.SaveConnectionString(ConnectionUtils.GetConnectionString(), "connectionString");
			
            var isActive = repository.IsActive();
            Assert.True(isActive);
        }
    }
}