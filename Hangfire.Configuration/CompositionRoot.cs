using Hangfire.Configuration.Internals;

namespace Hangfire.Configuration
{
    public class CompositionRoot
    {
        // internal services
        private readonly State _state = new();

        private StateMaintainer builderStateMaintainer(object appBuilder) =>
            new(BuildHangfire(appBuilder), BuildConfigurationStorage(),
                buildConfigurationUpdater(), _state);

        private ConfigurationUpdater buildConfigurationUpdater() => new(BuildConfigurationStorage(), _state);

        private UnitOfWork buildUnitOfWork() => new() {ConnectionString = _state.ReadOptions().ConnectionString};

        private WorkerDeterminer buildWorkerDeterminer() => new(BuildKeyValueStore());
        
        protected ServerCountSampleRecorder buildServerCountSampleRecorder()
        {
            return new ServerCountSampleRecorder(
                BuildKeyValueStore(), 
                _state,
                builderStateMaintainer(null), 
                BuildNow());
        }
        
        
        // outer services
        public Options BuildOptions() => new(_state);

        public WorkerServerStarter BuildWorkerServerStarter(object appBuilder) =>
            new(BuildHangfire(appBuilder), buildWorkerDeterminer(),
                builderStateMaintainer(appBuilder), _state, buildServerCountSampleRecorder());

        public PublisherStarter BuildPublisherStarter() => new(builderStateMaintainer(null), _state);

        public ConfigurationApi BuildConfigurationApi() =>
            new(BuildConfigurationStorage(),
                BuildHangfireSchemaCreator(), _state);

        public PublisherQueries BuildPublishersQuerier() => new(_state, builderStateMaintainer(null));

        public WorkerServerQueries BuildWorkerServersQuerier() => new(builderStateMaintainer(null), _state);

        public ViewModelBuilder BuildViewModelBuilder() => new(BuildConfigurationStorage());

        // boundary
        protected virtual IHangfire BuildHangfire(object appBuilder) =>
            new RealHangfire(appBuilder);

        protected virtual IHangfireSchemaCreator BuildHangfireSchemaCreator() =>
            new HangfireSchemaCreator();

        protected virtual IConfigurationStorage BuildConfigurationStorage() =>
            new ConfigurationStorage(buildUnitOfWork());

        protected virtual IKeyValueStore BuildKeyValueStore() =>
            new KeyValueStore(buildUnitOfWork());

        protected virtual INow BuildNow() => new Now();
    }
}