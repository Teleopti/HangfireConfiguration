using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
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
	                if (x.ConnectionString != null)
		                schemaName = schemaName ?? DefaultSchemaName.Name(x.ConnectionString);
	                
	                if (string.IsNullOrEmpty(x.ConnectionString))
	                {
		                return GetViewModel(x, null, null, schemaName);
	                }
					

	                return new ConnectionStringDialectSelector(x.ConnectionString)
		                .SelectDialect(
			                () =>
			                {
				                var builder = new SqlConnectionStringBuilder(x.ConnectionString);
				                return GetViewModel(x, builder.DataSource, builder.InitialCatalog, schemaName);
			                },
			                () =>
			                {
				                var builder = new NpgsqlConnectionStringBuilder(x.ConnectionString);
				                return GetViewModel(x, builder.Host, builder.Database, schemaName);
							});

					
                }).ToArray();
        }

        private static ViewModel GetViewModel(StoredConfiguration configuration, string serverName, string databaseName, string schemaName)
        {
	        return new ViewModel
	        {
		        Id = configuration.Id,
		        Name = configuration.Name,
		        ServerName = string.IsNullOrEmpty(serverName) ? null : serverName,
		        DatabaseName = string.IsNullOrEmpty(databaseName) ? null : databaseName,
		        SchemaName = schemaName,
		        Active = configuration.Active,
		        Workers = configuration.GoalWorkerCount,
		        MaxWorkersPerServer = configuration.MaxWorkersPerServer
	        };
        }
    }
}