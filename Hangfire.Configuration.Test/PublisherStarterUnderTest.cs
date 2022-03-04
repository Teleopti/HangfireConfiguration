using Hangfire.PostgreSql;
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
   //
   //      public void Start()
   //      {
			// new ConnectionStringDialectSelector(ConnectionUtils.GetConnectionString())
			// 	.SelectDialectVoid(
			// 		() => Start(null, (SqlServerStorageOptions)null), 
			// 		() => Start(null, (PostgreSqlStorageOptions)null));
   //      }

        public void Start()
        {
	        _instance.Start();
        }

        public void Start(ConfigurationOptions options, SqlServerStorageOptions storageOptions)
        {
            if (options != null)
                _options.UseOptions(options);
            if (storageOptions != null)
                _options.UseStorageOptions(storageOptions);
            _instance.Start();
        }

        public void Start(ConfigurationOptions options, PostgreSqlStorageOptions storageOptions)
        {
	        if (options != null)
		        _options.UseOptions(options);
	        if (storageOptions != null)
		        _options.UseStorageOptions(storageOptions);
	        _instance.Start();
        }
	}
}