using System;
using System.Threading;

namespace Hangfire.Configuration;

/// <summary>
/// Keeps the background job servers running, restarting them periodically so the
/// worker balancer recalculates the worker count. Emulates the application pool
/// recycle the balancer relies on when hosted in IIS.
/// </summary>
public class BackgroundJobServerRecycler : IDisposable
{
	private static readonly TimeSpan Interval = TimeSpan.FromHours(23);

	// spreads the recycles of servers started at the same time,
	// they should not all stop processing at the same moment
	private static readonly TimeSpan Jitter = TimeSpan.FromHours(1);

	private static readonly TimeSpan PollInterval = TimeSpan.FromMinutes(1);

	private readonly Func<ServerLifetime> _startServers;
	private readonly INow _now;
	private readonly Random _random = new();
	private readonly object _lock = new();

	private ServerLifetime _servers;
	private DateTime _recycleAt;
	private Timer _timer;
	private bool _stopping;
	private bool _disposed;

	internal BackgroundJobServerRecycler(Func<ServerLifetime> startServers, INow now)
	{
		_startServers = startServers;
		_now = now;
	}

	internal void Start()
	{
		lock (_lock)
		{
			startServers();
			_timer = new Timer(_ => Tick(), null, PollInterval, PollInterval);
		}
	}

	/// <summary>
	/// Recycles the servers if it is time to.
	/// </summary>
	public void Tick()
	{
		// a recycle in progress takes longer than the poll interval,
		// no reason to pile up ticks behind it
		if (!Monitor.TryEnter(_lock))
			return;
		try
		{
			if (_stopping)
				return;
			if (_now.UtcDateTime() < _recycleAt)
				return;
			_servers.SendStop();
			_servers.Dispose();
			startServers();
		}
		finally
		{
			Monitor.Exit(_lock);
		}
	}

	public void SendStop()
	{
		lock (_lock)
		{
			// no recycling once stopping, it would start servers again
			_stopping = true;
			_timer?.Dispose();
			_servers?.SendStop();
		}
	}

	public void Dispose()
	{
		lock (_lock)
		{
			if (_disposed)
				return;
			_disposed = true;
			_stopping = true;
			_timer?.Dispose();
			_servers?.Dispose();
		}
	}

	private void startServers()
	{
		_servers = _startServers();
		_recycleAt = _now.UtcDateTime()
			.Add(Interval)
			.AddTicks((long) (_random.NextDouble() * Jitter.Ticks));
	}
}
