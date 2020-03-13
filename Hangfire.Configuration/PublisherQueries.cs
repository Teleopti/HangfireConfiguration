using System.Collections.Generic;
using System.Linq;
using Hangfire.SqlServer;

namespace Hangfire.Configuration
{
    public class PublisherQueries
    {
        private readonly State _state;
        private readonly StateMaintainer _creator;

        internal PublisherQueries(State state, StateMaintainer creator)
        {
            _state = state;
            _creator = creator;
        }

        public IEnumerable<ConfigurationInfo> QueryPublishers(ConfigurationOptions options, SqlServerStorageOptions storageOptions)
        {
            _creator.Refresh(options, storageOptions);
            return _state.Configurations
                .Where(x => x.Configuration.Active.Value)
                .Select(x => x.ToConfigurationInfo())
                .ToArray();
        }
    }
}