using foip.CLI;
using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace foip
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new CLI.Options();
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                ScanOptions scanOptions = new ScanOptions(options);

                Scan scan = new Scan(scanOptions);

                DateTime startTime = DateTime.Now;
                scan.Start();
                TimeSpan duration = DateTime.Now - startTime;

                Console.WriteLine(duration.ToFriendlyDisplay(3));
            }

            

            Console.WriteLine("Finished. Please press any key to continue...");
            Console.ReadKey();
        }
    }
}
