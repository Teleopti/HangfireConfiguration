using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Hangfire.Configuration.Internals;

namespace Hangfire.Configuration;

public class KeyValueStore : IKeyValueStore
{
	private string schema => HangfireConfigurationSchemaInstaller.SchemaName;

	private readonly Connector _connector;
	private readonly ThreadLocal<ConnectorTransaction> _currentTransaction = new();

	internal KeyValueStore(Connector connector)
	{
		_connector = connector;
	}

	private ConnectorBase CurrentConnector() => 
		(ConnectorBase) _currentTransaction.Value ?? _connector;

	public void Write(string key, string value)
	{
		var c = CurrentConnector();
		var updateSqlQuery = c.PickDialect(
			$"UPDATE [{schema}].KeyValueStore SET [Value] = @Value WHERE [Key] = @Key", 
			$"UPDATE {schema}.KeyValueStore SET Value = @Value WHERE Key = @Key");
		var updated = c.Execute(
			updateSqlQuery,
			new {Key = key, Value = value});

		if (updated == 0)
		{
			var insertSqlQuery = c.PickDialect(
				$"INSERT INTO [{schema}].KeyValueStore ([Key], [Value]) VALUES (@Key, @Value)", 
				$"INSERT INTO {schema}.KeyValueStore (Key, Value) VALUES (@Key, @Value)");
			c.Execute(
				insertSqlQuery,
				new {Key = key, Value = value});
		}
	}

	public string Read(string key)
	{
		var c = CurrentConnector();
		var sqlQuery = c.PickDialect(
			$"SELECT [Value] FROM [{schema}].KeyValueStore WHERE [Key] = @Key", 
			$"SELECT Value FROM {schema}.KeyValueStore WHERE Key = @Key");
		return c
			.Query<string>(sqlQuery, new { Key = key })
			.SingleOrDefault();
	}

	public IEnumerable<string> ReadPrefix(string key)
	{
		var c = CurrentConnector();
		var sqlQuery = c.PickDialect(
			$"SELECT [Value] FROM [{schema}].KeyValueStore WHERE [Key] LIKE @Key", 
			$"SELECT Value FROM {schema}.KeyValueStore WHERE Key LIKE @Key");
		return c.Query<string>(sqlQuery, new { Key = key + "%" });
	}

	public void Delete(string key)
	{
		var c = CurrentConnector();
		var sqlQuery = c.PickDialect(
			$"DELETE FROM [{schema}].KeyValueStore WHERE [Key] = @Key", 
			$"DELETE FROM {schema}.KeyValueStore WHERE Key = @Key");
		c.Execute(sqlQuery, new { Key = key });
	}

	public void Transaction(Action action)
	{
		_currentTransaction.Value = _connector.Transaction();
		action.Invoke();
		_currentTransaction.Value.Commit();
		_currentTransaction.Value = null;
	}

	public void LockConfiguration()
	{
		var c = CurrentConnector();
		var sql = c.PickDialect(
			$@"SELECT * FROM [{schema}].KeyValueStore WITH (TABLOCKX)",
			$@"LOCK TABLE {schema}.KeyValueStore");
		c.Execute(sql);
	}
}