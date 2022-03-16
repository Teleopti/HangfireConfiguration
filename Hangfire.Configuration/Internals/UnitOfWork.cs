using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using Polly;

namespace Hangfire.Configuration.Internals;

internal interface IUnitOfWork : IDbVendorSelector
{
	int Execute(string sql);
	int Execute(string sql, object param);
	IEnumerable<T> Query<T>(string sql);
	IEnumerable<T> Query<T>(string sql, object param);
}

internal abstract class UnitOfWorkBase : IUnitOfWork
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

	protected void OpenWithRetry(IDbConnection connection)
	{
		_connectionRetry.Execute(connection.Open);
	}

	public T SelectDialect<T>(Func<T> sqlServer, Func<T> postgres, Func<T> redis = null)
	{
		return ConnectionString.ToDbVendorSelector().SelectDialect(sqlServer, postgres);
	}
}

internal class UnitOfWork : UnitOfWorkBase
{
	protected override void operation(Action<IDbConnection, IDbTransaction> action)
	{
		this.ExecuteDialect(() =>
			{
				using var connection = ConnectionString.CreateConnection();
				OpenWithRetry(connection);
				action.Invoke(connection, null);
			}, () =>
			{
				using var connection = ConnectionString.CreateConnection();
				connection.Open();
				action.Invoke(connection, null);
			}
		);
	}
}

internal class UnitOfWorkTransaction : UnitOfWorkBase, IDisposable
{
	private readonly IDbConnection _connection;
	private readonly IDbTransaction _transaction;

	public UnitOfWorkTransaction(string connectionString)
	{
		ConnectionString = connectionString;
		_connection = connectionString.CreateConnection();
		OpenWithRetry(_connection);
		_transaction = _connection.BeginTransaction();
	}

	protected override void operation(Action<IDbConnection, IDbTransaction> action)
	{
		action.Invoke(_connection, _transaction);
	}

	public void Commit()
	{
		_transaction.Commit();
	}

	public void Dispose()
	{
		_transaction.Dispose();
		_connection.Dispose();
	}
}