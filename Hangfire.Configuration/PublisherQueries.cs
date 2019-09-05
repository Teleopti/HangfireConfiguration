using System.Collections.Generic;
using System.Linq;
using Hangfire.SqlServer;

namespace Hangfire.Configuration
{
    public class PublisherQueries
    {
        private readonly HangfireStorageState _storageState;

        public PublisherQueries(HangfireStorageState storageState)
        {
            _storageState = storageState;
        }
        
        public IEnumerable<JobStorage> QueryPublishers() => 
            _storageState.StorageState
                .Where(x => x.Configuration.Active.Value)
                .Select(x => x.JobStorage)
                .ToArray();
    }
}