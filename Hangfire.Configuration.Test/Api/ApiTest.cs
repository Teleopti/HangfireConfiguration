using NUnit.Framework;

namespace Hangfire.Configuration.Test.Api
{
    public class ApiTest
    {
        [Test]
        public void ShouldNotThrow()
        {
            HangfireConfiguration.UseHangfireConfiguration(null);
        }
    }
}