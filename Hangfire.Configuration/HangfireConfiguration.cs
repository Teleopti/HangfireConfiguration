using System;
using System.Collections.Generic;
using System.Linq;
using Hangfire.Server;
using Hangfire.SqlServer;
#if NETSTANDARD2_0
using Microsoft.AspNetCore.Builder;
#else
using Owin;

#endif

namespace Hangfire.Configuration
{
    public class HangfireConfiguration
    {
        private readonly object _builder;
        private readonly ConfigurationOptions _options;
        private readonly CompositionRoot _compositionRoot;

        private HangfireConfiguration(object builder, ConfigurationOptions options, IDictionary<string, object> properties)
        {
            _builder = builder;
            _options = options;
            _compositionRoot = (properties?.ContainsKey("CompositionRoot") ?? false) ? (CompositionRoot) properties["CompositionRoot"] : new CompositionRoot();
        }

        public static HangfireConfiguration Current { get; private set; }

        public static HangfireConfiguration UseHangfireConfiguration(ConfigurationOptions options) => UseHangfireConfiguration(null, options);

#if NETSTANDARD2_0
        public static HangfireConfiguration UseHangfireConfiguration(IApplicationBuilder builder, ConfigurationOptions options) =>
#else
        public static HangfireConfiguration UseHangfireConfiguration(IAppBuilder builder, ConfigurationOptions options) =>
#endif
            UseHangfireConfiguration(builder, options, builder?.Properties);

#if NETSTANDARD2_0
        public static HangfireConfiguration UseHangfireConfiguration(IApplicationBuilder builder, ConfigurationOptions options, IDictionary<string, object> properties) =>
#else
        public static HangfireConfiguration UseHangfireConfiguration(IAppBuilder builder, ConfigurationOptions options, IDictionary<string, object> properties) =>
#endif
            Current = new HangfireConfiguration(builder, options, properties);

        public HangfireConfiguration StartPublishers(SqlServerStorageOptions storageOptions)
        {
            var starter = _compositionRoot.BuildPublisherStarter(new ConfigurationConnection {ConnectionString = _options.ConnectionString});
            starter.Start(_options, storageOptions);
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

        public IEnumerable<WorkerServer> QueryAllWorkerServers(SqlServerStorageOptions storageOptions) => 
            _compositionRoot.BuildWorkerServersQuerier(new ConfigurationConnection {ConnectionString = _options.ConnectionString})
                .QueryAllWorkerServers(_options, storageOptions);

        public IEnumerable<JobStorage> QueryPublishers() =>
            _compositionRoot.BuildPublishersQuerier().QueryPublishers();

        internal Configuration ConfigurationApi() =>
            _compositionRoot.BuildConfiguration(new ConfigurationConnection {ConnectionString = _options.ConnectionString});
        
        [Obsolete("Dont use directly, will be removed")]
        public static WorkerDeterminer GetWorkerDeterminer(string connectionString) =>
            new CompositionRoot().BuildWorkerDeterminer(new ConfigurationConnection {ConnectionString = connectionString});
    }
}