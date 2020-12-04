using System.Collections.Generic;
using System.Linq;

namespace Hangfire.Configuration.Test.Domain.Fake
{
    public static class FakeKeyValueStoreExtensions
    {
        public static void Has(this FakeKeyValueStore instance, ServerCountSample sample)
        {
	        var x = instance.ServerCountSamples();
            x.Samples = x.Samples.Append(sample).ToArray();
            Has(instance, x);
        }

        public static void Has(this FakeKeyValueStore instance, ServerCountSamples samples) =>
	        instance.ServerCountSamples(samples); 
        
        public static IEnumerable<ServerCountSample> Samples(this FakeKeyValueStore instance) =>
            instance.ServerCountSamples().Samples;
    }
}