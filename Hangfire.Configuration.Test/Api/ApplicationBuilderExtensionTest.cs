using NUnit.Framework;

namespace Hangfire.Configuration.Test.Api
{
    public class ApplicationBuilderExtensionTest
    {
        [Test]
        public void ShouldNotThrow()
        {
            var system = new SystemUnderTest();
            system.ApplicationBuilder.UseHangfireConfiguration(null);
        }
    }
}