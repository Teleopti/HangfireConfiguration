using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Hangfire.Configuration.Test.Domain.Fake;

public class FakeKeyValueStore : IKeyValueStore
{
	public int ReadConfigurationsQueryCount;
	public int WriteCount;

	private readonly Hashtable _data = new();

	public void Write(string key, string value)
	{
		WriteCount++;
		_data[key] = value;
	}

	public string Read(string key)
	{
		ReadConfigurationsQueryCount++;
		return _data[key] as string;
	}

	public IEnumerable<string> ReadPrefix(string key)
	{
		ReadConfigurationsQueryCount++;
		return _data.Keys
			.Cast<string>()
			.Where(x => x.StartsWith(key))
			.Select(x => _data[x] as string)
			.ToArray();
	}

	public void Delete(string key) => _data.Remove(key);

	public void Transaction(Action action) =>
		action.Invoke();

	public void LockConfiguration()
	{
	}
}