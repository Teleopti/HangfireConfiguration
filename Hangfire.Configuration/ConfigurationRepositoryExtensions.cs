using System.Linq;

namespace Hangfire.Configuration
{
    public static class ConfigurationRepositoryExtensions
    {
        public static void WriteGoalWorkerCount(this IConfigurationRepository repository, int? workers)
        {
            var configurations = repository.ReadConfigurations();
            var configuration = new StoredConfiguration();
            if (configurations.Any())
            {
                configuration = configurations.FirstOrDefault(x => x.Active.GetValueOrDefault());
                if (configuration == null)
                    configuration = configurations.First();
            }
            configuration.GoalWorkerCount = workers;
            repository.WriteConfiguration(configuration);
        }
    }
}