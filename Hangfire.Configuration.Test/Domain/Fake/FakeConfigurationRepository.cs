using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Hangfire.Configuration.Test.Domain.Fake
{
    public static class Blip
    {
        public static T Copy<T>(this T instance)
        {
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(instance));
        }
    }
    
    public class FakeConfigurationRepository : IConfigurationRepository
    {
        public IEnumerable<StoredConfiguration> Data = Enumerable.Empty<StoredConfiguration>();
        public int? Workers => Data.FirstOrDefault()?.GoalWorkerCount;

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