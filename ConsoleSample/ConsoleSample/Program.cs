using System;
using Microsoft.Owin.Hosting;

namespace ConsoleSample
{
    public static class Program
    {
        public static void Main()
        {
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