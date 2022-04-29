using System;
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
					var schemaName = x.SchemaName;
					var connstring = hidePassword(x);
					if (x.ConnectionString != null)
						schemaName ??= x.ConnectionString.GetProvider().DefaultSchemaName();

					return new ViewModel
					{
						Id = x.Id,
						Name = x.Name,
						ConnectionString = connstring,
						SchemaName = schemaName,
						Active = x.Active,
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
					var parsed = NpgsqlConnectionStringBuilderWorkaround.Parse(x.ConnectionString);
					if (parsed.Password == null)
						return x.ConnectionString;
					parsed.Password = hiddenPassword;
					return parsed.ToString();
				},
				() =>
				{
					var parsed = StackExchange.Redis.ConfigurationOptions.Parse(x.ConnectionString);
					if (parsed.Password != null)
						parsed.Password = hiddenPassword;
					return parsed.ToString();
				});
		}
	}
}