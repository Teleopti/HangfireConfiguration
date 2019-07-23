using System.Collections.Generic;
using System.Linq;

namespace Hangfire.Configuration.Test.Domain
{
    public class FakeConfigurationRepository : IConfigurationRepository
    {
        public IEnumerable<StoredConfiguration> Data = Enumerable.Empty<StoredConfiguration>();
        public int? Workers => Data.FirstOrDefault()?.GoalWorkerCount;

        private int _id;
        private int NextId() => _id++;

        public IEnumerable<StoredConfiguration> ReadConfigurations() => Data.ToArray();

        public void WriteConfiguration(StoredConfiguration configuration)
        {
            if (configuration.Id != null)
                Data = Data.Where(x => x.Id != configuration.Id).ToArray();
            configuration.Id = configuration.Id ?? NextId();
            Data = Data.Append(configuration).ToArray();
        }

        public void HasGoalWorkerCount(int goalWorkerCount) => Has(new StoredConfiguration {GoalWorkerCount = goalWorkerCount});

        public void Has(StoredConfiguration configuration)
        {
            Data = Data.Append(new StoredConfiguration()
            {
                Id = configuration.Id ?? NextId(),
                GoalWorkerCount = configuration.GoalWorkerCount,
                ConnectionString = configuration.ConnectionString,
                SchemaName = configuration.SchemaName,
                Active = configuration.Active
            });
        }

        public void Has(params StoredConfiguration[] configuration)
        {
            foreach (var storedConfiguration in configuration)
            {
                Has(storedConfiguration);
            }
        }
    }
}