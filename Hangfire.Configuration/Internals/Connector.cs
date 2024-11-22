using System;
using System.Data;

namespace Hangfire.Configuration.Internals;

internal class Connector : ConnectorBase
{
	protected override void operation(Action<IDbConnection, IDbTransaction> action)
	{
		this.PickAction(() =>
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