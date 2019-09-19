using System;
using System.Collections.Generic;
using System.Linq;

namespace Hangfire.Configuration
{
    public class State
    {
        public IEnumerable<ConfigurationAndStorage> Configurations = Enumerable.Empty<ConfigurationAndStorage>();
    }

    public class ConfigurationAndStorage
    {
        public StoredConfiguration Configuration;
        public Func<JobStorage> JobStorageCreator;

        private JobStorage _jobStorage;

        public JobStorage CreateJobStorage()
        {
            if (_jobStorage != null)
                return _jobStorage;
            _jobStorage = JobStorageCreator.Invoke();
            JobStorageCreator = null;
            return _jobStorage;
        }
    }
}