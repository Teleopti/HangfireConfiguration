using System.Collections.Generic;
using System.Linq;

namespace Hangfire.Configuration.Test.Domain.Fake
{
    public static class FakeServerCountSampleStorageExtensions
    {
        public static void Has(this FakeServerCountSampleStorage instance, ServerCountSample sample)
        {
            var x = instance.Read();
            x.Samples = x.Samples.Append(sample).ToArray();
            instance.Has(x);
        }

        public static IEnumerable<ServerCountSample> Samples(this FakeServerCountSampleStorage instance) =>
            instance.Read().Samples;
    }
}