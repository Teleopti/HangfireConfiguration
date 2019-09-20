using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Hangfire.Configuration
{
    public class State
    {
        public readonly ConcurrentDictionary<int, ConfigurationAndStorage> Configurations = new ConcurrentDictionary<int, ConfigurationAndStorage>();
    }

    public class ConfigurationAndStorage
    {
        public StoredConfiguration Configuration;
        public Func<JobStorage> JobStorageCreator;

        private JobStorage _jobStorage;
        private readonly object _lock = new object();

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
}