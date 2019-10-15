using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
#if !NET472
using Microsoft.AspNetCore.Hosting;
#else
using Microsoft.Owin.Hosting;
#endif

namespace ConsoleSample
{
    public static class Program
    {
        public static void Main()
        {
            var nodeAddress = $"http://localhost:12345";

#if !NET472
            using (var host = new WebHostBuilder()
                .UseUrls(nodeAddress)
                .UseStartup<Startup>()
                .UseKestrel()
                .Build())
            {
                host.Start();
                MainLoop();
            }
#else
            using (WebApp.Start<Startup>(nodeAddress))
                MainLoop();
#endif

            Console.WriteLine("Press Enter to exit...");
            Console.ReadLine();
        }

        private static void MainLoop()
        {
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
                        var workCount = int.Parse(command.Substring(4));
                        for (var i = 0; i < workCount; i++)
                        {
                            var number = i;
                            BackgroundJob.Enqueue<Services>(x => x.Random(number));
                        }

                        Console.WriteLine("Jobs enqueued.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }
    }

    public class Services
    {
        private static readonly Random Rand = new Random();

        [Queue("critical")]
        public void Random(int number)
        {
            int time;
            lock (Rand)
            {
                time = Rand.Next(10);
            }

            if (time < 5)
            {
                throw new Exception();
            }

            Thread.Sleep(TimeSpan.FromSeconds(5 + time));
            Console.WriteLine("Finished task: " + number);
        }
    }
}