using System.Collections.Generic;
using Hangfire.SqlServer;

namespace Hangfire.Configuration.Test
{
    public class PublisherQueriesUnderTest
    {
        private readonly PublisherQueries _instance;
        private readonly Options _options;

        public PublisherQueriesUnderTest(PublisherQueries instance, Options options)
        {
            _instance = instance;
            _options = options;
        }

        public IEnumerable<ConfigurationInfo> QueryPublishers()
            => QueryPublishers(null, null);
        
        public IEnumerable<ConfigurationInfo> QueryPublishers(ConfigurationOptions options, SqlServerStorageOptions storageOptions)
        {
            if (options != null)
                _options.UseOptions(options);
            if (storageOptions != null)
                _options.UseStorageOptions(storageOptions);
            return _instance.QueryPublishers();
        }
    }
}