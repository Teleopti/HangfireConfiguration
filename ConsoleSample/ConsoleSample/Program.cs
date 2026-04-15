using System;
using System.Linq;
using Hangfire;
using Hangfire.Configuration;
using Hangfire.PostgreSql;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Hosting;

namespace ConsoleSample;

public static class Program
{
	public static string NodeAddress = "http://localhost:12345";
	public static DatabaseSelection DatabaseSelection;

	public static void Main()
	{
		DatabaseSelection = SelectDatabaseConfiguration();

		Console.WriteLine("Starting site on " + NodeAddress);

		using (var host = new WebHostBuilder()
			       .UseUrls(NodeAddress)
			       .UseStartup<Startup>()
			       .UseKestrel()
			       .Build())
		{
			host.Start();
			MainLoop(Startup.HangfireConfiguration);
		}

		Console.WriteLine("Press Enter to exit...");
		Console.ReadLine();
	}

	private static DatabaseSelection SelectDatabaseConfiguration()
	{
		Console.WriteLine("Select database type:");
		Console.WriteLine("  1. SQL Server");
		Console.WriteLine("  2. PostgreSQL");
		Console.Write("Enter choice (1/2): ");

		while (true)
		{
			var input = Console.ReadLine()?.Trim();
			switch (input)
			{
				case "1":
					return new DatabaseSelection
					{
						ConfigurationConnectionString = @"Server=.\;Database=Hangfire.Sample;Trusted_Connection=True;",
						DefaultHangfireConnectionString = @"Server=.\;Database=Hangfire.Sample;Trusted_Connection=True;",
						StorageOptions = new SqlServerStorageOptions
						{
							CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
							QueuePollInterval = TimeSpan.Zero,
							SlidingInvisibilityTimeout = TimeSpan.FromMinutes(1),
							UseRecommendedIsolationLevel = true,
							DisableGlobalLocks = true,
							EnableHeavyMigrations = true,
							PrepareSchemaIfNecessary = true,
							SchemaName = "NotUsedSchemaName"
						}
					};
				case "2":
					return new DatabaseSelection
					{
						ConfigurationConnectionString = @"Username=postgres;Password=root;Host=localhost;Database=""hangfire.sample"";",
						DefaultHangfireConnectionString = @"Username=postgres;Password=root;Host=localhost;Database=""hangfire.sample"";",
						StorageOptions = new PostgreSqlStorageOptions
						{
							QueuePollInterval = TimeSpan.FromSeconds(2),
							PrepareSchemaIfNecessary = true,
							SchemaName = "NotUsedSchemaName"
						}
					};
				default:
					Console.Write("Invalid selection. Enter 1 or 2: ");
					break;
			}
		}
	}

	private static void MainLoop(HangfireConfiguration hangfireConfiguration)
	{
		Console.WriteLine("Started.");
		Console.WriteLine("'stop' to exit.");
		Console.WriteLine("'add <count> [queue]' to enqueue jobs. Queue: critical, invoices, default (default: default)");
		while (true)
		{
			var command = Console.ReadLine();

			if (command == null || command.Equals("stop", StringComparison.OrdinalIgnoreCase))
			{
				break;
			}

			if (command.StartsWith("add", StringComparison.OrdinalIgnoreCase))
			{
				try
				{
					var parts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
					var workCount = int.Parse(parts[1]);
					var queue = parts.Length > 2 ? parts[2].ToLowerInvariant() : "default";
					var publisher = hangfireConfiguration.QueryPublishers().First();
					for (var i = 0; i < workCount; i++)
					{
						var number = i;
						publisher.BackgroundJobClient.Enqueue<SampleService>(queue, x => x.DoWork(number));
					}

					Console.WriteLine($"{workCount} job(s) enqueued on '{queue}'");
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
			}
		}
	}
}