using Hangfire.SqlServer;
using Newtonsoft.Json;

namespace Hangfire.Configuration
{
    public class StateMaintainer
    {
        private readonly IHangfire _hangfire;
        private readonly IConfigurationRepository _repository;
        private readonly ConfigurationAutoUpdater _configurationAutoUpdater;
        private readonly State _state;

        internal StateMaintainer(IHangfire hangfire, IConfigurationRepository repository, ConfigurationAutoUpdater configurationAutoUpdater, State state)
        {
            _hangfire = hangfire;
            _repository = repository;
            _configurationAutoUpdater = configurationAutoUpdater;
            _state = state;
        }

        public void Refresh(ConfigurationOptions options, SqlServerStorageOptions storageOptions)
        {
            _configurationAutoUpdater.Update(options);

            // maybe not reload all the time
            _repository.ReadConfigurations()
                .ForEach(c =>
                {
                    _state.Configurations.AddOrUpdate(
                        c.Id.Value,
                        (id) => makeJobStorage(c, storageOptions),
                        (id, e) =>
                        {
                            e.Configuration = c;
                            return e;
                        });
                });
        }

        private ConfigurationAndStorage makeJobStorage(StoredConfiguration configuration, SqlServerStorageOptions storageOptions)
        {
            var options = copyOptions(storageOptions ?? new SqlServerStorageOptions());
            if (string.IsNullOrEmpty(configuration.SchemaName))
                options.SchemaName = "HangFire";
            else
                options.SchemaName = configuration.SchemaName;

            return new ConfigurationAndStorage
            {
                JobStorageCreator = () => _hangfire.MakeSqlJobStorage(configuration.ConnectionString, options),
                Configuration = configuration
            };
        }

        private static SqlServerStorageOptions copyOptions(SqlServerStorageOptions storageOptions) =>
            JsonConvert.DeserializeObject<SqlServerStorageOptions>(
                JsonConvert.SerializeObject(storageOptions)
            );
    }
}