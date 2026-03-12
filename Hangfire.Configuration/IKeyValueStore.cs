using System;
using System.Collections.Generic;

namespace Hangfire.Configuration;

public interface IKeyValueStore
{
	void Write(string key, string value);
	string Read(string key);
	IEnumerable<string> ReadPrefix(string key);
	void Delete(string key);

	void Transaction(Action action);
	void LockConfiguration();
}