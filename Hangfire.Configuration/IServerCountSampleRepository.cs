using System.Collections.Generic;

namespace Hangfire.Configuration
{
    public interface IServerCountSampleRepository
    {
        IEnumerable<ServerCountSample> Samples();
    }
}