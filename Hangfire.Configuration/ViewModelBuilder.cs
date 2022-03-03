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
				.Select((x, i) =>
				{
					var schemaName = x.SchemaName;
					var dialectSelector = new ConnectionStringDialectSelector(x.ConnectionString);
					if (x.ConnectionString != null)
					{
						schemaName ??= dialectSelector
							.SelectDialect(DefaultSchemaName.SqlServer, DefaultSchemaName.Postgres);
					}

					return new ViewModel
					{
						Id = x.Id,
						Name = x.Name,
						ServerName = string.IsNullOrEmpty(x.ConnectionString) ? null : x.ConnectionString.ServerName(),
						DatabaseName = string.IsNullOrEmpty(x.ConnectionString) ? null : x.ConnectionString.DatabaseName(),
						SchemaName = schemaName,
						Active = x.Active,
						Workers = x.GoalWorkerCount,
						MaxWorkersPerServer = x.MaxWorkersPerServer
					};
				}).ToArray();
		}
	}
}