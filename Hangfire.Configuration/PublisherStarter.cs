using System.Linq;

namespace Hangfire.Configuration
{
    public class PublisherStarter
    {
        private readonly StateMaintainer _stateMaintainer;
        private readonly State _state;

        internal PublisherStarter(StateMaintainer stateMaintainer, State state)
        {
            _stateMaintainer = stateMaintainer;
            _state = state;
        }

        public void Start()
        {
            _stateMaintainer.Refresh();
            _state.Configurations.Where(x => x.Configuration.Active.GetValueOrDefault())
                .ForEach(x => { x.CreateJobStorage(); });
        }
    }
}