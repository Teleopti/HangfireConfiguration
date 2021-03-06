using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

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
                    var connectionString = new SqlConnectionStringBuilder(x.ConnectionString);
                    var schemaName = x.SchemaName;
                    if (x.ConnectionString != null)
                        schemaName = schemaName ?? DefaultSchemaName.Name();
                    return new ViewModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        ServerName = string.IsNullOrEmpty(connectionString.DataSource) ? null : connectionString.DataSource,
                        DatabaseName = string.IsNullOrEmpty(connectionString.InitialCatalog) ? null : connectionString.InitialCatalog,
                        SchemaName = schemaName,
                        Active = x.Active,
                        Workers = x.GoalWorkerCount,
                        MaxWorkersPerServer = x.MaxWorkersPerServer
                    };
                }).ToArray();
        }
    }
}