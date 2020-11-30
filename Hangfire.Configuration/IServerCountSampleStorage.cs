using System.Collections.Generic;

namespace Hangfire.Configuration
{
    public interface IServerCountSampleStorage
    {
        IEnumerable<ServerCountSample> Samples();
        void Write(ServerCountSample sample);
    }
}