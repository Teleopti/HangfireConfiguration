using System.Collections.Generic;

namespace Hangfire.Configuration
{
    public class StoredConfiguration
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string ConnectionString { get; set; }
        public string SchemaName { get; set; }
        public int? GoalWorkerCount { get; set; }
        public bool? Active { get; set; }
        public int? MaxWorkersPerServer { get; set; }
    }
}