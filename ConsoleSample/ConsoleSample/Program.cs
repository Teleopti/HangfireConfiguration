using System;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.Owin.Hosting;

namespace ConsoleSample
{
    public static class Program
    {
        public static void Main()
        {
            GlobalConfiguration.Configuration
                .UseColouredConsoleLogProvider()
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(@"Server=.\;Database=Hangfire.Sample;Trusted_Connection=True;",
                    new SqlServerStorageOptions
                    {
                        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                        QueuePollInterval = TimeSpan.Zero,
                        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(1),
                        UseRecommendedIsolationLevel = true,
                        UsePageLocksOnDequeue = true,
                        DisableGlobalLocks = true,
                        EnableHeavyMigrations = true
                    });
            
            using (WebApp.Start<Startup>("http://localhost:12345"))
            {
                while (true)
                {
                    var command = Console.ReadLine();

                    if (command == null || command.Equals("stop", StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }
//
//                    if (command.StartsWith("add", StringComparison.OrdinalIgnoreCase))
//                    {
//                        try
//                        {
//                            var workCount = int.Parse(command.Substring(4));
//                            for (var i = 0; i < workCount; i++)
//                            {
//                                var number = i;
//                                BackgroundJob.Enqueue<Services>(x => x.Random(number));
//                            }
//
//                            Console.WriteLine("Jobs enqueued.");
//                        }
//                        catch (Exception ex)
//                        {
//                            Console.WriteLine(ex.Message);
//                        }
//                    }
                }
            }

            Console.WriteLine("Press Enter to exit...");
            Console.ReadLine();
        }
    }
}