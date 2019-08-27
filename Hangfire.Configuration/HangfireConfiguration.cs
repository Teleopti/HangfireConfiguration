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
        private static IEnumerable<RunningServer> _runningServers = Enumerable.Empty<RunningServer>();

        public static IEnumerable<RunningServer> RunningServers() => _runningServers;

        private readonly IAppBuilder _builder;
        private readonly ConfigurationOptions _options;
        private readonly CompositionRoot _compositionRoot;

        public HangfireConfiguration(IAppBuilder builder, ConfigurationOptions options)
        {
            _builder = builder;
            _options = options;
            _compositionRoot = new CompositionRoot(); 
        }

        public HangfireConfiguration StartServers(BackgroundJobServerOptions serverOptions, IBackgroundProcess[] additionalProcesses)
        {
            _runningServers = _compositionRoot.BuildServerStarter(_builder, _options)
                .StartServers(_options, serverOptions, additionalProcesses);
            return this;
        }

        
        
        
        
        
        [Obsolete("Dont use directly, will be removed")]
        public static WorkerDeterminer GetWorkerDeterminer(string connectionString) => 
            new CompositionRoot().BuildWorkerDeterminer(connectionString);
    }
}