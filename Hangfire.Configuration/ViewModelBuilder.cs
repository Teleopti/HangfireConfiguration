using System.Collections.Generic;
using System.Linq;
using Hangfire.Configuration.Internals;

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
						ConnectionString = x.ConnectionString,
						SchemaName = schemaName,
						Active = x.Active,
						Workers = x.GoalWorkerCount,
						MaxWorkersPerServer = x.MaxWorkersPerServer
					};
				}).ToArray();
		}
	}
}