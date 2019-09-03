namespace Hangfire.Configuration
{
    public class ConfigurationOptions
    {
        public string ConnectionString { get; set; }

        public string AutoUpdatedHangfireConnectionString { get; set; }
        public string AutoUpdatedHangfireSchemaName { get; set; }

        public bool UseWorkerDeterminer { get; set; } = true;
        public int DefaultGoalWorkerCount { get; set; } = 10;
        public int MinimumWorkerCount { get; set; } = 1;
        public int MaximumGoalWorkerCount { get; set; } = 100;
        public int MinimumServers { get; set; } = 2;
    }
}