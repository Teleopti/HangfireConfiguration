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
            new WorkerDeterminer(BuildServerCountSampleRepository(connection));

        public ConfigurationApi BuildConfigurationApi(ConfigurationOptions options) =>
            new ConfigurationApi(BuildConfigurationRepository(new UnitOfWork() {ConnectionString = options.ConnectionString}), BuildHangfireSchemaCreator(), options);

        public PublisherQueries BuildPublishersQuerier(UnitOfWork connection) =>
            new PublisherQueries(_state, builderStateMaintainer(null, connection));

        public WorkerServerQueries BuildWorkerServersQuerier(UnitOfWork connection) =>
            new WorkerServerQueries(builderStateMaintainer(null, connection), _state);

        public ViewModelBuilder BuildViewModelBuilder(UnitOfWork connection) =>
            new ViewModelBuilder(BuildConfigurationRepository(connection));

        // internal services
        private State _state = new State();

        private StateMaintainer builderStateMaintainer(object appBuilder, UnitOfWork connection) =>
            new StateMaintainer(BuildHangfire(appBuilder), BuildConfigurationRepository(connection), buildConfigurationUpdater(connection), _state);

        private ConfigurationUpdater buildConfigurationUpdater(UnitOfWork connection) =>
            new ConfigurationUpdater(BuildConfigurationRepository(connection), _state);


        // boundary
        protected virtual IHangfire BuildHangfire(object appBuilder) =>
            new RealHangfire(appBuilder);

        protected virtual IHangfireSchemaCreator BuildHangfireSchemaCreator() =>
            new HangfireSchemaCreator();

        protected virtual IConfigurationRepository BuildConfigurationRepository(UnitOfWork connection) =>
            new ConfigurationRepository(connection);
        
        protected virtual IServerCountSampleRepository BuildServerCountSampleRepository(UnitOfWork connection) =>
            new ServerCountSampleRepository();
    }
}