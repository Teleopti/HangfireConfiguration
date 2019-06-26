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
    }
}