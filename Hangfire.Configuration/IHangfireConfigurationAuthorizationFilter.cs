#if NETSTANDARD2_0
using Microsoft.AspNetCore.Http;
#else
using Microsoft.Owin;
#endif

namespace Hangfire.Configuration
{
	public interface IHangfireConfigurationAuthorizationFilter
	{
#if NETSTANDARD2_0
		bool Authorize(HttpContext context);
#else
		bool Authorize(IOwinContext context);
#endif
	}
}