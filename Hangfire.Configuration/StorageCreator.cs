using System.Collections.Generic;
using System.Linq;
using Hangfire.SqlServer;
using Newtonsoft.Json;

namespace Hangfire.Configuration
{
    public class StorageCreator
    {
        private readonly IHangfire _hangfire;
        private readonly IConfigurationRepository _repository;
        private readonly ConfigurationAutoUpdater _configurationAutoUpdater;
        private readonly StorageState _storageState;

        internal StorageCreator(IHangfire hangfire, IConfigurationRepository repository, ConfigurationAutoUpdater configurationAutoUpdater, StorageState storageState)
        {
            _hangfire = hangfire;
            _repository = repository;
            _configurationAutoUpdater = configurationAutoUpdater;
            _storageState = storageState;
        }

        public void Create(ConfigurationOptions options, SqlServerStorageOptions storageOptions)
        {
            _configurationAutoUpdater.Update(options);
            create(storageOptions, _repository.ReadConfigurations());
        }

        public void CreateActive(ConfigurationOptions options, SqlServerStorageOptions storageOptions)
        {
            _configurationAutoUpdater.Update(options);
            create(storageOptions, _repository.ReadConfigurations().Where(x => x.Active.GetValueOrDefault()));
        }

        private void create(
            SqlServerStorageOptions storageOptions, 
            IEnumerable<StoredConfiguration> configurations)
        {
            if (_storageState.State != null)
                return;
            _storageState.State = configurations
                .OrderBy(x => !(x.Active ?? false))
                .ThenBy(x => x.Id)
                .Select(configuration => makeJobStorage(configuration, storageOptions))
                .ToArray();
        }

        private Storage makeJobStorage(StoredConfiguration configuration, SqlServerStorageOptions storageOptions)
        {
            var options = copyOptions(storageOptions ?? new SqlServerStorageOptions());
            if (string.IsNullOrEmpty(configuration.SchemaName))
                options.SchemaName = "HangFire";
            else
                options.SchemaName = configuration.SchemaName;

            var storage = new Storage
            {
                Configuration = configuration,
                JobStorage = _hangfire.MakeSqlJobStorage(configuration.ConnectionString, options)
            };

            return storage;
        }

        private static SqlServerStorageOptions copyOptions(SqlServerStorageOptions storageOptions) =>
            JsonConvert.DeserializeObject<SqlServerStorageOptions>(
                JsonConvert.SerializeObject(storageOptions)
            );
    }
}