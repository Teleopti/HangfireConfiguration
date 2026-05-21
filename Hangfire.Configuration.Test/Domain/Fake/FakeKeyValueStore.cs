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
	private bool _inTransaction;
	private Action _beforeReadInTransaction;
	private Action _afterRead;

	public void BeforeReadInTransaction(Action hook) =>
		_beforeReadInTransaction = hook;

	public void AfterRead(Action hook) =>
		_afterRead = hook;

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

		if (_inTransaction && _beforeReadInTransaction != null)
		{
			var hook = _beforeReadInTransaction;
			_beforeReadInTransaction = null;
			hook();
		}

		var result = _data.Keys
			.Cast<string>()
			.Where(x => x.StartsWith(key))
			.Select(x => _data[x] as string)
			.ToArray();

		if (_afterRead != null)
		{
			var hook = _afterRead;
			_afterRead = null;
			hook();
		}

		return result;
	}

	public void Delete(string key) =>
		_data.Remove(key);

	public virtual void Transaction(Action action)
	{
		_inTransaction = true;
		action.Invoke();
		_inTransaction = false;
	}

	public void LockConfiguration()
	{
	}
}