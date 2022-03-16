using System;

namespace Hangfire.Configuration.Internals;

internal interface IDbVendorSelector
{
	T SelectDialect<T>(Func<T> sqlServer, Func<T> postgres, Func<T> redis);
}