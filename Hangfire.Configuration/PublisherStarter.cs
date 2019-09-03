using System.Collections.Generic;
using System.Linq;
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

        public IEnumerable<JobStorage> Start(ConfigurationOptions options, SqlServerStorageOptions storageOptions) =>
            _storageCreator.Create(options, storageOptions)
                .Select(x => x.JobStorage)
                .ToArray();
    }
}