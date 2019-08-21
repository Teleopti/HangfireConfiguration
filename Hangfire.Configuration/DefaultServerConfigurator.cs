using System.Linq;

namespace Hangfire.Configuration
{
    public class DefaultServerConfigurator
    {
        private readonly IConfigurationRepository _repository;

        public DefaultServerConfigurator(IConfigurationRepository repository)
        {
            _repository = repository;
        }

        //TODO: unit of work 
        public void Configure(string defaultConnectionString, string defaultSchema)
        {
            if (defaultConnectionString == null)
                return;
            var configurations = _repository.ReadConfigurations().ToArray();
            if (configurations.IsEmpty())
            {
                _repository.WriteConfiguration(new StoredConfiguration
                {
                    ConnectionString = defaultConnectionString,
                    SchemaName = defaultSchema,
                    Active = true
                });
            }
            else
            {
                var legacyConfiguration = configurations.First();
                legacyConfiguration.ConnectionString = defaultConnectionString;
                legacyConfiguration.SchemaName = defaultSchema;
                if (configurations.Where(x => (x.Active ?? false)).IsEmpty())
                    legacyConfiguration.Active = true;

                _repository.WriteConfiguration(legacyConfiguration);
            }
        }
    }
}