using System;
using Microsoft.AspNetCore.Hosting;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;

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

            var cancellationToken = new CancellationTokenSource();
            using (var host = builder.Build())
            {
                host.Start();

#pragma warning disable 4014
                host.WaitForShutdownAsync(cancellationToken.Token);
#pragma warning restore 4014

                while (true)
                {
                    var command = Console.ReadLine();
                    if (command == null || command.Equals("stop", StringComparison.OrdinalIgnoreCase))
                    {
                        cancellationToken.Cancel();
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

                Console.WriteLine("Press Enter to exit...");
                Console.ReadLine();
            }
        }
    }

    public class Services
    {
        private static readonly Random Rand = new Random();
        
        [Queue("critical")]
        public async Task Random(int number)
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