using System.Collections.Generic;
using System.Linq;

namespace Hangfire.Configuration
{
    public class ServerCountSampleRepository : IServerCountSampleRepository
    {
        public IEnumerable<ServerCountSample> Samples() => 
            Enumerable.Empty<ServerCountSample>();
    }
}