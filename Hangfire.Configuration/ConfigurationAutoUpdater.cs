using System;
using System.Data.SqlClient;
using System.Linq;

namespace Hangfire.Configuration
{
    public class ConfigurationAutoUpdater
    {
        private readonly IConfigurationRepository _repository;
        private readonly IDistributedLock _distributedLock;

        public ConfigurationAutoUpdater(IConfigurationRepository repository, IDistributedLock distributedLock)
        {
            _repository = repository;
            _distributedLock = distributedLock;
        }

        public void Update(ConfigurationOptions options)
        {
            if (options?.AutoUpdatedHangfireConnectionString == null)
                return;

            using (_distributedLock.Take(TimeSpan.FromSeconds(10)))
            {
                var configurations = _repository.ReadConfigurations();

                var configuration = configurations.SingleOrDefault(x => x.ConnectionString == null);

                if (configuration != null)
                {
                    configuration.Active = true;
                }
                else if (configurations.IsEmpty())
                {
                    configuration = new StoredConfiguration {Active = true};
                }
                else
                {
                    configuration = configurations.FirstOrDefault(x => isMarked(x.ConnectionString));
                    if (configuration == null)
                        return;
                }

//
//                var configuration = configurations.FirstOrDefault(x => isMarked(x.ConnectionString));
//                if (configuration == null)
//                {
//                    configuration = configurations.SingleOrDefault(x => x.ConnectionString == null);
//                    if (configuration == null)
//                        configuration = new StoredConfiguration {Active = true};
//                }
                
                configuration.ConnectionString = markConnectionString(options.AutoUpdatedHangfireConnectionString);
                configuration.SchemaName = options.AutoUpdatedHangfireSchemaName;

                _repository.WriteConfiguration(configuration);
            }
        }

        private static bool isMarked(string connectionString) =>
            new SqlConnectionStringBuilder(connectionString)
                .ApplicationName
                .EndsWith(".AutoUpdate");

        private static string markConnectionString(string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            if (applicationNameIsNotSet(builder))
                builder.ApplicationName = "Hangfire";
            builder.ApplicationName += ".AutoUpdate";
            return builder.ToString();
        }

        // because builder will return a app name even though the connection string does not have one
        private static bool applicationNameIsNotSet(SqlConnectionStringBuilder builder)
        {
            return string.IsNullOrEmpty(builder.ApplicationName) ||
                   builder.ApplicationName == ".Net SqlClient Data Provider" ||
                   builder.ApplicationName == "Core .Net SqlClient Data Provider";
        }
    }
}