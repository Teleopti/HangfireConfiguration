using System;
using System.Collections.Generic;
using System.Threading;
using Hangfire.Common;
using Hangfire.Server;
using Hangfire.Storage;

namespace Hangfire.Configuration.Test.Domain.Fake;

public class FakeStorageConnection : IStorageConnection
{
	public void Dispose()
	{
	}

	public IWriteOnlyTransaction CreateWriteTransaction()
	{
		throw new NotImplementedException();
	}

	public IDisposable AcquireDistributedLock(string resource, TimeSpan timeout)
	{
		throw new NotImplementedException();
	}

	public string CreateExpiredJob(Job job, IDictionary<string, string> parameters, DateTime createdAt, TimeSpan expireIn)
	{
		throw new NotImplementedException();
	}

	public IFetchedJob FetchNextJob(string[] queues, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}

	public void SetJobParameter(string id, string name, string value)
	{
		throw new NotImplementedException();
	}

	public string GetJobParameter(string id, string name)
	{
		throw new NotImplementedException();
	}

	public JobData GetJobData(string jobId)
	{
		throw new NotImplementedException();
	}

	public StateData GetStateData(string jobId)
	{
		throw new NotImplementedException();
	}

	public void AnnounceServer(string serverId, ServerContext context)
	{
		throw new NotImplementedException();
	}

	public void RemoveServer(string serverId)
	{
		throw new NotImplementedException();
	}

	public void Heartbeat(string serverId)
	{
		throw new NotImplementedException();
	}

	public int RemoveTimedOutServers(TimeSpan timeOut)
	{
		throw new NotImplementedException();
	}

	public HashSet<string> GetAllItemsFromSet(string key)
	{
		throw new NotImplementedException();
	}

	public string GetFirstByLowestScoreFromSet(string key, double fromScore, double toScore)
	{
		throw new NotImplementedException();
	}

	public void SetRangeInHash(string key, IEnumerable<KeyValuePair<string, string>> keyValuePairs)
	{
		throw new NotImplementedException();
	}

	public Dictionary<string, string> GetAllEntriesFromHash(string key)
	{
		throw new NotImplementedException();
	}
}