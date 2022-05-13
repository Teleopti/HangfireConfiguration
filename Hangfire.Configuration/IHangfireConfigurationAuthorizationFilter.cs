#if NETSTANDARD2_0

using Microsoft.AspNetCore.Http;

namespace Hangfire.Configuration
{
	public interface IHangfireConfigurationAuthorizationFilter
	{
		bool Authorize(HttpContext context);
	}
}

#endif