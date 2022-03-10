using System.Linq;

namespace Hangfire.Configuration
{
    public class KeyValueStore : IKeyValueStore
    {
        private readonly UnitOfWork _unitOfWork;

        private string schema => SqlServerObjectsInstaller.SchemaName;

        internal KeyValueStore(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public void Write(string key, string value)
        {
	        var updateSqlQuery = _unitOfWork.SelectDialect(
		        $"UPDATE [{schema}].KeyValueStore SET [Value] = @Value WHERE [Key] = @Key", 
		        $"UPDATE {schema}.KeyValueStore SET Value = @Value WHERE Key = @Key");
	        var insertSqlQuery = _unitOfWork.SelectDialect(
		        $"INSERT INTO [{schema}].KeyValueStore ([Key], [Value]) VALUES (@Key, @Value)", 
		        $"INSERT INTO {schema}.KeyValueStore (Key, Value) VALUES (@Key, @Value)");
	        
			var updated = _unitOfWork.Execute(
		        updateSqlQuery,
		        new {Key = key, Value = value});

	        if (updated == 0)
		        _unitOfWork.Execute(
			        insertSqlQuery,
			        new {Key = key, Value = value});
        }

        public string Read(string key)
        {
	        var sqlQuery = _unitOfWork.SelectDialect(
		        $"SELECT [Value] FROM [{schema}].KeyValueStore WHERE [Key] = @Key", 
		        $"SELECT Value FROM {schema}.KeyValueStore WHERE Key = @Key");
			return _unitOfWork
		        .Query<string>(sqlQuery, new { Key = key })
		        .SingleOrDefault();
        }
    }
}