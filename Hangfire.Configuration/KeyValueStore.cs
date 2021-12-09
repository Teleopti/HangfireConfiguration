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
	        var updateSqlQuery = "";
	        var insertSqlQuery = "";
	        if (new ConnectionStringDialectSelector(_unitOfWork.ConnectionString).IsPostgreSql())
	        {
				updateSqlQuery = $@"UPDATE {schema}.KeyValueStore SET Value = @Value WHERE Key = @Key";
				insertSqlQuery = $@"INSERT INTO {schema}.KeyValueStore (Key, Value) VALUES (@Key, @Value)";
			}
	        else
	        {
				updateSqlQuery = $@"UPDATE [{schema}].KeyValueStore SET [Value] = @Value WHERE [Key] = @Key";
				insertSqlQuery = $@"INSERT INTO [{schema}].KeyValueStore ([Key], [Value]) VALUES (@Key, @Value)";
			}
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
	        var sqlQuery = "";
			if(new ConnectionStringDialectSelector(_unitOfWork.ConnectionString).IsPostgreSql())
			{
				sqlQuery = $@"SELECT Value FROM {schema}.KeyValueStore WHERE Key = @Key";
			}
			else
			{
				sqlQuery = $@"SELECT [Value] FROM [{schema}].KeyValueStore WHERE [Key] = @Key";
			}
			return _unitOfWork
		        .Query<string>(sqlQuery, new { Key = key })
		        .SingleOrDefault();
        }
    }
}