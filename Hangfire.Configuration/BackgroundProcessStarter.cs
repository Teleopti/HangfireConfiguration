using System;
using System.Linq;
using Hangfire.Configuration.Internals;
using Hangfire.Server;

namespace Hangfire.Configuration;

public class BackgroundProcessStarter
{
	private readonly IHangfire _hangfire;
	private readonly StateMaintainer _stateMaintainer;
	private readonly State _state;
	private readonly object _appBuilder;

	internal BackgroundProcessStarter(
		IHangfire hangfire,
		StateMaintainer stateMaintainer,
		State state,
		object appBuilder)
	{
		_hangfire = hangfire;
		_stateMaintainer = stateMaintainer;
		_state = state;
		_appBuilder = appBuilder;
	}

	internal IDisposable Start(IBackgroundProcess[] additionalProcesses)
	{
		_stateMaintainer.Refresh();

		var configuration = _state.Configurations.FirstOrDefault(x => x.IsActive());
		if (configuration == null)
			configuration = _state.Configurations.FirstOrDefault();
		var server = _hangfire.StartBackgroundProcesses(configuration.JobStorage, additionalProcesses);

		var lifetime = new ServerLifetime([server]);
		ServerLifetime.HookApplicationLifetime(_appBuilder, lifetime);
		return lifetime;
	}
}