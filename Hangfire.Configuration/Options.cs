using Hangfire.PostgreSql;
using Hangfire.Pro.Redis;
using Hangfire.SqlServer;

namespace Hangfire.Configuration
{
    public class Options
    {
        private readonly State _state;

        internal Options(State state)
        {
            _state = state;
        }

        public void UseOptions(ConfigurationOptions options) =>
            _state.Options = options;

        public void UseStorageOptions(SqlServerStorageOptions storageOptions) =>
            _state.StorageOptionsSqlServer = storageOptions;

        public void UseStorageOptions(PostgreSqlStorageOptions storageOptions) =>
	        _state.StorageOptionsPostgreSql = storageOptions;
        
        public void UseStorageOptions(RedisStorageOptions storageOptions) =>
	        _state.StorageOptionsRedis = storageOptions;

		public void UseServerOptions(BackgroundJobServerOptions serverOptions) =>
            _state.ServerOptions = serverOptions;

        public ConfigurationOptions ConfigurationOptions() =>
            _state.ReadOptions();
    }
}