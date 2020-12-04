using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Hangfire.Configuration.Test.Domain.Fake
{
    public static class FakeKeyValueStoreServerCountSampleExtension
    {
        public static void Has(this FakeKeyValueStore instance, ServerCountSample sample)
        {
	        ServerCountSamples x = instance.Read();
            x.Samples = x.Samples.Append(sample).ToArray();
            Has(instance, x);
        }

        public static void Has(this FakeKeyValueStore instance, ServerCountSamples samples) =>
	        instance.Write(samples); 
        
        public static IEnumerable<ServerCountSample> Samples(this FakeKeyValueStore instance) =>
            instance.Read().Samples;
    }
}