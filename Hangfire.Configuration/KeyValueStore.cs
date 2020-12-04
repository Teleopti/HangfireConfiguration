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
	        var updated = _unitOfWork.Execute(
		        $@"UPDATE [{schema}].KeyValueStore SET [Value] = @Value WHERE [Key] = @Key",
		        new {Key = key, Value = value});

	        if (updated == 0)
		        _unitOfWork.Execute(
			        $@"INSERT INTO [{schema}].KeyValueStore ([Key], [Value]) VALUES (@Key, @Value)",
			        new {Key = key, Value = value});
        }

        public string Read(string key) =>
	        _unitOfWork
		        .Query<string>($@"SELECT [Value] FROM [{schema}].KeyValueStore WHERE [Key] = @Key", new {Key = key})
		        .SingleOrDefault();
    }
}