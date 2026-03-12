using System;
using System.Collections.Generic;
using System.Linq;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;

namespace Hangfire.Configuration.Test.Domain.Fake;

public class FakeMonitoringApi : IMonitoringApi
{
	private readonly IList<ServerDto> _servers = new List<ServerDto>();

	public void AnnounceServer(string serverId) => _servers.Add(new ServerDto {Name = serverId});
	
	public IList<QueueWithTopEnqueuedJobsDto> Queues() => new List<QueueWithTopEnqueuedJobsDto>();

	public IList<ServerDto> Servers() => _servers;

	public JobDetailsDto JobDetails(string jobId) => new();

	public StatisticsDto GetStatistics() => new();

	public JobList<EnqueuedJobDto> EnqueuedJobs(string queue, int from, int perPage) => 
		new(Enumerable.Empty<KeyValuePair<string, EnqueuedJobDto>>());

	public JobList<FetchedJobDto> FetchedJobs(string queue, int from, int perPage) => 
		new(Enumerable.Empty<KeyValuePair<string, FetchedJobDto>>());

	public JobList<ProcessingJobDto> ProcessingJobs(int from, int count) => 
		new(Enumerable.Empty<KeyValuePair<string, ProcessingJobDto>>());

	public JobList<ScheduledJobDto> ScheduledJobs(int from, int count) => 
		new(Enumerable.Empty<KeyValuePair<string, ScheduledJobDto>>());

	public JobList<SucceededJobDto> SucceededJobs(int from, int count) => 
		new(Enumerable.Empty<KeyValuePair<string, SucceededJobDto>>());

	public JobList<FailedJobDto> FailedJobs(int from, int count) => 
		new(Enumerable.Empty<KeyValuePair<string, FailedJobDto>>());

	public JobList<DeletedJobDto> DeletedJobs(int from, int count) => 
		new(Enumerable.Empty<KeyValuePair<string, DeletedJobDto>>());
	
	public long ScheduledCount() => 0;
	public long EnqueuedCount(string queue) => 0;
	public long FetchedCount(string queue) => 0;
	public long FailedCount() => 0;
	public long ProcessingCount() => 0;
	public long SucceededListCount() => 0;
	public long DeletedListCount() => 0;
	
	public IDictionary<DateTime, long> SucceededByDatesCount() => new Dictionary<DateTime, long>();
	public IDictionary<DateTime, long> FailedByDatesCount() => new Dictionary<DateTime, long>();
	public IDictionary<DateTime, long> HourlySucceededJobs() => new Dictionary<DateTime, long>();
	public IDictionary<DateTime, long> HourlyFailedJobs() => new Dictionary<DateTime, long>();
}