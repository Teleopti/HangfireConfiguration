using System;
using System.Collections.Generic;
using System.Linq;

namespace Hangfire.Configuration.Test.Domain.Fake
{
    public class FakeConfigurationStorage : IConfigurationStorage
    {
        public IEnumerable<StoredConfiguration> Data = Enumerable.Empty<StoredConfiguration>();
        public int? Workers => Data.FirstOrDefault()?.GoalWorkerCount;
        public int? MaxWorkersPerServer => Data.FirstOrDefault()?.MaxWorkersPerServer;

        private int _nextId = 1;
        private int NextId() => _nextId++;

        public IEnumerable<StoredConfiguration> ReadConfigurations() =>
            Data.Select(x => x.Copy()).ToArray();

        public void WriteConfiguration(StoredConfiguration configuration)
        {
            configuration = configuration.Copy();
            if (configuration.Id != null)
                Data = Data.Where(x => x.Id != configuration.Id).ToArray();
            var existing = Data.SingleOrDefault(x => x.Id == configuration.Id);
            configuration.Id = existing?.Id ?? configuration.Id ?? NextId();
            Data = Data.Append(configuration).OrderBy(x => x.Id).ToArray();
        }

        public void Transaction(Action action) => 
            action.Invoke();

        public void LockConfiguration(){}

        public void HasGoalWorkerCount(int goalWorkerCount) => Has(new StoredConfiguration {GoalWorkerCount = goalWorkerCount});

        public void Has(StoredConfiguration configuration)
        {
            configuration.Id = configuration.Id ?? NextId();
            Data = Data.Append(configuration.Copy()).ToArray();
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