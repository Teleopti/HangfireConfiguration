using System;

namespace Hangfire.Configuration;

public class Now : INow
{
	public DateTime UtcDateTime() => DateTime.UtcNow;
}