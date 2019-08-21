using System;
using Hangfire.Configuration.Test.Domain.Fake;
using Hangfire.Storage;
using Microsoft.Owin.Builder;
using Microsoft.Owin.Testing;

namespace Hangfire.Configuration.Test
{
    public class SystemUnderTest : CompositionRoot
    {
        private readonly Lazy<TestServer> _testServer;

        public SystemUnderTest()
        {
            AppBuilder = new AppBuilder();
            _testServer = new Lazy<TestServer>(() => TestServer.Create(app =>
            {
                app.Properties.Add("CompositionRoot", this);
                app.UseHangfireConfigurationInterface("/config", new HangfireConfigurationInterfaceOptions());
            }));
            
            Repository = new FakeConfigurationRepository();
            Creator = new FakeHangfireSchemaCreator();
            Monitor = new FakeMonitoringApi();
            Hangfire = new FakeHangfire(Monitor);

            Configuration = BuildConfiguration(null);
            ServerStarter = BuildServerStarter(AppBuilder, new ConfigurationOptions());
            Determiner = BuildWorkerDeterminer(null);
        }


        public AppBuilder AppBuilder { get; }
        public TestServer TestServer => _testServer.Value;

        public FakeMonitoringApi Monitor { get; }
        public FakeConfigurationRepository Repository { get; }
        public FakeHangfireSchemaCreator Creator { get; }
        public FakeHangfire Hangfire { get; }

        public WorkerDeterminer Determiner { get; }
        public Configuration Configuration { get; }
        public ServerStarter ServerStarter { get; }


        public sealed override IConfigurationRepository BuildRepository(string connectionString) => Repository;
        public sealed override IHangfire BuildHangfire() => Hangfire;
        public sealed override IMonitoringApi BuildMonitoringApi() => Monitor;
        public sealed override IHangfireSchemaCreator BuildHangfireSchemaCreator() => Creator;
    }
}