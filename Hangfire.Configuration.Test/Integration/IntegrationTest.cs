using Xunit;
#if !NET472
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;

#else
using Microsoft.Owin.Testing;
#endif

namespace Hangfire.Configuration.Test.Integration
{
	[Collection("NotParallel")]
	public class IntegrationTest
	{
		[Fact(Skip = "Sus"), CleanDatabase]
		public void ShouldStartServerWithWorkers()
		{
			using (new HangfireServerUnderTest())
			{
			}
		}
	}
}