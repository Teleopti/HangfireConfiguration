using System.Collections.Generic;
using System.Linq;

namespace Hangfire.Configuration
{
    public interface IServerCountSampleStorage
    {
        ServerCountSamples Read();
        void Write(ServerCountSamples samples);
    }

    public class ServerCountSamples
    {
        public IEnumerable<ServerCountSample> Samples { get; set; } = 
            Enumerable.Empty<ServerCountSample>();
    }
}