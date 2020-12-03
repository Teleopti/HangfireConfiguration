namespace Hangfire.Configuration.Test.Domain.Fake
{
    public class FakeServerCountSampleStorage : IServerCountSampleStorage
    {
        public ServerCountSamples Data = null;

        public void Has(ServerCountSamples samples) => Data = samples;

        public ServerCountSamples Read() => Data ?? new ServerCountSamples();
        public void Write(ServerCountSamples samples) => Data = samples;
    }
}