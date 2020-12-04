using System;
using Newtonsoft.Json;

namespace Hangfire.Configuration
{
	public static class KeyValueStoreExtensions
	{
		public static ServerCountSamples ServerCountSamples(this IKeyValueStore instance) =>
			instance.Read("ServerCountSamples", () => new ServerCountSamples());

		public static void ServerCountSamples(this IKeyValueStore instance, ServerCountSamples samples) =>
			instance.Write("ServerCountSamples", samples);


		public static T Read<T>(this IKeyValueStore instance, string key, Func<T> @default)
		{
			var value = instance.Read(key);
			return value == null ? 
				@default.Invoke() : 
				JsonConvert.DeserializeObject<T>(value);
		}

		public static void Write<T>(this IKeyValueStore instance, string key, T value) =>
			instance.Write(key, JsonConvert.SerializeObject(value));
	}
}