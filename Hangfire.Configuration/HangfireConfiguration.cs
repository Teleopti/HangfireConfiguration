using System.Linq;

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

        public static string ReadActiveConfigurationConnectionString(string connectionString)
        {
            var repository = new ConfigurationRepository(connectionString);
            return repository.ReadActiveConfigurationConnectionString();
        }

        public static void SaveDefaultConfiguration(string connectionString, string connectionStringToBeStored, string schemaName)
        {
            var repository = new ConfigurationRepository(connectionString);
            repository.WriteDefaultConfiguration(connectionStringToBeStored, schemaName);
        }
    }
}