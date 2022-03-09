using System;
using Newtonsoft.Json;

namespace Hangfire.Configuration
{
	public static class KeyValueStoreExtensions
	{
		public static ServerCountSamples ServerCountSamples(this IKeyValueStore instance) =>
			instance.read("ServerCountSamples", () => new ServerCountSamples());

		public static void ServerCountSamples(this IKeyValueStore instance, ServerCountSamples samples) =>
			instance.write("ServerCountSamples", samples);


		private static T read<T>(this IKeyValueStore instance, string key, Func<T> @default)
		{
			var value = instance.Read(key);
			return value == null ? 
				@default.Invoke() : 
				JsonConvert.DeserializeObject<T>(value);
		}

		private static void write<T>(this IKeyValueStore instance, string key, T value) =>
			instance.Write(key, JsonConvert.SerializeObject(value));
	}
}