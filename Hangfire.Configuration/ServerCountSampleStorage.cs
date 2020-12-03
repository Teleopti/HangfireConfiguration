namespace Hangfire.Configuration
{
    public class ServerCountSampleStorage : IServerCountSampleStorage
    {
        private readonly UnitOfWork _connection;

        internal ServerCountSampleStorage(UnitOfWork connection)
        {
            _connection = connection;
        }

        public ServerCountSamples Read() => new ServerCountSamples();

        public void Write(ServerCountSamples samples)
        {
        }
    }
}