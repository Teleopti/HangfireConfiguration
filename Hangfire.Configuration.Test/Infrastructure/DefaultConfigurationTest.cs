using Xunit;

namespace Hangfire.Configuration.Test.Infrastructure
{
    public class DefaultConfigurationTest
    {
        [Fact, CleanDatabase]
        public void ShouldReadEmptyConnectionString()
        {
            var connectionString = HangfireConfiguration.ReadConnectionString(ConnectionUtils.GetConnectionString());
			
            Assert.Null(connectionString);
        }
        
        [Fact, CleanDatabase]
        public void ShouldSaveConnectionString()
        {
            HangfireConfiguration.SaveDefaultConfiguration(ConnectionUtils.GetConnectionString(), "connectionString", null);
			
            var connectionString = HangfireConfiguration.ReadConnectionString(ConnectionUtils.GetConnectionString());
            Assert.Equal("connectionString", connectionString);
        }
        
        [Fact, CleanDatabase]
        public void ShouldReadConnectionString()
        {
            HangfireConfiguration.SaveDefaultConfiguration(ConnectionUtils.GetConnectionString(), "connectionString", null);
			
            var connectionString = HangfireConfiguration.ReadConnectionString(ConnectionUtils.GetConnectionString());
            
            Assert.Equal("connectionString", connectionString);
        }
        
        [Fact, CleanDatabase]
        public void ShouldUpdateWithConnectionString()
        {
            var repository = new ConfigurationRepository(ConnectionUtils.GetConnectionString());
            repository.WriteGoalWorkerCount(1);
            
            HangfireConfiguration.SaveDefaultConfiguration(ConnectionUtils.GetConnectionString(), "connectionString", null);
			
            var connectionString = HangfireConfiguration.ReadConnectionString(ConnectionUtils.GetConnectionString());
            Assert.Equal("connectionString", connectionString);
        }
        
        [Fact, CleanDatabase]
        public void ShouldReadEmptyDefaultConfiguration()
        {
            var repository = new ConfigurationRepository(ConnectionUtils.GetConnectionString());
			
            Assert.Null(repository.ReadConfiguration());
        }
		
        [Fact, CleanDatabase]
        public void ShouldReadDefaultConfiguration()
        {
            var repository = new ConfigurationRepository(ConnectionUtils.GetConnectionString());
            repository.WriteGoalWorkerCount(1);
            repository.SaveDefaultConfiguration("connectionString", "schemaName");

            var result = repository.ReadConfiguration();
			
            Assert.Equal(1, result.Id);
            Assert.Equal("connectionString", result.ConnectionString);
            Assert.Equal("schemaName", result.SchemaName);
            Assert.Equal(1, result.Workers);
            Assert.Equal(true, result.Active);
        }
        
        [Fact, CleanDatabase]
        public void ShouldActivateOnSave()
        {
            HangfireConfiguration.SaveDefaultConfiguration(ConnectionUtils.GetConnectionString(), "connectionString", null);
			
            var repository = new ConfigurationRepository(ConnectionUtils.GetConnectionString());
            Assert.True(repository.ReadConfiguration().Active);
        }
        
        [Fact, CleanDatabase]
        public void ShouldActivateOnUpdateWithConnectionString()
        {
            var repository = new ConfigurationRepository(ConnectionUtils.GetConnectionString());
            repository.WriteGoalWorkerCount(1);
            
            HangfireConfiguration.SaveDefaultConfiguration(ConnectionUtils.GetConnectionString(), "connectionString", null);
			
            Assert.True(repository.ReadConfiguration().Active);
        }
        
        [Fact, CleanDatabase]
        public void ShouldSaveSchemaName()
        {
            HangfireConfiguration.SaveDefaultConfiguration(ConnectionUtils.GetConnectionString(), "connectionString", "schemaName");
			
            var repository = new ConfigurationRepository(ConnectionUtils.GetConnectionString());
            var schemaName = repository.ReadConfiguration().SchemaName;
            Assert.Equal("schemaName", schemaName);
        }
        
        [Fact, CleanDatabase]
        public void ShouldSaveSchemaNameOnUpdate()
        {
            var repository = new ConfigurationRepository(ConnectionUtils.GetConnectionString());
            repository.WriteGoalWorkerCount(1);
            
            HangfireConfiguration.SaveDefaultConfiguration(ConnectionUtils.GetConnectionString(), "connectionString", "schemaName");
            
            var schemaName = repository.ReadConfiguration().SchemaName;
            Assert.Equal("schemaName", schemaName);
        }
    }
}