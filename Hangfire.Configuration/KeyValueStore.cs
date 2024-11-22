using System.Linq;
using Hangfire.Configuration.Internals;

namespace Hangfire.Configuration
{
    public class KeyValueStore : IKeyValueStore
    {
        private readonly Connector _connector;

        private string schema => HangfireConfigurationSchemaInstaller.SchemaName;

        internal KeyValueStore(Connector connector)
        {
            _connector = connector;
        }

        public void Write(string key, string value)
        {
	        var updateSqlQuery = _connector.PickDialect(
		        $"UPDATE [{schema}].KeyValueStore SET [Value] = @Value WHERE [Key] = @Key", 
		        $"UPDATE {schema}.KeyValueStore SET Value = @Value WHERE Key = @Key");
	        var insertSqlQuery = _connector.PickDialect(
		        $"INSERT INTO [{schema}].KeyValueStore ([Key], [Value]) VALUES (@Key, @Value)", 
		        $"INSERT INTO {schema}.KeyValueStore (Key, Value) VALUES (@Key, @Value)");
	        
			var updated = _connector.Execute(
		        updateSqlQuery,
		        new {Key = key, Value = value});

	        if (updated == 0)
		        _connector.Execute(
			        insertSqlQuery,
			        new {Key = key, Value = value});
        }

        public string Read(string key)
        {
	        var sqlQuery = _connector.PickDialect(
		        $"SELECT [Value] FROM [{schema}].KeyValueStore WHERE [Key] = @Key", 
		        $"SELECT Value FROM {schema}.KeyValueStore WHERE Key = @Key");
			return _connector
		        .Query<string>(sqlQuery, new { Key = key })
		        .SingleOrDefault();
        }
    }
}