namespace Hangfire.Configuration.Test.Domain.Fake
{
    public class ConfigurationOptionsForTest : ConfigurationOptions
    {
        public int MinimumServerCount
        {
            set => WorkerDeterminerOptions.MinimumServerCount = value;
        }

        public bool UseServerCountSampling
        {
            set => WorkerDeterminerOptions.UseServerCountSampling = value;
        }

        public int MaximumGoalWorkerCount
        {
            set => WorkerDeterminerOptions.MaximumGoalWorkerCount = value;
        }

        public int DefaultGoalWorkerCount
        {
            set => WorkerDeterminerOptions.DefaultGoalWorkerCount = value;
        }

        public int MinimumWorkerCount
        {
            set => WorkerDeterminerOptions.MinimumWorkerCount = value;
        }
    }
}