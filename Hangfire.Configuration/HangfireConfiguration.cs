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
        private readonly CompositionRoot _compositionRoot;

        private HangfireConfiguration(object builder, ConfigurationOptions options, IDictionary<string, object> properties)
        {
            _builder = builder;
            _compositionRoot = (properties?.ContainsKey("CompositionRoot") ?? false) ? (CompositionRoot) properties["CompositionRoot"] : new CompositionRoot();
            _compositionRoot
                .BuildOptionator()
                .UseOptions(options);
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
            _compositionRoot.BuildOptionator().UseStorageOptions(storageOptions);
            return this;
        }

        public HangfireConfiguration UseServerOptions(BackgroundJobServerOptions serverOptions)
        {
            _compositionRoot.BuildOptionator().UseServerOptions(serverOptions);
            return this;
        }

        public HangfireConfiguration StartPublishers()
        {
            _compositionRoot
                .BuildPublisherStarter(new UnitOfWork {ConnectionString = _compositionRoot._state.ReadOptions().ConnectionString})
                .Start();
            return this;
        }

        public HangfireConfiguration StartWorkerServers(IEnumerable<IBackgroundProcess> additionalProcesses)
        {
            _compositionRoot.BuildWorkerServerStarter(
                    _builder,
                    new UnitOfWork {ConnectionString = _compositionRoot._state.ReadOptions().ConnectionString}
                )
                .Start(additionalProcesses.ToArray());
            return this;
        }

        public IEnumerable<ConfigurationInfo> QueryAllWorkerServers()
        {
            return _compositionRoot.BuildWorkerServersQuerier(new UnitOfWork {ConnectionString = _compositionRoot._state.ReadOptions().ConnectionString})
                .QueryAllWorkerServers();
        }

        public IEnumerable<ConfigurationInfo> QueryPublishers()
        {
            return _compositionRoot.BuildPublishersQuerier(new UnitOfWork {ConnectionString = _compositionRoot._state.ReadOptions().ConnectionString})
                .QueryPublishers();
        }

        public ConfigurationApi ConfigurationApi() =>
            _compositionRoot.BuildConfigurationApi();

        internal ViewModelBuilder ViewModelBuilder() =>
            _compositionRoot.BuildViewModelBuilder(new UnitOfWork {ConnectionString = _compositionRoot._state.ReadOptions().ConnectionString});
    }
}