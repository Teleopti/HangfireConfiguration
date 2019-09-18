namespace Hangfire.Configuration
{
    public class CompositionRoot
    {
        // outer services
        public WorkerServerStarter BuildWorkerServerStarter(object appBuilder, ConfigurationConnection connection) =>
            new WorkerServerStarter(BuildHangfire(appBuilder), BuildWorkerDeterminer(connection), buildStorageCreator(appBuilder, connection), _storageState);

        public PublisherStarter BuildPublisherStarter(ConfigurationConnection connection) =>
            new PublisherStarter(buildStorageCreator(null, connection));

        public WorkerDeterminer BuildWorkerDeterminer(ConfigurationConnection connection) =>
            new WorkerDeterminer(BuildRepository(connection));

        public ConfigurationApi BuildConfigurationApi(ConfigurationConnection connection) =>
            new ConfigurationApi(BuildRepository(connection), BuildHangfireSchemaCreator());

        public PublisherQueries BuildPublishersQuerier() =>
            new PublisherQueries(_storageState);

        public WorkerServerQueries BuildWorkerServersQuerier(ConfigurationConnection connection) =>
            new WorkerServerQueries(buildStorageCreator(null, connection), _storageState);

        public ViewModelBuilder BuildViewModelBuilder(ConfigurationConnection connection) =>
            new ViewModelBuilder(BuildRepository(connection));

        // internal services
        private StorageState _storageState = new StorageState();

        private StorageCreator buildStorageCreator(object appBuilder, ConfigurationConnection connection) =>
            new StorageCreator(BuildHangfire(appBuilder), BuildRepository(connection), buildDefaultServerConfigurator(connection), _storageState);

        private ConfigurationAutoUpdater buildDefaultServerConfigurator(ConfigurationConnection connection) =>
            new ConfigurationAutoUpdater(BuildRepository(connection), BuildDistributedLock(connection));


        // boundary
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