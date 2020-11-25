using System.Collections.Generic;

namespace Hangfire.Configuration
{
    public class ConfigurationOptions
    {
        public string ConnectionString { get; set; }

        public bool PrepareSchemaIfNecessary { get; set; }
        public bool AllowNewServerCreation { get; set; }
        public bool AllowMultipleActive { get; set; } = false;

        public string AutoUpdatedHangfireConnectionString { get; set; }
        public string AutoUpdatedHangfireSchemaName { get; set; }
        public IEnumerable<UpdateConfiguration> UpdateConfigurations { get; set; }

        public bool UseWorkerDeterminer { get; set; } = true;
        public int DefaultGoalWorkerCount { get; set; } = 10;
        public int MaximumGoalWorkerCount { get; set; } = 100;
        public int MinimumWorkerCount { get; set; } = 1;
        
        public int? MinimumKnownWorkerServerCount { get; set; }
    }

    public class UpdateConfiguration
    {
        public string Name { get; set; }
        public string ConnectionString { get; set; }
        public string SchemaName { get; set; }
    }
}