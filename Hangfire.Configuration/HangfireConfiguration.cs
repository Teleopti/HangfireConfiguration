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
        private SqlServerStorageOptions _storageOptions;
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

        public HangfireConfiguration UseStorageOptions(SqlServerStorageOptions storageOptions)
        {
            _storageOptions = storageOptions;
            return this;
        }

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

        public IEnumerable<JobStorage> QueryAllWorkerServers() => QueryAllWorkerServers(_storageOptions);
        public IEnumerable<JobStorage> QueryAllWorkerServers(SqlServerStorageOptions storageOptions) => 
            _compositionRoot.BuildWorkerServersQuerier(new ConfigurationConnection {ConnectionString = _options.ConnectionString})
                .QueryAllWorkerServers(_options, storageOptions);

        public IEnumerable<JobStorage> QueryPublishers() => QueryPublishers(_storageOptions);
        public IEnumerable<JobStorage> QueryPublishers(SqlServerStorageOptions storageOptions) =>
            _compositionRoot.BuildPublishersQuerier(new ConfigurationConnection {ConnectionString = _options?.ConnectionString})
                .QueryPublishers(_options, storageOptions);

        public ConfigurationApi ConfigurationApi() =>
            _compositionRoot.BuildConfigurationApi(new ConfigurationConnection {ConnectionString = _options.ConnectionString});

        internal ViewModelBuilder ViewModelBuilder() => 
            _compositionRoot.BuildViewModelBuilder(new ConfigurationConnection {ConnectionString = _options.ConnectionString});

        [Obsolete("Dont use directly, will be removed")]
        public static WorkerDeterminer GetWorkerDeterminer(string connectionString) =>
            new CompositionRoot().BuildWorkerDeterminer(new ConfigurationConnection {ConnectionString = connectionString});

    }
}