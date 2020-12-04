using System.Collections.Generic;
using System.Linq;

namespace Hangfire.Configuration.Test.Domain.Fake
{
	public class FakeKeyValueStore : IKeyValueStore
    {
        private IEnumerable<(string Key,string Value)> _data = Enumerable.Empty<(string, string)>(); 

        public void Write(string key, string value) => 
	        _data = _data.Where(x => x.Key != key).Append((key, value));

        public string Read(string key) => 
	        _data.Where(x => x.Key == key).Select(x => x.Value).SingleOrDefault();
    }
}