using System;
using System.Data;

namespace Hangfire.Configuration.Internals;

internal class ConnectorTransaction : ConnectorBase, IDisposable
{
	private readonly IDbConnection _connection;
	private readonly IDbTransaction _transaction;

	public ConnectorTransaction(string connectionString)
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