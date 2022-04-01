using System;

namespace Hangfire.Configuration.Test.Domain.Fake;

public class FakeNow : INow
{
	public DateTime Time;
	public DateTime UtcDateTime() => Time;
}