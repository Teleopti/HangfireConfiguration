﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Configuration;
#if !NET472
using Microsoft.AspNetCore.Hosting;
#else
using Microsoft.Owin.Hosting;
#endif

namespace ConsoleSample
{
    public static class Program
    {
        public static string NodeAddress = "http://localhost:12345";
        
        public static void Main()
        {
            Console.WriteLine("Starting site on " + NodeAddress);
            
#if !NET472
            using (var host = new WebHostBuilder()
                .UseUrls(NodeAddress)
                .UseStartup<Startup>()
                .UseKestrel()
                .Build())
            {
                host.Start();
                MainLoop();
            }
#else
            using (WebApp.Start<Startup>(NodeAddress))
                MainLoop();
#endif

            Console.WriteLine("Press Enter to exit...");
            Console.ReadLine();
        }

        private static void MainLoop()
        {
            Console.WriteLine("Started.");
            Console.WriteLine("'stop' to exit.");
            Console.WriteLine("'add 1' to queue a job.");
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
                            var client = new BackgroundJobClient(HangfireConfiguration.Current.QueryPublishers().First().JobStorage);
                            client.Enqueue<Services>(x => x.Random(number));
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