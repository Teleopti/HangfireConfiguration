using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace Hangfire.Configuration
{
    public class ViewModelBuilder
    {
        private readonly IConfigurationRepository _repository;

        public ViewModelBuilder(IConfigurationRepository repository)
        {
            _repository = repository;
        }
        
        public IEnumerable<ViewModel> BuildServerConfigurations()
        {
            var storedConfiguration = _repository.ReadConfigurations();

            return storedConfiguration.Select((x, i) => new ViewModel
            {
                Id = x?.Id,
                ServerName = getServerName(x?.ConnectionString),
                DatabaseName = getDatabaseName(x?.ConnectionString),
                SchemaName = x?.SchemaName,
                Active = x?.Active != null ? (x?.Active == true ? "Active" : "Inactive") : null,
                Workers = x?.GoalWorkerCount,
            });
        }
        
        private string getDatabaseName(string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            return String.IsNullOrEmpty(builder.InitialCatalog) ? null : builder.InitialCatalog;
        }

        private string getServerName(string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            return String.IsNullOrEmpty(builder.DataSource) ? null : builder.DataSource;
        }
    }
}