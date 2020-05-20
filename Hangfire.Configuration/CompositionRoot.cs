namespace Hangfire.Configuration
{
    public class CompositionRoot
    {
        // outer services
        public WorkerServerStarter BuildWorkerServerStarter(object appBuilder, UnitOfWork connection) =>
            new WorkerServerStarter(BuildHangfire(appBuilder), BuildWorkerDeterminer(connection), builderStateMaintainer(appBuilder, connection), _state);

        public PublisherStarter BuildPublisherStarter(UnitOfWork connection) =>
            new PublisherStarter(builderStateMaintainer(null, connection), _state);

        public WorkerDeterminer BuildWorkerDeterminer(UnitOfWork connection) =>
            new WorkerDeterminer(BuildRepository(connection));

        public ConfigurationApi BuildConfigurationApi(ConfigurationOptions options) =>
            new ConfigurationApi(BuildRepository(new UnitOfWork() {ConnectionString = options.ConnectionString}), BuildHangfireSchemaCreator(), options);

        public PublisherQueries BuildPublishersQuerier(UnitOfWork connection) =>
            new PublisherQueries(_state, builderStateMaintainer(null, connection));

        public WorkerServerQueries BuildWorkerServersQuerier(UnitOfWork connection) =>
            new WorkerServerQueries(builderStateMaintainer(null, connection), _state);

        public ViewModelBuilder BuildViewModelBuilder(UnitOfWork connection) =>
            new ViewModelBuilder(BuildRepository(connection));

        // internal services
        private State _state = new State();

        private StateMaintainer builderStateMaintainer(object appBuilder, UnitOfWork connection) =>
            new StateMaintainer(BuildHangfire(appBuilder), BuildRepository(connection), buildConfigurationUpdater(connection), _state);

        private ConfigurationUpdater buildConfigurationUpdater(UnitOfWork connection) =>
            new ConfigurationUpdater(BuildRepository(connection), _state);


        // boundary
        protected virtual IHangfire BuildHangfire(object appBuilder) =>
            new RealHangfire(appBuilder);

        protected virtual IHangfireSchemaCreator BuildHangfireSchemaCreator() =>
            new HangfireSchemaCreator();

        protected virtual IConfigurationRepository BuildRepository(UnitOfWork connection) =>
            new ConfigurationRepository(connection);
    }
}