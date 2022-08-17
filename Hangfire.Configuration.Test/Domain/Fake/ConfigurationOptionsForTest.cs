namespace Hangfire.Configuration.Test.Domain.Fake
{
    public class ConfigurationOptionsForTest : ConfigurationOptions
    {
        public int MinimumServerCount
        {
            set => WorkerBalancerOptions.MinimumServerCount = value;
        }

        public bool UseServerCountSampling
        {
            set => WorkerBalancerOptions.UseServerCountSampling = value;
        }

        public int MaximumGoalWorkerCount
        {
            set => WorkerBalancerOptions.MaximumGoalWorkerCount = value;
        }

        public int DefaultGoalWorkerCount
        {
            set => WorkerBalancerOptions.DefaultGoalWorkerCount = value;
        }

        public int MinimumWorkerCount
        {
            set => WorkerBalancerOptions.MinimumWorkerCount = value;
        }
    }
}