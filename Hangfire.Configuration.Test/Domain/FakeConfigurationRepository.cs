using System;
using System.Collections.Generic;
using System.Linq;

namespace Hangfire.Configuration.Test.Domain
{
    public class FakeConfigurationRepository : IConfigurationRepository
    {
        public int? Workers { get; private set; }
        private int? Id { get; set; }
        private string ConnectionString { get; set; }
        private string SchemaName { get; set; }
        private bool? Active { get; set; }
        
        private IEnumerable<StoredConfiguration> Data = Enumerable.Empty<StoredConfiguration>();

        public void WriteGoalWorkerCount(int? workers)
        {
            Workers = workers;
        }

        public int? ReadGoalWorkerCount()
        {
            return Data.Single().Workers;
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

        public void Has(StoredConfiguration configuration)
        {
            Data = Data.Append(new StoredConfiguration(){
                Id = configuration.Id,
                Workers = configuration.Workers,
                ConnectionString = configuration.ConnectionString,
                SchemaName = configuration.SchemaName,
                Active = configuration.Active
            });
        }

        public void Has(IEnumerable<StoredConfiguration> configuration)
        {
            foreach (var storedConfiguration in configuration)
            {
                Data = Data.Append(new StoredConfiguration(){
                    Id = storedConfiguration.Id,
                    Workers = storedConfiguration.Workers,
                    ConnectionString = storedConfiguration.ConnectionString,
                    SchemaName = storedConfiguration.SchemaName,
                    Active = storedConfiguration.Active
                });
            }
        }
    }
}