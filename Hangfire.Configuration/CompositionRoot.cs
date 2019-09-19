namespace Hangfire.Configuration
{
    public class CompositionRoot
    {
        // outer services
        public WorkerServerStarter BuildWorkerServerStarter(object appBuilder, ConfigurationConnection connection) =>
            new WorkerServerStarter(BuildHangfire(appBuilder), BuildWorkerDeterminer(connection), buildStorageCreator(appBuilder, connection), _state);

        public PublisherStarter BuildPublisherStarter(ConfigurationConnection connection) =>
            new PublisherStarter(buildStorageCreator(null, connection), _state);

        public WorkerDeterminer BuildWorkerDeterminer(ConfigurationConnection connection) =>
            new WorkerDeterminer(BuildRepository(connection));

        public ConfigurationApi BuildConfigurationApi(ConfigurationConnection connection) =>
            new ConfigurationApi(BuildRepository(connection), BuildHangfireSchemaCreator());

        public PublisherQueries BuildPublishersQuerier(ConfigurationConnection connection) =>
            new PublisherQueries(_state, buildStorageCreator(null, connection));

        public WorkerServerQueries BuildWorkerServersQuerier(ConfigurationConnection connection) =>
            new WorkerServerQueries(buildStorageCreator(null, connection), _state);

        public ViewModelBuilder BuildViewModelBuilder(ConfigurationConnection connection) =>
            new ViewModelBuilder(BuildRepository(connection));

        // internal services
        private State _state = new State();

        private StateMaintainer buildStorageCreator(object appBuilder, ConfigurationConnection connection) =>
            new StateMaintainer(BuildHangfire(appBuilder), BuildRepository(connection), buildDefaultServerConfigurator(connection), _state);

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