using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using Polly;

namespace Hangfire.Configuration.Internals;

internal abstract class ConnectorBase : IDbVendorSelector
{
	protected abstract void operation(Action<IDbConnection, IDbTransaction> action);

	private static readonly Policy _connectionRetry = Policy.Handle<TimeoutException>()
		.Or<SqlException>(DetectTransientSqlException.IsTransient)
		.OrInner<SqlException>(DetectTransientSqlException.IsTransient)
		.WaitAndRetry(6, i => TimeSpan.FromSeconds(Math.Min(30, Math.Pow(i, 2))));

	public string ConnectionString { get; set; }

	public int Execute(string sql)
	{
		const int result = default;
		operation((c, t) => { c.Execute(sql, null, t); });
		return result;
	}

	public int Execute(string sql, object param)
	{
		var result = default(int);
		operation((c, t) => { result = c.Execute(sql, param, t); });
		return result;
	}

	public IEnumerable<T> Query<T>(string sql)
	{
		var result = default(IEnumerable<T>);
		operation((c, t) => { result = c.Query<T>(sql, null, t); });
		return result;
	}

	public IEnumerable<T> Query<T>(string sql, object param)
	{
		var result = default(IEnumerable<T>);
		operation((c, t) => { result = c.Query<T>(sql, param, t); });
		return result;
	}

	protected static void OpenWithRetry(IDbConnection connection)
	{
		_connectionRetry.Execute(connection.Open);
	}

	public T SelectDialect<T>(Func<T> sqlServer, Func<T> postgres, Func<T> redis = null)
	{
		return ConnectionString.ToDbVendorSelector().SelectDialect(sqlServer, postgres);
	}
}