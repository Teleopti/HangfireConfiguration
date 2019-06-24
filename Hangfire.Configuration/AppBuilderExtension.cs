using Owin;

namespace Hangfire.Configuration
{
	public static class AppBuilderExtension
	{
		public static void UseHangfireConfiguration(this IAppBuilder builder, string connectionString) => 
			builder.Map("/config", subApp => subApp.Use(typeof(ConfigurationMiddleware), connectionString));
	}
}