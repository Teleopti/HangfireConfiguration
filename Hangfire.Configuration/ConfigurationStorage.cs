using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Hangfire.Configuration
{
	public class ConfigurationStorage
	{
		private readonly IKeyValueStore _store;

		internal ConfigurationStorage(IKeyValueStore store)
		{
			_store = store;
		}
		
		public void Transaction(Action action)
		{
			_store.Transaction(action);
		}

		public void LockConfiguration()
		{
			_store.LockConfiguration();
		}

		public IEnumerable<StoredConfiguration> ReadConfigurations()
		{
			return _store.ReadPrefix("Configuration:")
				.Select(JsonConvert.DeserializeObject<StoredConfiguration>)
				.OrderBy(x => x.Id)
				.ToArray();
		}

		public void WriteConfiguration(StoredConfiguration configuration)
		{
			if (configuration.Id != null)
				_store.Write($"Configuration:{configuration.Id}", JsonConvert.SerializeObject(configuration));
			else
			{
				var id = ReadConfigurations()
					.Select(x => x.Id)
					.Max() ?? 0;
				configuration.Id = id + 1;
				_store.Write($"Configuration:{configuration.Id}", JsonConvert.SerializeObject(configuration));
			}
		}
		
		public void DeleteConfiguration(StoredConfiguration configuration)
		{
			_store.Delete($"Configuration:{configuration.Id}");
		}
	}
}