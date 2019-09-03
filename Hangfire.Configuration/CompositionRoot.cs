using Hangfire.Storage;

namespace Hangfire.Configuration
{
    public class CompositionRoot
    {
        // outer services
        public WorkerServerStarter BuildWorkerServerStarter(object appBuilder, ConfigurationConnection connection) =>
            new WorkerServerStarter(BuildHangfire(appBuilder), BuildWorkerDeterminer(connection), buildStorageCreator(appBuilder, connection));

        public PublisherStarter BuildPublisherStarter(ConfigurationConnection connection) =>
            new PublisherStarter(buildStorageCreator(null, connection));

        public WorkerDeterminer BuildWorkerDeterminer(ConfigurationConnection connection) =>
            new WorkerDeterminer(BuildConfiguration(connection));

        public Configuration BuildConfiguration(ConfigurationConnection connection) =>
            new Configuration(BuildRepository(connection), BuildHangfireSchemaCreator());

        // internal services
        private StorageCreator buildStorageCreator(object appBuilder, ConfigurationConnection connection) =>
            new StorageCreator(BuildHangfire(appBuilder), BuildRepository(connection), buildDefaultServerConfigurator(connection));

        private DefaultServerConfigurator buildDefaultServerConfigurator(ConfigurationConnection connection) =>
            new DefaultServerConfigurator(BuildRepository(connection), BuildDistributedLock(connection));

        // boundry
        protected virtual IHangfire BuildHangfire(object appBuilder) =>
            new RealHangfire(appBuilder);

        protected virtual IHangfireSchemaCreator BuildHangfireSchemaCreator() =>
            new HangfireSchemaCreator();

        protected virtual IConfigurationRepository BuildRepository(ConfigurationConnection connection) =>
            new ConfigurationRepository(connection);

        protected virtual IDistributedLock BuildDistributedLock(ConfigurationConnection connection) =>
            new DistributedLock("HangfireConfigurationLock", connection.ConnectionString);
    }
}