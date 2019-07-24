using System.Linq;
using Hangfire.Server;
using Hangfire.SqlServer;
using Microsoft.Owin.Builder;
using Xunit;

namespace Hangfire.Configuration.Test.Domain
{
    public class ServerStarterTest
    {
        [Fact]
        public void ShouldStartServer()
        {
            var repository = new FakeConfigurationRepository();
            repository.Has(new StoredConfiguration());
            var useHangfireServer = new FakeHangfire();
            var target = new ServerStarter(null, new Configuration(repository), useHangfireServer);

            target.StartServers(null, null);

            Assert.NotEmpty(useHangfireServer.StartedServers);
        }

        [Fact]
        public void ShouldPassServerOptionsToHangfire()
        {
            var repository = new FakeConfigurationRepository();
            repository.Has(new StoredConfiguration());
            var useHangfireServer = new FakeHangfire();
            var target = new ServerStarter(null, new Configuration(repository), useHangfireServer);

            target.StartServers(new BackgroundJobServerOptions {ServerName = "server!"}, null);

            Assert.Equal("server!", useHangfireServer.StartedServers.Single().options.ServerName);
        }

        [Fact]
        public void ShouldPassAppBuilderToHangfire()
        {
            var repository = new FakeConfigurationRepository();
            repository.Has(new StoredConfiguration());
            var useHangfireServer = new FakeHangfire();
            var appBuilder = new AppBuilder();
            var target = new ServerStarter(appBuilder, new Configuration(repository), useHangfireServer);

            target.StartServers(null, null);

            Assert.Same(appBuilder, useHangfireServer.StartedServers.Single().builder);
        }

        [Fact]
        public void ShouldPassBackgroundProcessesToHangfire()
        {
            var repository = new FakeConfigurationRepository();
            repository.Has(new StoredConfiguration());
            var useHangfireServer = new FakeHangfire();
            var target = new ServerStarter(null, new Configuration(repository), useHangfireServer);
            var backgroundProcess = new Worker();

            target.StartServers(null, null, backgroundProcess);

            Assert.Same(backgroundProcess, useHangfireServer.StartedServers.Single().backgroundProcesses.Single());
        }

        [Fact]
        public void ShouldStartTwoServers()
        {
            var repository = new FakeConfigurationRepository();
            repository.Has(new StoredConfiguration());
            repository.Has(new StoredConfiguration());
            var useHangfireServer = new FakeHangfire();
            var target = new ServerStarter(null, new Configuration(repository), useHangfireServer);

            target.StartServers(null, null);

            Assert.Equal(2, useHangfireServer.StartedServers.Count());
        }

        [Fact]
        public void ShouldPassBackgroundProcessesToFirstServer()
        {
            var repository = new FakeConfigurationRepository();
            repository.Has(new StoredConfiguration());
            repository.Has(new StoredConfiguration());
            var useHangfireServer = new FakeHangfire();
            var target = new ServerStarter(null, new Configuration(repository), useHangfireServer);

            target.StartServers(null, null, new Worker());

            Assert.NotEmpty(useHangfireServer.StartedServers.First().backgroundProcesses);
            Assert.Empty(useHangfireServer.StartedServers.Last().backgroundProcesses);
        }

        [Fact]
        public void ShouldConstructHangfireStorage()
        {
            var repository = new FakeConfigurationRepository();
            repository.Has(new StoredConfiguration());
            var useHangfireServer = new FakeHangfire();
            var target = new ServerStarter(null, new Configuration(repository), useHangfireServer);

            target.StartServers(null, null);

            Assert.NotNull(useHangfireServer.StartedServers.Single().storage);
        }

        [Fact]
        public void ShouldConstructSqlHangfireStorage()
        {
            var repository = new FakeConfigurationRepository();
            repository.Has(new StoredConfiguration {ConnectionString = "connectionString"});
            var hangfire = new FakeHangfire();
            var target = new ServerStarter(null, new Configuration(repository), hangfire);

            target.StartServers(null, null);

            Assert.Equal("connectionString", (hangfire.StartedServers.Single().storage as FakeJobStorage).ConnectionString);
        }

        [Fact]
        public void ShouldConstructSqlHangfireStorageWithOptions()
        {
            var repository = new FakeConfigurationRepository();
            repository.Has(new StoredConfiguration());
            var hangfire = new FakeHangfire();
            var target = new ServerStarter(null, new Configuration(repository), hangfire);

            target.StartServers(null, new SqlServerStorageOptions {PrepareSchemaIfNecessary = false});

            Assert.False((hangfire.StartedServers.Single().storage as FakeJobStorage).Options.PrepareSchemaIfNecessary);
        }

        [Fact]
        public void ShouldUseSchemaNameFromConfiguration()
        {
            var repository = new FakeConfigurationRepository();
            repository.Has(new StoredConfiguration {SchemaName = "SchemaName"});
            var hangfire = new FakeHangfire();
            var target = new ServerStarter(null, new Configuration(repository), hangfire);

            target.StartServers(null, new SqlServerStorageOptions {SchemaName = "Ignored"});

            Assert.Equal("SchemaName", (hangfire.StartedServers.Single().storage as FakeJobStorage).Options.SchemaName);
        }

        [Fact]
        public void ShouldUseSchemaNameFromConfiguration2()
        {
            var repository = new FakeConfigurationRepository();
            repository.Has(new StoredConfiguration {SchemaName = "SchemaName"});
            var hangfire = new FakeHangfire();
            var target = new ServerStarter(null, new Configuration(repository), hangfire);

            target.StartServers(null, null);

            Assert.Equal("SchemaName", (hangfire.StartedServers.Single().storage as FakeJobStorage).Options.SchemaName);
        }
        
        [Fact]
        public void ShouldUseSchemaNameFromConfigurationOfTwoServers()
        {
            var repository = new FakeConfigurationRepository();
            repository.Has(new StoredConfiguration {SchemaName = "SchemaName1"});
            repository.Has(new StoredConfiguration {SchemaName = "SchemaName2"});
            var hangfire = new FakeHangfire();
            var target = new ServerStarter(null, new Configuration(repository), hangfire);

            target.StartServers(null, null);

            Assert.Equal("SchemaName1", (hangfire.StartedServers.First().storage as FakeJobStorage).Options.SchemaName);
            Assert.Equal("SchemaName2", (hangfire.StartedServers.Last().storage as FakeJobStorage).Options.SchemaName);
        }
    }
}