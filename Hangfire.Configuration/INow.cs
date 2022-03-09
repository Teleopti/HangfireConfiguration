using System;

namespace Hangfire.Configuration;

public interface INow
{
	DateTime UtcDateTime();
}