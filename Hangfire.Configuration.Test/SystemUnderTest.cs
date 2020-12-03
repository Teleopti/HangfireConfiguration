#if !NET472
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder.Internal;
#else
using Microsoft.Owin.Testing;
using Microsoft.Owin.Builder;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Hangfire.Configuration.Test.Domain.Fake;

namespace Hangfire.Configuration.Test
{
    public class SystemUnderTest : CompositionRoot
    {
        private readonly Lazy<TestServer> _testServer;

        public SystemUnderTest(string urlPathMatch = null)
        {
#if !NET472
            ApplicationBuilder = new ApplicationBuilder(null);
#else
            ApplicationBuilder = new AppBuilder();
#endif

            ConfigurationStorage = new FakeConfigurationStorage();
            ServerCountSampleStorage = new FakeServerCountSampleStorage();
            SchemaCreator = new FakeHangfireSchemaCreator();
            Monitor = new FakeMonitoringApi();
            Hangfire = new FakeHangfire(ApplicationBuilder, Monitor);
            _now = new FakeNow {Time = "2020-12-01 09:00".Utc()};

            Options = BuildOptions();
            ConfigurationApi = BuildConfigurationApi();
            WorkerServerStarter =
                new WorkerServerStarterUnderTest(BuildWorkerServerStarter(ApplicationBuilder), Options);
            PublisherStarter = new PublisherStarterUnderTest(BuildPublisherStarter(), Options);
            PublisherQueries = new PublisherQueriesUnderTest(BuildPublishersQuerier(), Options);
            WorkerServerQueries = new WorkerServerQueriesUnderTest(BuildWorkerServersQuerier(), Options);
            ViewModelBuilder = BuildViewModelBuilder();
            ServerCountSampleRecorder = buildServerCountSampleRecorder();

            _testServer = testServer(urlPathMatch);
        }

        private Lazy<TestServer> testServer(string urlPathMatch)
        {
            return new Lazy<TestServer>(() =>
#if !NET472
                        new TestServer(new WebHostBuilder().Configure(app =>
#else
                    TestServer.Create(app =>
#endif
                        {
                            var url = urlPathMatch ?? "/config";
                            app.Properties.Add("CompositionRoot", this);
                            app.UseHangfireConfigurationUI(url, Options.ConfigurationOptions());
                        }))
#if !NET472
                )
#endif
                ;
        }

#if !NET472
        public ApplicationBuilder ApplicationBuilder { get; }
        public HttpClient TestClient => _testServer.Value.CreateClient();
#else
        public AppBuilder ApplicationBuilder { get; }
        public HttpClient TestClient => _testServer.Value.HttpClient;
#endif

        public FakeMonitoringApi Monitor { get; }
        public FakeConfigurationStorage ConfigurationStorage { get; }
        public FakeServerCountSampleStorage ServerCountSampleStorage { get; }
        public FakeHangfireSchemaCreator SchemaCreator { get; }
        public FakeHangfire Hangfire { get; }
        private FakeNow _now;

        public Options Options { get; }
        public ConfigurationApi ConfigurationApi { get; }
        public WorkerServerStarterUnderTest WorkerServerStarter { get; }
        public PublisherStarterUnderTest PublisherStarter { get; }
        public PublisherQueriesUnderTest PublisherQueries { get; }
        public WorkerServerQueriesUnderTest WorkerServerQueries { get; }
        public ViewModelBuilder ViewModelBuilder { get; }
        public ServerCountSampleRecorder ServerCountSampleRecorder { get; }

        protected override IConfigurationStorage BuildConfigurationStorage() => ConfigurationStorage;
        protected override IServerCountSampleStorage BuildServerCountSampleStorage() => ServerCountSampleStorage;
        protected override IHangfire BuildHangfire(object appBuilder) => Hangfire;
        protected override IHangfireSchemaCreator BuildHangfireSchemaCreator() => SchemaCreator;
        protected override INow BuildNow() => _now;

        public SystemUnderTest WithConfiguration(StoredConfiguration configurations)
        {
            ConfigurationStorage.Has(configurations);
            return this;
        }

        public SystemUnderTest WithAnnouncedServer(string serverId)
        {
            Monitor.AnnounceServer(serverId, null);
            return this;
        }

        public class FakeNow : INow
        {
            public DateTime Time;
            public DateTime UtcDateTime() => Time;
        }

        public SystemUnderTest Now(string time) => Now(time.Utc());

        public SystemUnderTest Now(DateTime time)
        {
            _now.Time = time;
            return this;
        }

        public SystemUnderTest WithServerCountSample(ServerCountSample sample)
        {
            ServerCountSampleStorage.Has(sample);
            return this;
        }

        public SystemUnderTest WithOptions(ConfigurationOptions options)
        {
	        Options.UseOptions(options);
	        return this;
        }
    }
}