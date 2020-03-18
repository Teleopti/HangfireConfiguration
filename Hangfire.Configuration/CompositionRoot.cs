namespace Hangfire.Configuration
{
    public class CompositionRoot
    {
        // outer services
        public WorkerServerStarter BuildWorkerServerStarter(object appBuilder, ConfigurationConnection connection) =>
            new WorkerServerStarter(BuildHangfire(appBuilder), BuildWorkerDeterminer(connection), builderStateMaintainer(appBuilder, connection), _state);

        public PublisherStarter BuildPublisherStarter(ConfigurationConnection connection) =>
            new PublisherStarter(builderStateMaintainer(null, connection), _state);

        public WorkerDeterminer BuildWorkerDeterminer(ConfigurationConnection connection) =>
            new WorkerDeterminer(BuildRepository(connection));

        public ConfigurationApi BuildConfigurationApi(ConfigurationOptions options) =>
            new ConfigurationApi(BuildRepository(new ConfigurationConnection() {ConnectionString = options.ConnectionString}), BuildHangfireSchemaCreator(), options);

        public PublisherQueries BuildPublishersQuerier(ConfigurationConnection connection) =>
            new PublisherQueries(_state, builderStateMaintainer(null, connection));

        public WorkerServerQueries BuildWorkerServersQuerier(ConfigurationConnection connection) =>
            new WorkerServerQueries(builderStateMaintainer(null, connection), _state);

        public ViewModelBuilder BuildViewModelBuilder(ConfigurationConnection connection) =>
            new ViewModelBuilder(BuildRepository(connection));

        // internal services
        private State _state = new State();

        private StateMaintainer builderStateMaintainer(object appBuilder, ConfigurationConnection connection) =>
            new StateMaintainer(BuildHangfire(appBuilder), BuildRepository(connection), buildConfigurationUpdater(connection), _state);

        private ConfigurationUpdater buildConfigurationUpdater(ConfigurationConnection connection) =>
            new ConfigurationUpdater(BuildRepository(connection), BuildDistributedLock(connection), _state);


        // boundary
        protected virtual IHangfire BuildHangfire(object appBuilder) =>
            new RealHangfire(appBuilder);

        protected virtual IHangfireSchemaCreator BuildHangfireSchemaCreator() =>
            new HangfireSchemaCreator();

        protected virtual IConfigurationRepository BuildRepository(ConfigurationConnection connection) =>
            new ConfigurationRepository(connection);

        protected virtual IDistributedLock BuildDistributedLock(ConfigurationConnection connection) =>
            new DistributedLock(connection.ConnectionString);
    }
}