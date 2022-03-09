using System;

namespace Hangfire.Configuration.Internals;

internal class ConfigurationAndStorage
{
	public StoredConfiguration Configuration;
	public Func<JobStorage> JobStorageCreator;

	private JobStorage _jobStorage;
	private readonly object _lock = new();

	public JobStorage CreateJobStorage()
	{
		if (_jobStorage != null)
			return _jobStorage;

		lock (_lock)
		{
			if (_jobStorage != null)
				return _jobStorage;
			_jobStorage = JobStorageCreator.Invoke();
			JobStorageCreator = null;
			return _jobStorage;
		}
	}
}