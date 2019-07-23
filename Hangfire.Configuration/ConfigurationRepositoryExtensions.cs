namespace Hangfire.Configuration
{
    public static class ConfigurationRepositoryExtensions
    {
        public static void WriteNewStorageConfiguration(this IConfigurationRepository repository, string connectionString, string schemaName, bool active)
        {
            repository.WriteConfiguration(new StoredConfiguration()
            {
                ConnectionString = connectionString,
                SchemaName = schemaName,
                Active = active
            });
        }
    }
}