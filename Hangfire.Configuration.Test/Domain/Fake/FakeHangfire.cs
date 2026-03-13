using System;
using System.Collections.Generic;
using System.Linq;
using Hangfire.Server;

namespace Hangfire.Configuration.Test.Domain.Fake;

public class FakeHangfire(FakeMonitoringApi monitoringApi) : IHangfire
{
    public FakeJobStorage LastCreatedStorage => CreatedStorages.LastOrDefault();
    public IEnumerable<FakeJobStorage> CreatedStorages = Enumerable.Empty<FakeJobStorage>();

    public List<(
        FakeJobStorage storage,
        BackgroundJobServerOptions options,
        IBackgroundProcess[] backgroundProcesses
        )> StartedServers = new();

    public List<(
        FakeJobStorage storage,
        IBackgroundProcess[] backgroundProcesses
        )> BackgroundProcessesStarted = new();

    public IBackgroundProcessingServer StartBackgroundJobServer(
        JobStorage storage,
        BackgroundJobServerOptions options,
        IBackgroundProcess[] additionalProcesses)
    {
        StartedServers.Add((storage as FakeJobStorage, options, additionalProcesses));
        return null;
    }

    public IBackgroundProcessingServer StartBackgroundProcesses(
        JobStorage storage, 
        IBackgroundProcess[] processes)
    {
        BackgroundProcessesStarted.Add((storage as FakeJobStorage, processes));
        return null;
    }

    public JobStorage MakeJobStorage(string connectionString, object options)
    {
        var storage = new FakeJobStorage(connectionString, options, monitoringApi);
        CreatedStorages = CreatedStorages.Append(storage).ToArray();
        return storage;
    }
}