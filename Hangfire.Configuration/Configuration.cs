using System.Data.SqlClient;

namespace Hangfire.Configuration
{
	public class Configuration
	{
		private readonly IConfigurationRepository _repository;
		
		public Configuration(IConfigurationRepository repository) => 
			_repository = repository;
		
		public void WriteGoalWorkerCount(int? workers) => 
			_repository.WriteGoalWorkerCount(workers);

		public int? ReadGoalWorkerCount() => 
			_repository.ReadGoalWorkerCount();

		public ConfigurationViewModel GetConfiguration()
		{
			var storedConfiguration = _repository.ReadConfiguration();
			
			return new ConfigurationViewModel()
			{
				Id = storedConfiguration?.Id,
				ServerName = getServerName(storedConfiguration?.ConnectionString),
				DatabaseName = getDatabaseName(storedConfiguration?.ConnectionString),
				SchemaName = storedConfiguration?.SchemaName,
				Active = storedConfiguration?.Active == true ? "Active" : "Inactive"
			};
		}
		
		public void SaveNewStorageConfiguration(NewStorageConfiguration newStorageConfiguration)
		{
			SqlConnectionStringBuilder connectionStringBuilder = new SqlConnectionStringBuilder();
			connectionStringBuilder["Data Source"] = newStorageConfiguration.Server;
			connectionStringBuilder["Initial Catalog"] = newStorageConfiguration.Database;
			connectionStringBuilder["User ID"] = newStorageConfiguration.User;
			connectionStringBuilder["Password"] = newStorageConfiguration.Password;

			var connectionString = connectionStringBuilder.ConnectionString;
			var schemaName = newStorageConfiguration.SchemaName;

			_repository.SaveNewStorageConfiguration(connectionString, schemaName, false);
		}

		private string getDatabaseName(string connectionString)
		{
			var builder = new SqlConnectionStringBuilder(connectionString);
			return builder.InitialCatalog;
		}

		private string getServerName(string connectionString)
		{
			var builder = new SqlConnectionStringBuilder(connectionString);
			return builder.DataSource;
		}
	}

	public class NewStorageConfiguration
	{
		public string Server { get; set; }
		public string Database { get; set; }
		public string User { get; set; }
		public string Password { get; set; }
		public string SchemaName { get; set; }
	}
}