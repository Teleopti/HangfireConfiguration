using System;
using System.Collections.Generic;
using System.Linq;
using Hangfire.Server;
using Hangfire.SqlServer;

namespace Hangfire.Configuration
{
    public class HangfireConfiguration
    {
        private IEnumerable<EnabledStorage> _enabledStorages = Enumerable.Empty<EnabledStorage>();
        public IEnumerable<EnabledStorage> EnabledJobStorages() => _enabledStorages;
        
        private readonly object _builder;
        private readonly ConfigurationOptions _options;
        private readonly CompositionRoot _compositionRoot;

        public HangfireConfiguration(object builder, ConfigurationOptions options)
        {
            _builder = builder;
            _options = options;
            _compositionRoot = new CompositionRoot();
        }

        public HangfireConfiguration StartPublishers(SqlServerStorageOptions storageOptions)
        {
            var starter = _compositionRoot.BuildPublisherStarter(new ConfigurationConnection {ConnectionString = _options.ConnectionString});
            _enabledStorages = starter.Start(_options, storageOptions)
                .Select((s, i) => new EnabledStorage
                {
                    Number = i + 1,
                    JobStorage = s
                }).ToArray();
            return this;
        }

        public HangfireConfiguration StartWorkerServers(SqlServerStorageOptions storageOptions, BackgroundJobServerOptions serverOptions, IBackgroundProcess[] additionalProcesses)
        {
            _compositionRoot.BuildWorkerServerStarter(
                    _builder,
                    new ConfigurationConnection {ConnectionString = _options.ConnectionString}
                )
                .Start(_options, serverOptions, storageOptions, additionalProcesses);
            return this;
        }

        [Obsolete("Dont use directly, will be removed")]
        public static WorkerDeterminer GetWorkerDeterminer(string connectionString) =>
            new CompositionRoot().BuildWorkerDeterminer(new ConfigurationConnection {ConnectionString = connectionString});
    }

    public class EnabledStorage
    {
        public int Number;
        public JobStorage JobStorage;
    }
}