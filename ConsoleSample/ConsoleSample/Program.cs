using System;
using Microsoft.AspNetCore.Hosting;

namespace ConsoleSample
{
    public static class Program
    {
        public static void Main()
        {
            var nodeAddress = $"http://localhost:12345";
            IWebHostBuilder builder = new WebHostBuilder()
                .UseUrls(nodeAddress)
                .UseStartup<Startup>()
                .UseKestrel();

            using (var host = builder.Build())
            {
                host.Start();

                Console.WriteLine("Use Ctrl-C to shutdown the host...");
                host.WaitForShutdown();
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
//                }
//            }

            Console.WriteLine("Press Enter to exit...");
            Console.ReadLine();
        }
    }
}