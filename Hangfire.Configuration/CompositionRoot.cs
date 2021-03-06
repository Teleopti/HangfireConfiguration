using System;

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
            new WorkerDeterminer(BuildKeyValueStore());
        
        protected ServerCountSampleRecorder buildServerCountSampleRecorder()
        {
            return new ServerCountSampleRecorder(
                BuildKeyValueStore(), 
                _state,
                builderStateMaintainer(null), 
                BuildNow());
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

        protected virtual IKeyValueStore BuildKeyValueStore() =>
            new KeyValueStore(buildUnitOfWork());

        protected virtual INow BuildNow() => new Now();
    }

    public class Now : INow
    {
        public DateTime UtcDateTime() => DateTime.UtcNow;
    }

    public interface INow
    {
        DateTime UtcDateTime();
    }
}