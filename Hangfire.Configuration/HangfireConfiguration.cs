namespace Hangfire.Configuration
{
    public class HangfireConfiguration
    {
        private static Configuration BuildConfiguration(string connectionString)
        {
            var repository = new ConfigurationRepository(connectionString);
            var configuration = new Configuration(repository);
            return configuration;
        }
        
        public static WorkerDeterminer GetWorkerDeterminer(string connectionString)
        {
            var configuration = BuildConfiguration(connectionString);
            return new WorkerDeterminer(configuration, JobStorage.Current.GetMonitoringApi());
        }
        
        public static string ReadActiveConfigurationConnectionString(string connectionString)
        {
            var configuration = BuildConfiguration(connectionString);
            return configuration.ReadActiveConfigurationConnectionString();
        }

        public static void ConfigureDefaultStorage(string connectionString, string defaultHangfireConnectionString, string schemaName)
        {
            var configuration = BuildConfiguration(connectionString);
            configuration.ConfigureDefaultStorage(defaultHangfireConnectionString, schemaName);
        }
    }
}