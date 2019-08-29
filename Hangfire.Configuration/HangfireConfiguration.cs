using System;
using System.Collections.Generic;
using System.Linq;
using Hangfire.Server;
using Hangfire.SqlServer;
using Owin;

namespace Hangfire.Configuration
{
    public class HangfireConfiguration
    {
        private readonly IAppBuilder _builder;
        private readonly ConfigurationOptions _options;
        private readonly CompositionRoot _compositionRoot;

        public HangfireConfiguration(IAppBuilder builder, ConfigurationOptions options)
        {
            _builder = builder;
            _options = options;
            _compositionRoot = new CompositionRoot(); 
        }

        public StartedHangfire Start(SqlServerStorageOptions storageOptions)
        {
            var serverStarter = _compositionRoot.BuildServerStarter(_builder, _options);
            var starter = _compositionRoot.BuildStarter(_options);
            var enabledStorages = starter.Start(_options, storageOptions);
            
            return new StartedHangfire(serverStarter, enabledStorages, _options);
        }
        
        
        [Obsolete("Dont use directly, will be removed")]
        public static WorkerDeterminer GetWorkerDeterminer(string connectionString) => 
            new CompositionRoot().BuildWorkerDeterminer(connectionString);
    }

    public class StartedHangfire
    {
        //private  IEnumerable<RunningServer> _runningServers = Enumerable.Empty<RunningServer>();

        public  IEnumerable<StorageWithConfiguration> EnabledStorages() => _enabledStorages;
        
        private readonly ServerStarter _serverStarter;
        private readonly IEnumerable<StorageWithConfiguration> _enabledStorages;
        private readonly ConfigurationOptions _options;

        public StartedHangfire(ServerStarter serverStarter, IEnumerable<StorageWithConfiguration> enabledStorages, ConfigurationOptions options)
        {
            _serverStarter = serverStarter;
            _enabledStorages = enabledStorages;
            _options = options;
        }
        
        public StartedHangfire WithServers(BackgroundJobServerOptions serverOptions, IBackgroundProcess[] additionalProcesses)
        {
            _serverStarter.StartServers(_options, serverOptions, _enabledStorages, additionalProcesses);
            return this;
        }
    }
    
}