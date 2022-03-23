using System.Collections.Generic;

namespace Hangfire.Configuration.Internals;

internal interface IConnector : IDbVendorSelector
{
	int Execute(string sql);
	int Execute(string sql, object param);
	IEnumerable<T> Query<T>(string sql);
	IEnumerable<T> Query<T>(string sql, object param);
}