using Hangfire.SqlServer;

namespace Hangfire.Configuration.Test
{
    public class PublisherStarterUnderTest
    {
        private readonly PublisherStarter _instance;
        private readonly Options _options;

        public PublisherStarterUnderTest(PublisherStarter instance, Options options)
        {
            _instance = instance;
            _options = options;
        }

        public void Start() => Start(null, null);

        public void Start(ConfigurationOptions options, SqlServerStorageOptions storageOptions)
        {
            if (options != null)
                _options.UseOptions(options);
            if (storageOptions != null)
                _options.UseStorageOptions(storageOptions);
            _instance.Start();
        }
    }
}