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
            HangfireConfiguration.SaveConfigurationInfo(ConnectionUtils.GetConnectionString(), "connectionString", null);
			
            var connectionString = HangfireConfiguration.ReadConnectionString(ConnectionUtils.GetConnectionString());
            Assert.Equal("connectionString", connectionString);
        }
        
        [Fact, CleanDatabase]
        public void ShouldRead()
        {
            HangfireConfiguration.SaveConfigurationInfo(ConnectionUtils.GetConnectionString(), "anotherConnectionString", null);
			
            var connectionString = HangfireConfiguration.ReadConnectionString(ConnectionUtils.GetConnectionString());
            
            Assert.Equal("anotherConnectionString", connectionString);
        }
        
        [Fact, CleanDatabase]
        public void ShouldUpdateWithConnectionString()
        {
            var repository = new ConfigurationRepository(ConnectionUtils.GetConnectionString());
            repository.WriteGoalWorkerCount(1);
            
            HangfireConfiguration.SaveConfigurationInfo(ConnectionUtils.GetConnectionString(), "connectionString", null);
			
            var connectionString = HangfireConfiguration.ReadConnectionString(ConnectionUtils.GetConnectionString());
            Assert.Equal("connectionString", connectionString);
        }
        
        [Fact, CleanDatabase]
        public void ShouldActivateOnSave()
        {
            HangfireConfiguration.SaveConfigurationInfo(ConnectionUtils.GetConnectionString(), "connectionString", null);
			
            var repository = new ConfigurationRepository(ConnectionUtils.GetConnectionString());
            var isActive = repository.IsActive();
            Assert.True(isActive);
        }
        
        [Fact, CleanDatabase]
        public void ShouldActivateOnUpdateWithConnectionString()
        {
            var repository = new ConfigurationRepository(ConnectionUtils.GetConnectionString());
            repository.WriteGoalWorkerCount(1);
            
            HangfireConfiguration.SaveConfigurationInfo(ConnectionUtils.GetConnectionString(), "connectionString", null);
			
            var isActive = repository.IsActive();
            Assert.True(isActive);
        }
        
        [Fact, CleanDatabase]
        public void ShouldSaveSchemaName()
        {
            HangfireConfiguration.SaveConfigurationInfo(ConnectionUtils.GetConnectionString(), "connectionString", "schemaName");
			
            var repository = new ConfigurationRepository(ConnectionUtils.GetConnectionString());
            var schemaName = repository.ReadConfiguration().SchemaName;
            Assert.Equal("schemaName", schemaName);
        }
        
        [Fact, CleanDatabase]
        public void ShouldSaveSchemaNameOnUpdate()
        {
            var repository = new ConfigurationRepository(ConnectionUtils.GetConnectionString());
            repository.WriteGoalWorkerCount(1);
            
            HangfireConfiguration.SaveConfigurationInfo(ConnectionUtils.GetConnectionString(), "connectionString", "schemaName");
            
            var schemaName = repository.ReadConfiguration().SchemaName;
            Assert.Equal("schemaName", schemaName);
        }
    }
}