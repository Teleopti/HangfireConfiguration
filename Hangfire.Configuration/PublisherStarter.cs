using System.Linq;
using Hangfire.SqlServer;

namespace Hangfire.Configuration
{
    public class PublisherStarter
    {
        private readonly StorageCreator _storageCreator;
        private readonly State _state;

        public PublisherStarter(StorageCreator storageCreator, State state)
        {
            _storageCreator = storageCreator;
            _state = state;
        }

        public void Start(ConfigurationOptions options, SqlServerStorageOptions storageOptions)
        {
            _storageCreator.Refresh(options, storageOptions);
            _state.Configurations.Where(x => x.Configuration.Active.GetValueOrDefault())
                .ForEach(x => { x.CreateJobStorage(); });
        }
    }
}