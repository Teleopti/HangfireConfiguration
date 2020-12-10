namespace Hangfire.Configuration.Test.Infrastructure
{
    public class SystemUnderInfraTest : CompositionRoot
    {
        public IKeyValueStore KeyValueStore => base.BuildKeyValueStore();
        public IConfigurationStorage ConfigurationStorage => base.BuildConfigurationStorage();

        public void WithOptions(ConfigurationOptions configurationOptions)
        {
            BuildOptions().UseOptions(configurationOptions);
        }
    }
}