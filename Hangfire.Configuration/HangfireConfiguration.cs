using System;
using System.Collections.Generic;
using System.Linq;
using Hangfire.Server;
using Hangfire.SqlServer;

namespace Hangfire.Configuration
{
    public class HangfireConfiguration
    {
        public  IEnumerable<EnabledStorage> EnabledJobStorages() => _enabledStorages;

        private readonly object _builder;
        private readonly ConfigurationOptions _options;
        private readonly CompositionRoot _compositionRoot;
        private  IEnumerable<StorageWithConfiguration> _storagesWithConfiguration = Enumerable.Empty<StorageWithConfiguration>();
        private  IEnumerable<EnabledStorage> _enabledStorages = Enumerable.Empty<EnabledStorage>();
        
        public HangfireConfiguration(object builder, ConfigurationOptions options)
        {
            _builder = builder;
            _options = options;
            _compositionRoot = new CompositionRoot(); 
        }

        public HangfireConfiguration StartPublishers(SqlServerStorageOptions storageOptions)
        {
            var starter = _compositionRoot.BuildStarter(_options);
            _storagesWithConfiguration = starter.Start(_options, storageOptions);

            _enabledStorages = _storagesWithConfiguration.Select((s, i) =>
            {
                return new EnabledStorage
                {
                    Number = i + 1,
                    JobStorage = s.JobStorage
                };
            }).ToArray();
            
            return this;
        }
        
        public HangfireConfiguration StartWorkers(SqlServerStorageOptions storageOptions, BackgroundJobServerOptions serverOptions, IBackgroundProcess[] additionalProcesses)
        {
            StartPublishers(storageOptions);
            var serverStarter = _compositionRoot.BuildServerStarter(_builder);
            serverStarter.StartServers(_options, serverOptions, _storagesWithConfiguration, additionalProcesses);
            return this;
        }
        
        
        [Obsolete("Dont use directly, will be removed")]
        public static WorkerDeterminer GetWorkerDeterminer(string connectionString) => 
            new CompositionRoot().BuildWorkerDeterminer(connectionString);
    }

    public class EnabledStorage
    {
        public int Number;
        public JobStorage JobStorage;
    }
}