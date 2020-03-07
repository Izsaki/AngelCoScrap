using System;
using System.Diagnostics;

namespace AngelCoScrap
{
    class Program
    {
        static string GetRemoteFilter()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Remote filter options: 'only', 'open', 'no'");
            Console.Write("Remote Filter: ");
            Console.ResetColor();
            var filter = Console.ReadLine();
            return filter;
        }
        static void EndScreen(int resultCount, Stopwatch sw)
        {
            sw.Stop();

            Console.WriteLine();
            Console.WriteLine("---------------------------");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(resultCount + " company names saved.");
            Console.ResetColor();
            Console.WriteLine("Time: " + sw.Elapsed.ToString());
        }
        static void Main(string[] args)
        {
            string url = "https://angel.co";
            string username = "iusghsmhqexwksopnw@ttirv.com";
            string password = "Meet9500";
            string saveFileName = "companies.txt";
            string backupFileName = "backup.txt";
            string remoteFilter = GetRemoteFilter();

            Stopwatch sw = new Stopwatch();
            var scraper = new ScrapeHelper(saveFileName,backupFileName);

            sw.Start();
            Console.WriteLine("Scraping started at: " + DateTime.Now);
            scraper.Scrape(url, username, password, remoteFilter);
            EndScreen(scraper.AllResultCount, sw);
        }
    }
}
