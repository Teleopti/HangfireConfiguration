using Hangfire.SqlServer;

namespace Hangfire.Configuration
{
    public class PublisherStarter
    {
        private readonly StorageCreator _storageCreator;

        public PublisherStarter(StorageCreator storageCreator)
        {
            _storageCreator = storageCreator;
        }

        public void Start(ConfigurationOptions options, SqlServerStorageOptions storageOptions) =>
            _storageCreator.CreateActive(options, storageOptions);
    }
}