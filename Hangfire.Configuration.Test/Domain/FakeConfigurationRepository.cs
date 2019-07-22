using System;
using System.Collections.Generic;
using System.Linq;

namespace Hangfire.Configuration.Test.Domain
{
    public class FakeConfigurationRepository : IConfigurationRepository
    {
        private int? Id { get; set; }
        private string ConnectionString { get; set; }
        private string SchemaName { get; set; }
        private bool? Active { get; set; }
        private IEnumerable<StoredConfiguration> Data = Enumerable.Empty<StoredConfiguration>();
        
        public int? Workers => Data.FirstOrDefault()?.GoalWorkerCount;

        public IEnumerable<StoredConfiguration> ReadConfigurations()
        {
            return Data.ToArray();
        }

        public void WriteConfiguration(StoredConfiguration configuration)
        {
            if (configuration.Id != null)
                Data = Data.Where(x => x.Id != configuration.Id).ToArray();
            Data = Data.Append(configuration).ToArray();
        }

        public IEnumerable<StoredConfiguration> ReadConfiguration()
        {
            return Data.ToArray();
        }

        public void WriteNewStorageConfiguration(string connectionString, string schemaName, bool active)
        {
            Data = Data.Append(new StoredConfiguration()
            {
                ConnectionString = connectionString,
                SchemaName = schemaName,
                Active = active
            });
        }

        public void ActivateStorage(int configurationId)
        {
            foreach (var d in Data)
            {
                d.Active = d.Id == configurationId;
            }
        }

        public void Has(StoredConfiguration configuration)
        {
            Data = Data.Append(new StoredConfiguration()
            {
                Id = configuration.Id,
                GoalWorkerCount = configuration.GoalWorkerCount,
                ConnectionString = configuration.ConnectionString,
                SchemaName = configuration.SchemaName,
                Active = configuration.Active
            });
        }

        public void Has(IEnumerable<StoredConfiguration> configuration)
        {
            foreach (var storedConfiguration in configuration)
            {
                Data = Data.Append(new StoredConfiguration()
                {
                    Id = storedConfiguration.Id,
                    GoalWorkerCount = storedConfiguration.GoalWorkerCount,
                    ConnectionString = storedConfiguration.ConnectionString,
                    SchemaName = storedConfiguration.SchemaName,
                    Active = storedConfiguration.Active
                });
            }
        }
    }
}