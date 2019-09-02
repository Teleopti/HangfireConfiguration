using Hangfire.Storage;

namespace Hangfire.Configuration
{
    public class CompositionRoot
    {
        public ServerStarter BuildServerStarter(object appBuilder) => 
            new ServerStarter(BuildHangfire(appBuilder));

        public virtual IHangfire BuildHangfire(object appBuilder) => new RealHangfire(appBuilder);

        public WorkerDeterminer BuildWorkerDeterminer(string connectionString) => new WorkerDeterminer(BuildConfiguration(connectionString), BuildMonitoringApi());

        public virtual IMonitoringApi BuildMonitoringApi() => JobStorage.Current.GetMonitoringApi();

        public Configuration BuildConfiguration(string connectionString) => new Configuration(BuildRepository(connectionString), BuildHangfireSchemaCreator());

        public virtual IHangfireSchemaCreator BuildHangfireSchemaCreator() => new HangfireSchemaCreator();

        public virtual IConfigurationRepository BuildRepository(string connectionString) => new ConfigurationRepository(connectionString);

        public virtual IHangfireStorage BuildHangfireStorage() => new RealHangfireStorage();

        public virtual IDistributedLock BuildDistributedLock(string connectionString) => new DistributedLock("HangfireConfigurationLock", connectionString);

        public DefaultServerConfigurator BuildDefaultServerConfigurator(string connectionString) => new DefaultServerConfigurator(BuildRepository(connectionString), BuildDistributedLock(connectionString));

        public HangfireStarter BuildStarter(ConfigurationOptions options) => new HangfireStarter(BuildHangfireStorage(), BuildRepository(options.ConnectionString), BuildDefaultServerConfigurator(options.ConnectionString));
    }
}