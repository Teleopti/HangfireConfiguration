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
        private readonly ConfigurationOptions _options;
        private readonly Lazy<TestServer> _testServer;

        public SystemUnderTest(ConfigurationOptions options = null, string urlPathMatch = null)
        {
            _options = options ?? new ConfigurationOptions();
            urlPathMatch = urlPathMatch ?? "/config";
#if !NET472
            ApplicationBuilder = new ApplicationBuilder(null);
#else
            ApplicationBuilder = new AppBuilder();
#endif
            _testServer = new Lazy<TestServer>(() =>
#if !NET472
                        new TestServer(new WebHostBuilder().Configure(app =>
#else
                        TestServer.Create(app =>
#endif
                        {
                            app.Properties.Add("CompositionRoot", this);
                            app.UseHangfireConfigurationUI(urlPathMatch, _options);
                        }))
#if !NET472
                        )
#endif
                ;

            var connection = new UnitOfWork();

            ConfigurationRepository = new FakeConfigurationRepository();
            ServerCountSampleRepository = new FakeServerCountSampleRepository();
            SchemaCreator = new FakeHangfireSchemaCreator();
            Monitor = new FakeMonitoringApi();
            Hangfire = new FakeHangfire(ApplicationBuilder, Monitor);

            ConfigurationApi = BuildConfigurationApi(_options);
            WorkerServerStarter = BuildWorkerServerStarter(ApplicationBuilder, connection);
            WorkerDeterminer = BuildWorkerDeterminer(null);
            PublisherStarter = BuildPublisherStarter(connection);
            PublisherQueries = BuildPublishersQuerier(connection);
            WorkerServerQueries = BuildWorkerServersQuerier(connection);
            ViewModelBuilder = BuildViewModelBuilder(connection);
            ServerCountSampleRecorder = BuildServerCountSampleRecorder(connection);
        }

#if !NET472
        public ApplicationBuilder ApplicationBuilder { get; }
        public HttpClient TestClient => _testServer.Value.CreateClient();
#else
        public AppBuilder ApplicationBuilder { get; }
        public HttpClient TestClient => _testServer.Value.HttpClient;
#endif

	    public FakeMonitoringApi Monitor { get; }
        public FakeConfigurationRepository ConfigurationRepository { get; }
        public FakeServerCountSampleRepository ServerCountSampleRepository { get; }
        public FakeHangfireSchemaCreator SchemaCreator { get; }
        public FakeHangfire Hangfire { get; }

        public ConfigurationApi ConfigurationApi { get; }
        public WorkerDeterminer WorkerDeterminer { get; }
        public WorkerServerStarter WorkerServerStarter { get; }
        public PublisherStarter PublisherStarter { get; }
        public PublisherQueries PublisherQueries { get;}
        public WorkerServerQueries WorkerServerQueries { get; }
        public ViewModelBuilder ViewModelBuilder { get; }
        public ServerCountSampleRecorder ServerCountSampleRecorder { get; }

        protected override IConfigurationRepository BuildConfigurationRepository(UnitOfWork connection) => ConfigurationRepository;
        protected override IServerCountSampleRepository BuildServerCountSampleRepository(UnitOfWork connection) => ServerCountSampleRepository;
        protected override IHangfire BuildHangfire(object appBuilder) => Hangfire;
        protected override IHangfireSchemaCreator BuildHangfireSchemaCreator() => SchemaCreator;
    }
}