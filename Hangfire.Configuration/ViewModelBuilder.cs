using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Hangfire.Configuration.Internals;
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
					{
						schemaName ??= x.SelectDialect(
							DefaultSchemaName.SqlServer, 
							DefaultSchemaName.Postgres,
							DefaultSchemaName.Redis);
					}

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
			return x.SelectDialect(
				() => new SqlConnectionStringBuilder(x.ConnectionString).Password == string.Empty ? x.ConnectionString : new SqlConnectionStringBuilder(x.ConnectionString) { Password = hiddenPassword }.ToString(),
				() => new NpgsqlConnectionStringBuilder(x.ConnectionString).Password == null ? x.ConnectionString : new NpgsqlConnectionStringBuilder(x.ConnectionString) { Password = hiddenPassword }.ToString(),
				() =>
				{
					var splitByComma = x.ConnectionString.Split(',');
					for (var i = 0; i < splitByComma.Length; i++)
					{
						var splitByEqual = splitByComma[i].Split('=');
						if (splitByEqual.Length > 1)
						{
							if (string.Equals(splitByEqual[0].Trim(), "password", StringComparison.OrdinalIgnoreCase))
							{
								splitByComma[i] = "password = " + hiddenPassword;
							}
						}
					}

					return string.Join(",", splitByComma);
				});
		}
	}
}