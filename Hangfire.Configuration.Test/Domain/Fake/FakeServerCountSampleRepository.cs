using System.Collections.Generic;
using System.Linq;

namespace Hangfire.Configuration.Test.Domain.Fake
{
    public class FakeServerCountSampleRepository : IServerCountSampleRepository
    {
        public IEnumerable<ServerCountSample> Data = Enumerable.Empty<ServerCountSample>();
        
        public void Has(ServerCountSample sample)
        {
            Data = Data.Append(sample).ToArray();
        }

        public IEnumerable<ServerCountSample> Samples() => 
            Data;

        public void Write(ServerCountSample sample)
        {
            Data = Data.Append(sample).ToArray();
        }
    }
}