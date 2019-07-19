using System.Collections.Generic;

namespace Hangfire.Configuration.Test.Domain
{
    public class FakeConfigurationRepository : IConfigurationRepository
    {
        public int? Workers { get; private set; }
        private int? Id { get; set; }
        private string ConnectionString { get; set; }
        private string SchemaName { get; set; }
        private bool? Active { get; set; }

        public void WriteGoalWorkerCount(int? workers)
        {
            Workers = workers;
        }

        public int? ReadGoalWorkerCount()
        {
            return Workers;
        }

        public IEnumerable<StoredConfiguration> ReadConfiguration()
        {

            var configurations = new List<StoredConfiguration>();

            configurations.Add(new StoredConfiguration(){
                Id = Id,
                ConnectionString = ConnectionString,
                SchemaName = SchemaName,
                Workers = Workers,
                Active = Active
            });

            return configurations;
        }

        public void WriteNewStorageConfiguration(string connectionString, string schemaName, bool active)
        {
            ConnectionString = connectionString;
            SchemaName = schemaName;
            Active = active;
        }

        public void Has(StoredConfiguration configuration)
        {
            {
                Id = configuration.Id;
                Workers = configuration.Workers;
                ConnectionString = configuration.ConnectionString;
                SchemaName = configuration.SchemaName;
                Active = configuration.Active;
            }
        }
    }
}