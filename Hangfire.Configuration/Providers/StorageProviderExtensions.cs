using Hangfire.Configuration.Internals;

namespace Hangfire.Configuration.Providers;

internal static class StorageProviderExtensions
{
	public static IStorageProvider GetProvider(this string connectionString) =>
		connectionString.ToDbSelector()
			.PickFunc<IStorageProvider>(
				() => new SqlServerStorageProvider(),
				() => new PostgresStorageProvider(),
#if Redis
				() => new RedisStorageProvider()
#else
				() => new SqlServerStorageProvider()
#endif
			);
}