using System;

namespace Hangfire.Configuration;

public interface IDbVendorSelector
{
	T SelectDialect<T>(Func<T> sqlServer, Func<T> postgres, Func<T> redis);
}