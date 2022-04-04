using System;
using System.Collections.Generic;

namespace Hangfire.Configuration.Internals;

internal class PublisherQueryCache
{
	private readonly INow _now;
	private DateTime? _timeout;
	private ConfigurationInfo[] _data;
	private readonly object _lock = new();

	public PublisherQueryCache(INow now)
	{
		_now = now;
	}

	public IEnumerable<ConfigurationInfo> Get(Func<ConfigurationInfo[]> valueFactory)
	{
		if (_now.UtcDateTime() >= _timeout)
			Invalidate();

		var data = _data;
		if (data != null)
			return data;

		lock (_lock)
		{
			if (_data != null)
				return _data;

			_data = valueFactory.Invoke();
			_timeout = _now.UtcDateTime().AddMinutes(1);
			return _data;
		}
	}

	public void Invalidate()
	{
		lock (_lock)
			_data = null;
	}
}