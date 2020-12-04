using System.Collections;

namespace Hangfire.Configuration.Test.Domain.Fake
{
	public class FakeKeyValueStore : IKeyValueStore
    {
        private readonly Hashtable _data = new Hashtable();
        public void Write(string key, string value) => _data[key] = value;
        public string Read(string key) => _data[key] as string;
    }
}