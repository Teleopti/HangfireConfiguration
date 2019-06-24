using System;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using SqlSetup = Hangfire.Configuration.SqlServerObjectsInstaller;

namespace Hangfire.Configuration
{
	public interface IConfigurationRepository
	{
		void WriteGoalWorkerCount(int? workers);
		int? ReadGoalWorkerCount();
	}
	
	public class ConfigurationRepository : IConfigurationRepository
	{
		private readonly Func<IDbConnection> _connectionFactory;
		private const string goalWorkerCountKey = "GoalWorkerCount";
		
		public ConfigurationRepository(string connectionString)
		{
			_connectionFactory = () =>
			{
				var conn = new SqlConnection(connectionString);
				conn.Open();
				return conn;
			};
		}
		
		public void WriteGoalWorkerCount(int? workers)
		{
			using(var connection = _connectionFactory()) 
			{
				connection.Execute(
$@"MERGE [{SqlSetup.SchemaName}].[Configuration] AS Target
	USING (VALUES (@key, @value)) AS Source ([Key], [Value]) 
		ON (Target.[Key] = Source.[Key])
			WHEN MATCHED THEN UPDATE SET [Value] = Source.Value
			WHEN NOT MATCHED THEN INSERT ([Key], [Value]) VALUES (Source.[Key], Source.Value);",
					new { key = goalWorkerCountKey, value = workers });
			}
		}

        public int? ReadGoalWorkerCount()
		{
			using(var connection = _connectionFactory()) 
			{
				return connection.QueryFirstOrDefault<int?>(
$@"SELECT TOP 1 [Value] FROM [{SqlSetup.SchemaName}].Configuration WHERE [Key] = '{goalWorkerCountKey}'"					
					);
			};		
		}
	}
}