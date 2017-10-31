using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace foip.CLI
{
    enum OutputType
    {
        Plaintext,
        CSV,
        XML
    }

    public class Options
    {
        [Option('i', "ip", Required = true, HelpText = "The IP or IP list to scan", DefaultValue = "127.0.0.1")]
        public string IpItemsToScan { get; set; }

        [Option('p', "ports", Required = true, HelpText = "The port or port list to scan", DefaultValue = "80, 443")]
        public string PortsToScan { get; set; }

        [Option('c', "connections", Required = false, HelpText = "Max simulateneous connections", DefaultValue = 500)]
        public int MaxSimultaneousConnections { get; set; }

        [Option('t', "timeout", Required = false, HelpText = "Connection timeout (in milliseconds)", DefaultValue = 1000)]
        public int TimeoutMilliseconds { get; set; }

        [Option('r', "randomize", Required = false, HelpText = "Randomize the order in which hosts and ports are scanned", DefaultValue = false)]
        public bool Randomize { get; set; }

        //[Option('o', "output", Required = false, HelpText = "The format of the output", DefaultValue = OutputType.Plaintext)]
        //public OutputType OutputType;

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}
