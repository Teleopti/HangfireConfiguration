using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Hangfire.Configuration
{
    internal class State
    {
        public IEnumerable<ConfigurationAndStorage> Configurations = Enumerable.Empty<ConfigurationAndStorage>();
        public bool ConfigurationAutoUpdaterRan { get; set; }
    }

    internal class ConfigurationAndStorage
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