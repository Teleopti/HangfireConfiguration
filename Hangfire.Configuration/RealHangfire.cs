using Hangfire.Configuration.Providers;
using Hangfire.Server;
#if NET472
using Owin;
#else
using Microsoft.AspNetCore.Builder;
#endif

namespace Hangfire.Configuration
{
	public class RealHangfire : IHangfire
	{
		private readonly object _applicationBuilder;

		public RealHangfire(object applicationBuilder)
		{
			_applicationBuilder = applicationBuilder;
		}

		public void UseHangfireServer(
			JobStorage storage,
			BackgroundJobServerOptions options,
			params IBackgroundProcess[] additionalProcesses)
		{
#if !NET472
            ((IApplicationBuilder) _applicationBuilder).UseHangfireServer(options, additionalProcesses, storage);
#else
			((IAppBuilder) _applicationBuilder).UseHangfireServer(storage, options, additionalProcesses);
#endif
		}

		public JobStorage MakeJobStorage(string connectionString, object options)
		{
			return connectionString.GetProvider()
				.NewStorage(connectionString, options);
		}
	}
}