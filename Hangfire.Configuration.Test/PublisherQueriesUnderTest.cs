using System.Collections.Generic;
using Hangfire.PostgreSql;
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
            => _instance.QueryPublishers();
        
        public IEnumerable<ConfigurationInfo> QueryPublishers(ConfigurationOptions options)
        {
	        if (options != null)
		        _options.UseOptions(options);
	        return _instance.QueryPublishers();
        }
    }
}