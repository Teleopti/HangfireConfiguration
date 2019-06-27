using Owin;

namespace Hangfire.Configuration
{
	public static class AppBuilderExtension
	{
		public static void UseHangfireConfiguration(this IAppBuilder builder, string pathMatch, HangfireConfigurationOptions options)
		{
			builder.Map(pathMatch, subApp =>
			{
				subApp.Use(typeof(ConfigurationMiddleware), options);
			});
		}
	}

	public class HangfireConfigurationOptions
	{
		public string ConnectionString;
		public bool PrepareSchemaIfNecessary;
	}
}