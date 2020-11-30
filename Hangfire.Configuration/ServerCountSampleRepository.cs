using System.Collections.Generic;
using System.Linq;

namespace Hangfire.Configuration
{
    public class ServerCountSampleRepository : IServerCountSampleRepository
    {
        private readonly UnitOfWork _connection;

        public ServerCountSampleRepository(UnitOfWork connection)
        {
            _connection = connection;
        }

        public IEnumerable<ServerCountSample> Samples() => 
            Enumerable.Empty<ServerCountSample>();

        public void Write(ServerCountSample sample)
        {
            
        }
    }
}