using System.Collections.Generic;

namespace Hangfire.Configuration
{
    public interface IConfigurationRepository
    {
        IEnumerable<StoredConfiguration> ReadConfigurations();
        void WriteConfiguration(StoredConfiguration configuration);

        void ActivateStorage(int configurationId);
    }
}