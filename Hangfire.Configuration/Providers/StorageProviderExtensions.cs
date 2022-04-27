using Hangfire.Configuration.Internals;

namespace Hangfire.Configuration.Providers;

internal static class StorageProviderExtensions
{
	public static IStorageProvider GetProvider(this string connectionString) =>
		connectionString.ToDbVendorSelector()
			.SelectDialect<IStorageProvider>(
				() => new SqlServerStorageProvider(),
				() => new PostgresStorageProvider(),
				() => new RedisStorageProvider()
			);
}