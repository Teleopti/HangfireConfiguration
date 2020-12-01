#if !NET472
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder.Internal;
#else
using Microsoft.Owin.Testing;
using Microsoft.Owin.Builder;
#endif
using System;
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

            var connection = new UnitOfWork();

            ConfigurationStorage = new FakeConfigurationStorage();
            ServerCountSampleStorage = new FakeServerCountSampleStorage();
            SchemaCreator = new FakeHangfireSchemaCreator();
            Monitor = new FakeMonitoringApi();
            Hangfire = new FakeHangfire(ApplicationBuilder, Monitor);

            Options = BuildOptionator();
            ConfigurationApi = BuildConfigurationApi();
            WorkerServerStarter =
                new WorkerServerStarterUnderTest(BuildWorkerServerStarter(ApplicationBuilder, connection), Options);
            WorkerDeterminer = BuildWorkerDeterminer(null);
            PublisherStarter = new PublisherStarterUnderTest(BuildPublisherStarter(connection), Options);
            PublisherQueries = new PublisherQueriesUnderTest(BuildPublishersQuerier(connection), Options);
            WorkerServerQueries = new WorkerServerQueriesUnderTest(BuildWorkerServersQuerier(connection), Options);
            ViewModelBuilder = BuildViewModelBuilder(connection);
            ServerCountSampleRecorder = BuildServerCountSampleRecorder(connection);

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

        public Options Options { get; }
        public ConfigurationApi ConfigurationApi { get; }
        public WorkerDeterminer WorkerDeterminer { get; }
        public WorkerServerStarterUnderTest WorkerServerStarter { get; }
        public PublisherStarterUnderTest PublisherStarter { get; }
        public PublisherQueriesUnderTest PublisherQueries { get; }
        public WorkerServerQueriesUnderTest WorkerServerQueries { get; }
        public ViewModelBuilder ViewModelBuilder { get; }
        public ServerCountSampleRecorder ServerCountSampleRecorder { get; }

        protected override IConfigurationStorage BuildConfigurationStorage(UnitOfWork connection) =>
            ConfigurationStorage;

        protected override IServerCountSampleStorage BuildServerCountSampleStorage(UnitOfWork connection) =>
            ServerCountSampleStorage;

        protected override IHangfire BuildHangfire(object appBuilder) => Hangfire;
        protected override IHangfireSchemaCreator BuildHangfireSchemaCreator() => SchemaCreator;
    }
}