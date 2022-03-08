using System;
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

    internal class ConfigurationAndStorage
    {
        public StoredConfiguration Configuration;
        public Func<JobStorage> JobStorageCreator;

        private JobStorage _jobStorage;
        private readonly object _lock = new object();

        public JobStorage CreateJobStorage()
        {
            if (_jobStorage != null)
                return _jobStorage;

            lock (_lock)
            {
                if (_jobStorage != null)
                    return _jobStorage;
                _jobStorage = JobStorageCreator.Invoke();
                JobStorageCreator = null;
                return _jobStorage;
            }
        }
    }

    internal static class ConfigurationAndStorageExtensions
    {
        internal static ConfigurationInfo ToConfigurationInfo(this ConfigurationAndStorage instance) =>
            new ConfigurationInfo
            {
                ConfigurationId = instance.Configuration.Id.Value,
                Name = instance.Configuration.Name,
                JobStorage = instance.CreateJobStorage()
            };
    }
}