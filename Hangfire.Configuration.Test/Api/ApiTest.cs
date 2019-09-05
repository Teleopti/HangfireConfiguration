using Xunit;

namespace Hangfire.Configuration.Test.Api
{
    public class ApiTest
    {
        [Fact]
        public void ShouldNotThrow()
        {
            HangfireConfiguration.UseHangfireConfiguration(null);
        }
    }
}