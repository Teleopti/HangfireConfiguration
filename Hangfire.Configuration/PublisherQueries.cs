using System.Collections.Generic;
using System.Linq;
using Hangfire.Configuration.Internals;

namespace Hangfire.Configuration
{
    public class PublisherQueries
    {
        private readonly State _state;
        private readonly StateMaintainer _stateMaintainer;

        internal PublisherQueries(State state, StateMaintainer stateMaintainer)
        {
            _state = state;
            _stateMaintainer = stateMaintainer;
        }

        public IEnumerable<ConfigurationInfo> QueryPublishers()
        {
            _stateMaintainer.Refresh();
            return _state.Configurations
                .Where(x => x.Configuration.Active.GetValueOrDefault())
                .Select(x => new ConfigurationInfo(x))
                .ToArray();
        }
    }
}