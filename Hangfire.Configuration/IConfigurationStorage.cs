using System;
using System.Collections.Generic;

namespace Hangfire.Configuration;

public interface IConfigurationStorage
{
	IEnumerable<StoredConfiguration> ReadConfigurations();
	void WriteConfiguration(StoredConfiguration configuration);

	void Transaction(Action action);
	void LockConfiguration();
}