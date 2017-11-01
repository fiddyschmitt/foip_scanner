using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

    public enum Fields
    {
        [Description("The current datetime")]
        Date,

        [Description("The numeric IP")]
        IP,

        [Description("The numeric Port")]
        Port,

        [Description("The hostname resolved from the IP")]
        Hostname,

        [Description("The fully qualified domain name resolved from the IP")]
        FQDN,

        [Description("A guess at the URL scheme. Eg. http, https")]
        Scheme
    }

    public class Options
    {
        [Option('i', "ip", Required = true, HelpText = "{IP_FIELD_HELP}", DefaultValue = "127.0.0.1")]
        public string IpItemsToScan { get; set; }

        [Option('p', "ports", Required = true, HelpText = "{PORTS_FIELD_HELP}", DefaultValue = "80, 443")]
        public string PortsToScan { get; set; }

        [Option('c', "connections", Required = false, HelpText = "Max simulateneous connections", DefaultValue = 500)]
        public int MaxSimultaneousConnections { get; set; }

        [Option('t', "timeout", Required = false, HelpText = "Connection timeout (in milliseconds)", DefaultValue = 1000)]
        public int TimeoutMilliseconds { get; set; }

        [Option('r', "randomize", Required = false, HelpText = "Randomize the order in which hosts and ports are scanned", DefaultValue = false)]
        public bool Randomize { get; set; }

        [Option('f', "format", Required = false, HelpText = "{FORMAT_FIELD_HELP}", DefaultValue = "{IP}:{PORT}")]
        public string Format { get; set; }

        //[Option('o', "output", Required = false, HelpText = "The format of the output", DefaultValue = OutputType.Plaintext)]
        //public OutputType OutputType;

        [HelpOption]
        public string GetUsage()
        {
            string autogenText = HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));

            string result = autogenText;
            result = result.Replace("{IP_FIELD_HELP}", GetIPFieldHelp());
            result = result.Replace("{PORTS_FIELD_HELP}", GetPortsFieldHelp());
            result = result.Replace("{FORMAT_FIELD_HELP}", GetFormatFieldHelp());

            return result;
        }

        public static string GetIPFieldHelp()
        {
            string result = string.Format(
@"The IP or IP list to scan.

{0}Can be a single IP:
{1}192.168.1.1
                    
{0}Can be a range of IPs:
{1}192.168.1.1 - 192.168.1.255

{0}Can be a combination:
{1}192.168.1.1, 10.0.0.1-10.0.0.50
",
                "\t\t\t\t",
                "\t\t\t\t\t");

            return result;
        }

        public static string GetPortsFieldHelp()
        {
            string result = string.Format(
@"The port or port list to scan.

{0}Can be a single port:
{1}80
                    
{0}Can be a range of ports:
{1}1-65535

{0}Can be a combination:
{1}80, 443, 8000-8100
",
                "\t\t\t\t",
                "\t\t\t\t\t");

            return result;
        }

        public static string GetFormatFieldHelp()
        {
            string formatFieldHelp = string.Format(
@"The format of the output line.

{0,30}For example, this format:
{0,40}{{DATE}}, {{IP}}:{{PORT}}, {{SCHEME}}://{{HOSTNAME}}/
                    
{0,30}Produces this output:
{0,40}{1}, 192.168.1.1:80, http://magpie/

{0,30}Valid fields are:
{0,40}{2}",
                " ",
                DateTime.Now,
                FieldsAsString(string.Format("{0}{1,40}", Environment.NewLine, " ")));

            return formatFieldHelp;
        }

        public static string FieldsAsString(string separator)
        {
            var fields = Enum.GetValues(typeof(Fields)).Cast<Fields>()
                .Select(f => new
                {
                    Field = f.ToString(),
                    Description = f.GetAttributeOfType<DescriptionAttribute>().Description
                })
                .OrderBy(f => f.Field)
                .Select(f => string.Format("{0,10}{1,3}{2,10}", "{" + f.Field.ToUpper() + "}", " ", f.Description))
                .ToArray();

            string result = string.Join(separator, fields.ToArray());
            return result;
        }
    }
}
