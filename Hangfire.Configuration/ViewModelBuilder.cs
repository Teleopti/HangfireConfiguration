using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Hangfire.Configuration.Internals;
using Hangfire.Configuration.Providers;
using Npgsql;

namespace Hangfire.Configuration
{
	public class ViewModelBuilder
	{
		private readonly IConfigurationStorage _storage;

		public ViewModelBuilder(IConfigurationStorage storage)
		{
			_storage = storage;
		}

		public IEnumerable<ViewModel> BuildServerConfigurations()
		{
			return _storage.ReadConfigurations()
				.Select(x =>
				{
					var schemaName = x.AppliedSchemaName();
					var connectionString = hidePassword(x);

					return new ViewModel
					{
						Id = x.Id,
						Name = x.Name,
						ConnectionString = connectionString,
						SchemaName = schemaName,
						Active = x.IsActive(),
						WorkerBalancerEnabled = x.WorkerBalancerIsEnabled(),
						Workers = x.GoalWorkerCount,
						MaxWorkersPerServer = x.MaxWorkersPerServer
					};
				}).ToArray();
		}

		private static string hidePassword(StoredConfiguration x)
		{
			const string hiddenPassword = "******";
			return x.ConnectionString.ToDbVendorSelector().SelectDialect(
				() => new SqlConnectionStringBuilder(x.ConnectionString).Password == string.Empty ? x.ConnectionString : new SqlConnectionStringBuilder(x.ConnectionString) {Password = hiddenPassword}.ToString(),
				() =>
				{
					var parsed = new NpgsqlConnectionStringBuilder(x.ConnectionString);
					if (parsed.Password == null)
						return x.ConnectionString;
					parsed.Password = hiddenPassword;
					return parsed.ToString();
				},
				() =>
				{
#if Redis
					var parsed = StackExchange.Redis.ConfigurationOptions.Parse(x.ConnectionString);
					if (parsed.Password != null)
						parsed.Password = hiddenPassword;
					return parsed.ToString();
#else
					return x.ConnectionString;
#endif
				});
		}
	}
}