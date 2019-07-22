using System.Collections.Generic;

namespace Hangfire.Configuration
{
    public interface IConfigurationRepository
    {
        IEnumerable<StoredConfiguration> ReadConfigurations();
        void WriteConfiguration(StoredConfiguration configuration);

        int? ReadGoalWorkerCount();
        IEnumerable<StoredConfiguration> ReadConfiguration();
        void WriteNewStorageConfiguration(string connectionString, string schemaName, bool active);
        void ActivateStorage(int configurationId);
    }
}