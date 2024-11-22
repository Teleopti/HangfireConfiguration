using System;

namespace Hangfire.Configuration.Internals;

internal interface IDbSelector
{
	T PickFunc<T>(Func<T> sqlServer, Func<T> postgres, Func<T> redis);
}