using Xunit;

namespace Hangfire.Configuration.Test.Api
{
    public class ApplicationBuilderExtensionTest
    {
        [Fact]
        public void ShouldNotThrow()
        {
            var system = new SystemUnderTest();
            system.ApplicationBuilder.UseHangfireConfiguration(null);
        }
    }
}