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

        private HangfireConfiguration(object builder, ConfigurationOptions options,
            IDictionary<string, object> properties)
        {
            _builder = builder;
            _compositionRoot = (properties?.ContainsKey("CompositionRoot") ?? false)
                ? (CompositionRoot) properties["CompositionRoot"]
                : new CompositionRoot();
            _compositionRoot
                .BuildOptions()
                .UseOptions(options);
        }

        public static HangfireConfiguration Current { get; private set; }

        public static HangfireConfiguration UseHangfireConfiguration(ConfigurationOptions options) =>
            UseHangfireConfiguration(null, options);

#if NETSTANDARD2_0
        public static HangfireConfiguration UseHangfireConfiguration(IApplicationBuilder builder,
            ConfigurationOptions options) =>
#else
        public static HangfireConfiguration UseHangfireConfiguration(IAppBuilder builder, ConfigurationOptions options) =>
#endif
            UseHangfireConfiguration(builder, options, builder?.Properties);

#if NETSTANDARD2_0
        public static HangfireConfiguration UseHangfireConfiguration(IApplicationBuilder builder,
            ConfigurationOptions options, IDictionary<string, object> properties) =>
#else
        public static HangfireConfiguration UseHangfireConfiguration(IAppBuilder builder, ConfigurationOptions options, IDictionary<string, object> properties) =>
#endif
            Current = new HangfireConfiguration(builder, options, properties);

        public HangfireConfiguration UseStorageOptions(SqlServerStorageOptions storageOptions)
        {
            _compositionRoot.BuildOptions().UseStorageOptions(storageOptions);
            return this;
        }

        public HangfireConfiguration UseServerOptions(BackgroundJobServerOptions serverOptions)
        {
            _compositionRoot.BuildOptions().UseServerOptions(serverOptions);
            return this;
        }

        public HangfireConfiguration StartPublishers()
        {
            _compositionRoot
                .BuildPublisherStarter()
                .Start();
            return this;
        }

        public HangfireConfiguration StartWorkerServers(IEnumerable<IBackgroundProcess> additionalProcesses)
        {
            _compositionRoot
                .BuildWorkerServerStarter(_builder)
                .Start(additionalProcesses.ToArray());
            return this;
        }

        public IEnumerable<ConfigurationInfo> QueryAllWorkerServers()
        {
            return _compositionRoot
                .BuildWorkerServersQuerier()
                .QueryAllWorkerServers();
        }

        public IEnumerable<ConfigurationInfo> QueryPublishers()
        {
            return _compositionRoot
                .BuildPublishersQuerier()
                .QueryPublishers();
        }

        public ConfigurationApi ConfigurationApi() =>
            _compositionRoot.BuildConfigurationApi();

        internal ViewModelBuilder ViewModelBuilder() =>
            _compositionRoot.BuildViewModelBuilder();
    }
}