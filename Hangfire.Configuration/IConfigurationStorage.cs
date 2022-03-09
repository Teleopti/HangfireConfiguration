using System;
using System.Collections.Generic;

namespace Hangfire.Configuration;

public interface IConfigurationStorage
{
	IEnumerable<StoredConfiguration> ReadConfigurations(IUnitOfWork unitOfWork = null);
	void WriteConfiguration(StoredConfiguration configuration, IUnitOfWork unitOfWork = null);

	void UnitOfWork(Action<IUnitOfWork> action);
	void LockConfiguration(IUnitOfWork unitOfWork);
}