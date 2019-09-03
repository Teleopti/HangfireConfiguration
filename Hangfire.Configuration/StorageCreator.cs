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
        private readonly DefaultServerConfigurator _defaultServerConfigurator;

        public StorageCreator(IHangfire hangfire, IConfigurationRepository repository, DefaultServerConfigurator defaultServerConfigurator)
        {
            _hangfire = hangfire;
            _repository = repository;
            _defaultServerConfigurator = defaultServerConfigurator;
        }

        public IEnumerable<HangfireStorage> Create(ConfigurationOptions options, SqlServerStorageOptions storageOptions)
        {
            _defaultServerConfigurator.Configure(options);

            return _repository
                .ReadConfigurations()
                .OrderBy(x => !(x.Active ?? false))
                .ThenBy(x => x.Id)
                .Select(configuration => makeJobStorage(configuration, storageOptions))
                .ToArray();
        }

        private HangfireStorage makeJobStorage(StoredConfiguration configuration, SqlServerStorageOptions storageOptions)
        {
            var options = copyOptions(storageOptions ?? new SqlServerStorageOptions());
            options.SchemaName = configuration.SchemaName;
            return new HangfireStorage
            {
                Configuration = configuration,
                JobStorage = _hangfire.MakeSqlJobStorage(configuration.ConnectionString, options)
            };
        }

        private static SqlServerStorageOptions copyOptions(SqlServerStorageOptions storageOptions) =>
            JsonConvert.DeserializeObject<SqlServerStorageOptions>(
                JsonConvert.SerializeObject(storageOptions)
            );
    }
}