namespace Hangfire.Configuration
{
    public class HangfireConfiguration
    {
        public static WorkerDeterminer GetWorkerDeterminer(string connectionString)
        {
            var repository = new ConfigurationRepository(connectionString);
            var configuration = new Configuration(repository);
            return new WorkerDeterminer(configuration, JobStorage.Current.GetMonitoringApi());
        }

        public static string ReadConnectionString(string connectionString)
        {
            var repository = new ConfigurationRepository(connectionString);
            return repository.ReadConnectionString();
        }

        public static void SaveConnectionString(string connectionString, string connectionStringToBeStored)
        {
            var repository = new ConfigurationRepository(connectionString);
            repository.SaveConnectionString(connectionStringToBeStored);
        }
    }
}