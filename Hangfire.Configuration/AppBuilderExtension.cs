using Owin;

namespace Hangfire.Configuration
{
	public static class AppBuilderExtension
	{
		public static void UseHangfireConfiguration(this IAppBuilder builder, string pathMatch, string connectionString) => 
			builder.Map(pathMatch, subApp => subApp.Use(typeof(ConfigurationMiddleware), connectionString));
	}
}