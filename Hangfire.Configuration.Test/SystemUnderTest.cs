using System;
using System.Net.Http;
using Hangfire.Configuration.Test.Domain.Fake;
using Hangfire.Storage;
#if !NET472
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Hosting;
#else
using Microsoft.Owin.Builder;
using Microsoft.Owin.Testing;
#endif

namespace Hangfire.Configuration.Test
{
    public class SystemUnderTest : CompositionRoot
    {
        private readonly Lazy<TestServer> _testServer;

        public SystemUnderTest()
        {
            AppBuilder = new object();
            _testServer = new Lazy<TestServer>(() =>
#if !NET472
                        new TestServer(new WebHostBuilder().Configure(app =>
#else
                        TestServer.Create(app =>
#endif
                        {
                            app.Properties.Add("CompositionRoot", this);
                            app.UseHangfireConfigurationUI("/config", new HangfireConfigurationUIOptions());
                        }))
#if !NET472
                        )
#endif
                ;

            var connection = new ConfigurationConnection();

            Repository = new FakeConfigurationRepository();
            SchemaCreator = new FakeHangfireSchemaCreator();
            Monitor = new FakeMonitoringApi();
            Hangfire = new FakeHangfire(AppBuilder, Monitor);
            DistributedLock = new FakeDistributedLock();

            Configuration = BuildConfiguration(null);
            WorkerServerStarter = BuildWorkerServerStarter(AppBuilder, connection);
            Determiner = BuildWorkerDeterminer(null);
            PublisherStarter = BuildPublisherStarter(connection);
        }

        public object AppBuilder { get; }
#if !NET472
        public HttpClient TestClient => _testServer.Value.CreateClient();
#else
        public HttpClient TestClient => _testServer.Value.HttpClient;
#endif

	    public FakeMonitoringApi Monitor { get; }
        public FakeConfigurationRepository Repository { get; }
        public FakeHangfireSchemaCreator SchemaCreator { get; }
        public FakeHangfire Hangfire { get; }
        public FakeDistributedLock DistributedLock { get; }

        public WorkerDeterminer Determiner { get; }
        public Configuration Configuration { get; }
        public WorkerServerStarter WorkerServerStarter { get; }
        public PublisherStarter PublisherStarter { get; }

        protected override IConfigurationRepository BuildRepository(ConfigurationConnection connection) => Repository;
        protected override IHangfire BuildHangfire(object appBuilder) => Hangfire;
        protected override IHangfireSchemaCreator BuildHangfireSchemaCreator() => SchemaCreator;
        protected override IDistributedLock BuildDistributedLock(ConfigurationConnection connection) => DistributedLock;
    }
}