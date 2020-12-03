namespace Hangfire.Configuration.Test.Infrastructure
{
    public class SystemUnderInfraTest : CompositionRoot
    {
        public IServerCountSampleStorage ServerCountSampleStorage => base.BuildServerCountSampleStorage();

        public void WithOptions(ConfigurationOptions configurationOptions)
        {
            BuildOptions().UseOptions(configurationOptions);
        }
    }
}