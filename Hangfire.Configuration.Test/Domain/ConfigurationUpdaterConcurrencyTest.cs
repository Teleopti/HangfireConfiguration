using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Hangfire.Configuration.Test.Domain.Fake;
using NUnit.Framework;

namespace Hangfire.Configuration.Test.Domain;

public class ConfigurationUpdaterConcurrencyTest
{
	[Test]
	public void ShouldNotBuildJobStorageFromStaleSnapshotWhenAnotherWriterAppliesEquivalentFixBeforeFirstPass()
	{
		// Sibling race to the one below, but with the concurrent fix landing earlier:
		//
		// 1. The DB-management step has just cleared the auto-updated row's ConnectionString
		//    to null.
		// 2. Two web pods start at the same time:
		//    - Pod A and Pod B do their OUTER read in refresh()        → both see CS=null.
		//    - Pod A's Update() races ahead, detects changes on its first pass, enters
		//      the transaction, writes CS=valid, commits.
		//    - Pod B's Update() first-pass read happens AFTER Pod A's commit            →
		//      reads CS=valid → performUpdates sees nothing to do → returns false
		//      *without* ever entering the transaction.
		//    - refresh() trusts the return value and does NOT re-read.
		//    - refresh() builds Pod B's ConfigurationState from its OUTER snapshot
		//      (still CS=null) → lazy JobStorage closes over null → time bomb.
		//
		// Root cause: Update re-reads storage inside its first pass even though the
		// caller already passed in a snapshot via `stored`. That second read can see a
		// different world than the caller's, and Update's "no changes" decision is then
		// made against a snapshot the caller will never see.

		var system = new SystemUnderTestWithRaceHook();

		system.WithConfiguration(new StoredConfiguration
		{
			Name = DefaultConfigurationName.Name(),
			ConnectionString = null,
			Active = true
		});

		var validConnectionString = new SqlConnectionStringBuilder { DataSource = "from-external" }.ToString();

		// Simulate the other pod committing the fix between this pod's OUTER read
		// (inside refresh) and this pod's first-pass read (inside Update).
		// "AfterNextReadPrefix" fires immediately after the next ReadPrefix returns,
		// which is the OUTER read in refresh.
		system.KeyValueStore.AfterNextReadPrefix(() =>
		{
			var row = system.ConfigurationStorage.ReadConfigurations().Single();
			row.ConnectionString = validConnectionString;
			system.ConfigurationStorage.WriteConfiguration(row);
		});

		system.BackgroundJobServerStarter.Start(new ConfigurationOptions
		{
			ExternalConfigurations =
			[
				new ExternalConfiguration
				{
					Name = DefaultConfigurationName.Name(),
					ConnectionString = validConnectionString
				}
			]
		});

		var startedStorage = system.Hangfire.StartedServers.Single().storage;
		Assert.That(startedStorage.ConnectionString, Is.EqualTo(validConnectionString),
			"BackgroundJobServerStarter built JobStorage from a stale OUTER snapshot. " +
			"Update's first pass re-read storage instead of using the caller's `stored`, " +
			"saw a fixed row, decided there was nothing to do, and returned false " +
			"without ever entering the transaction.");
	}

	[Test]
	public void ShouldNotBuildJobStorageFromStaleSnapshotWhenAnotherWriterAppliesEquivalentFixDuringUpdate()
	{
		// Reproduces the rolling-deploy race in ConfigurationUpdater.Update:
		//
		// 1. The DB-management step has just cleared the auto-updated row's ConnectionString
		//    to null (HangfireSchemas.clearAutoUpdatedConnectionStringsAttempt1 in twfm).
		// 2. Two web pods start at the same time. Both call refresh():
		//    - Pod A and Pod B do their OUTER read           → both see CS=null.
		//    - Pod A and Pod B's Update() first pass         → both see CS=null,
		//      both mark Changed=true, both want to enter the transaction.
		//    - Pod A wins the lock, transaction reads CS=null, performUpdates writes
		//      CS=valid, commits.
		//    - Pod B gets the lock, transaction reads CS=valid (Pod A already fixed it),
		//      performUpdates sees nothing to do, no writes happen, `updated` stays false.
		//    - Pod B's Update returns false  →  refresh trusts that and does NOT re-read.
		//    - Pod B's refresh builds a ConfigurationState from its OUTER snapshot
		//      (CS=null) → lazy JobStorage closes over null → time bomb that fires the
		//      next time MonitoringApi/JobStorage is touched (here: applyWorkerBalancer).

		var system = new SystemUnderTestWithRaceHook();

		// Pre-existing row with the connection string cleared by DBManager.
		system.WithConfiguration(new StoredConfiguration
		{
			Name = DefaultConfigurationName.Name(),
			ConnectionString = null,
			Active = true
		});

		var validConnectionString = new SqlConnectionStringBuilder { DataSource = "from-external" }.ToString();

		// Simulate the *other* pod committing the fix between this pod's first-pass read
		// and its transactional second-pass read inside Update. We do that by hooking the
		// next ReadPrefix that runs inside a transaction and writing CS=valid right before
		// it returns — so the transactional read sees the already-fixed row and decides
		// there is nothing to do.
		system.KeyValueStore.BeforeNextReadPrefixInTransaction(() =>
		{
			var row = system.ConfigurationStorage.ReadConfigurations().Single();
			row.ConnectionString = validConnectionString;
			system.ConfigurationStorage.WriteConfiguration(row);
		});

		system.BackgroundJobServerStarter.Start(new ConfigurationOptions
		{
			ExternalConfigurations =
			[
				new ExternalConfiguration
				{
					Name = DefaultConfigurationName.Name(),
					ConnectionString = validConnectionString
				}
			]
		});

		// Storage in the DB is correct (the concurrent writer fixed it). The bug is that
		// THIS pod built its in-memory ConfigurationState from a stale snapshot, so the
		// JobStorage it just handed to Hangfire was constructed with a null connection
		// string — exactly what blows up in production as
		// "ArgumentNullException: nameOrConnectionString" inside SqlServerStorage.
		var startedStorage = system.Hangfire.StartedServers.Single().storage;
		Assert.That(startedStorage.ConnectionString, Is.EqualTo(validConnectionString),
			"BackgroundJobServerStarter built JobStorage from a stale snapshot. " +
			"ConfigurationUpdater.Update returned false because its transactional pass " +
			"found nothing to write (another writer had already applied the equivalent fix), " +
			"so refresh() did not re-read and used the now-stale OUTER snapshot.");
	}
}

internal class HookableFakeKeyValueStore : IKeyValueStore
{
	private readonly FakeKeyValueStore _inner = new();
	private bool _inTransaction;
	private bool _runningHook;
	private Action _beforeNextReadPrefixInTransaction;
	private Action _afterNextReadPrefix;

	public void BeforeNextReadPrefixInTransaction(Action hook) =>
		_beforeNextReadPrefixInTransaction = hook;

	public void AfterNextReadPrefix(Action hook) =>
		_afterNextReadPrefix = hook;

	public void Write(string key, string value) => _inner.Write(key, value);
	public string Read(string key) => _inner.Read(key);
	public void Delete(string key) => _inner.Delete(key);
	public void LockConfiguration() => _inner.LockConfiguration();

	public void Transaction(Action action)
	{
		_inTransaction = true;
		try { _inner.Transaction(action); }
		finally { _inTransaction = false; }
	}

	public IEnumerable<string> ReadPrefix(string key)
	{
		if (_inTransaction && !_runningHook && _beforeNextReadPrefixInTransaction != null)
		{
			var hook = _beforeNextReadPrefixInTransaction;
			_beforeNextReadPrefixInTransaction = null;
			_runningHook = true;
			try { hook(); }
			finally { _runningHook = false; }
		}

		var result = _inner.ReadPrefix(key).ToArray();

		if (!_runningHook && _afterNextReadPrefix != null)
		{
			var hook = _afterNextReadPrefix;
			_afterNextReadPrefix = null;
			_runningHook = true;
			try { hook(); }
			finally { _runningHook = false; }
		}

		return result;
	}
}

internal class SystemUnderTestWithRaceHook : SystemUnderTest
{
	public new HookableFakeKeyValueStore KeyValueStore { get; } = new();

	protected override IKeyValueStore BuildKeyValueStore() => KeyValueStore;
}
