using System;

namespace Hangfire.Configuration.Internals;

internal static class ServerCountSamplingPolicy
{
	public static readonly TimeSpan Interval = TimeSpan.FromMinutes(10);
	public const int Limit = 6;
	public static readonly TimeSpan MaxAge = TimeSpan.FromHours(24);
	public static readonly TimeSpan RecentWindow = TimeSpan.FromTicks(Interval.Ticks * Limit);
}
