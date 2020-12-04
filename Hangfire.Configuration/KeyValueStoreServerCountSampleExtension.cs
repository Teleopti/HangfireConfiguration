using Newtonsoft.Json;

namespace Hangfire.Configuration
{
	public static class KeyValueStoreServerCountSampleExtension
	{
		public static ServerCountSamples Read(this IKeyValueStore instance)
		{
			var value = instance.Read("ServerCountSamples");
			return value != null ? JsonConvert.DeserializeObject<ServerCountSamples>(value) : new ServerCountSamples();
		}

		public static void Write(this IKeyValueStore instance, ServerCountSamples samples) =>
			instance.Write("ServerCountSamples", JsonConvert.SerializeObject(samples));
	}
}