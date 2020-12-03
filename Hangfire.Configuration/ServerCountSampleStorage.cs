using System.Linq;
using Newtonsoft.Json;

namespace Hangfire.Configuration
{
    public class ServerCountSampleStorage : IServerCountSampleStorage
    {
        private readonly UnitOfWork _unitOfWork;

        private string schema => SqlServerObjectsInstaller.SchemaName;

        internal ServerCountSampleStorage(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public ServerCountSamples Read()
        {
            var samples = _unitOfWork
                .Query<string>($@"SELECT [Value] FROM [{schema}].KeyValueStore")
                .FirstOrDefault();

            return samples == null
                ? new ServerCountSamples()
                : JsonConvert.DeserializeObject<ServerCountSamples>(samples);
        }

        public void Write(ServerCountSamples samples)
        {
            var updated = _unitOfWork.Execute(
                $@"UPDATE [{schema}].KeyValueStore SET [Value] = @Value WHERE [Key] = @Key",
                new {Key = "ServerCountSamples", Value = JsonConvert.SerializeObject(samples)});

            if (updated == 0)
                _unitOfWork.Execute(
                    $@"INSERT INTO [{schema}].KeyValueStore ([Key], [Value]) VALUES (@Key, @Value);",
                    new {Key = "ServerCountSamples", Value = JsonConvert.SerializeObject(samples)});
        }
    }
}