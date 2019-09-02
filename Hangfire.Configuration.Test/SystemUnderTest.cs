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
                            app.UseHangfireConfigurationInterface("/config", new HangfireConfigurationInterfaceOptions());
                        }))
#if !NET472
                        )
#endif
                ;

            Repository = new FakeConfigurationRepository();
            SchemaCreator = new FakeHangfireSchemaCreator();
            Monitor = new FakeMonitoringApi();
            HangfireStorage = new FakeHangfireStorage(Monitor);
            Hangfire = new FakeHangfire(AppBuilder);
            DistributedLock = new FakeDistributedLock();

            Configuration = BuildConfiguration(null);
            ServerStarter = BuildServerStarter(AppBuilder);
            Determiner = BuildWorkerDeterminer(null);
            HangfireStarter = BuildStarter(new ConfigurationOptions());
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
        public FakeHangfireStorage HangfireStorage { get; }
        public FakeHangfire Hangfire { get; }
        public FakeDistributedLock DistributedLock { get; }

        public WorkerDeterminer Determiner { get; }
        public Configuration Configuration { get; }
        public ServerStarter ServerStarter { get; }
        public HangfireStarter HangfireStarter { get; }

        public sealed override IConfigurationRepository BuildRepository(string connectionString) => Repository;
        public sealed override IHangfire BuildHangfire(object appBuilder) => Hangfire;
        public sealed override IHangfireStorage BuildHangfireStorage() => HangfireStorage;
        public sealed override IMonitoringApi BuildMonitoringApi() => Monitor;
        public sealed override IHangfireSchemaCreator BuildHangfireSchemaCreator() => SchemaCreator;
        public sealed override IDistributedLock BuildDistributedLock(string connectionString) => DistributedLock;
    }
}