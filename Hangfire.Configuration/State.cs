using System.Collections.Generic;
using System.Linq;
using Hangfire.PostgreSql;
using Hangfire.Pro.Redis;
using Hangfire.SqlServer;

namespace Hangfire.Configuration
{
    internal class State
    {
        public ConfigurationOptions Options { private get; set; }
        public SqlServerStorageOptions StorageOptionsSqlServer { get; set; }
        public PostgreSqlStorageOptions StorageOptionsPostgreSql { get; set; }
        public RedisStorageOptions StorageOptionsRedis { get; set; }
        public BackgroundJobServerOptions ServerOptions { get; set; }

        public IEnumerable<ConfigurationAndStorage> Configurations = Enumerable.Empty<ConfigurationAndStorage>();
        public bool ConfigurationUpdaterRan { get; set; }

        public ConfigurationOptions ReadOptions() => 
            Options ?? new ConfigurationOptions();
    }
}