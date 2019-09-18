using System.Collections.Generic;
using System.Linq;
using Hangfire.SqlServer;

namespace Hangfire.Configuration
{
    public class PublisherQueries
    {
        private readonly StorageState _storageState;

        public PublisherQueries(StorageState storageState)
        {
            _storageState = storageState;
        }
        
        public IEnumerable<JobStorage> QueryPublishers() => 
            _storageState.State
                .Where(x => x.Configuration.Active.Value)
                .Select(x => x.JobStorage)
                .ToArray();
    }
}