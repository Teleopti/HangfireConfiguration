namespace Hangfire.Configuration
{
    public class CompositionRoot
    {
        // internal services
        internal State _state = new State();

        private StateMaintainer builderStateMaintainer(object appBuilder) =>
            new StateMaintainer(BuildHangfire(appBuilder), BuildConfigurationStorage(),
                buildConfigurationUpdater(), _state);

        private ConfigurationUpdater buildConfigurationUpdater() =>
            new ConfigurationUpdater(BuildConfigurationStorage(), _state);

        private UnitOfWork buildUnitOfWork() =>
            new UnitOfWork {ConnectionString = _state.ReadOptions().ConnectionString};

        private WorkerDeterminer buildWorkerDeterminer() =>
            new WorkerDeterminer(BuildServerCountSampleStorage());
        
        protected ServerCountSampleRecorder buildServerCountSampleRecorder()
        {
            return new ServerCountSampleRecorder(
                BuildServerCountSampleStorage(), 
                _state,
                builderStateMaintainer(null));
        }
        
        
        // outer services
        public Options BuildOptions() =>
            new Options(_state);

        public WorkerServerStarter BuildWorkerServerStarter(object appBuilder) =>
            new WorkerServerStarter(BuildHangfire(appBuilder), buildWorkerDeterminer(),
                builderStateMaintainer(appBuilder), _state, buildServerCountSampleRecorder());

        public PublisherStarter BuildPublisherStarter() =>
            new PublisherStarter(builderStateMaintainer(null), _state);

        public ConfigurationApi BuildConfigurationApi() =>
            new ConfigurationApi(
                BuildConfigurationStorage(),
                BuildHangfireSchemaCreator(), _state);

        public PublisherQueries BuildPublishersQuerier() =>
            new PublisherQueries(_state, builderStateMaintainer(null));

        public WorkerServerQueries BuildWorkerServersQuerier() =>
            new WorkerServerQueries(builderStateMaintainer(null), _state);

        public ViewModelBuilder BuildViewModelBuilder() =>
            new ViewModelBuilder(BuildConfigurationStorage());


        // boundary
        protected virtual IHangfire BuildHangfire(object appBuilder) =>
            new RealHangfire(appBuilder);

        protected virtual IHangfireSchemaCreator BuildHangfireSchemaCreator() =>
            new HangfireSchemaCreator();

        protected virtual IConfigurationStorage BuildConfigurationStorage() =>
            new ConfigurationStorage(buildUnitOfWork());

        protected virtual IServerCountSampleStorage BuildServerCountSampleStorage() =>
            new ServerCountSampleStorage(buildUnitOfWork());
    }
}