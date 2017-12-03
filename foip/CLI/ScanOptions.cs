using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace foip.CLI
{
    public class ScanOptions
    {
        public IEnumerable<IPAddress> Addresses { get; }
        public IEnumerable<int> Ports { get; }
        public Options RawOptions { get; }

        public static IEnumerable<UInt32> ToRange(UInt32 minInclusive, UInt32 maxInclusive)
        {
            for (UInt32 i = minInclusive; i <= maxInclusive; i++)
            {
                yield return i;
            }
        }

        public static IEnumerable<IPAddress> ExtractIPs(string ipItemList)
        {
            Regex listRegex = new Regex("(.*?)-(.*)");
            IEnumerable<IPAddress> result = new List<IPAddress>();
            IPAddress IP;

            string[] tokens = ipItemList.Split(',');

            tokens
                .Select(t => t.Trim())
                .ToList()
                .ForEach(item =>
                {
                    if (IPAddress.TryParse(item, out IP))
                    {
                        //It's a normal IP
                        var l = new List<IPAddress> { IP };
                        result = result.Concat(l.AsEnumerable<IPAddress>());
                    }
                    else
                    {
                        Match m = listRegex.Match(item);

                        if (m.Success)
                        {
                            //Logger.WriteLine(m.Groups[1].Value.Trim() + " -> " + m.Groups[2].Value.Trim());

                            IPAddress fromIP = IPAddress.Parse(m.Groups[1].Value.Trim());
                            IPAddress toIP = IPAddress.Parse(m.Groups[2].Value.Trim());

                            UInt32 fromIPNumeric = fromIP.ToUInt32();
                            UInt32 toIPIntNumeric = toIP.ToUInt32();

                            var range = ToRange(fromIPNumeric, toIPIntNumeric).Select(n => n.ToIPAddress());
                            result = result.Concat(range);
                        }
                    }
                });

            return result;
        }

        public static IEnumerable<int> ExtractPorts(string portItemList)
        {
            Regex listRegex = new Regex("(.*?)-(.*)");
            IEnumerable<int> result = new List<int>();
            int port;

            string[] tokens = portItemList.Split(',');

            tokens
                .Select(t => t.Trim())
                .ToList()
                .ForEach(item =>
                {
                    if (int.TryParse(item, out port))
                    {
                        //It's a normal IP
                        var l = new List<int> { port };
                        result = result.Concat(l.AsEnumerable<int>());
                    }
                    else
                    {
                        Match m = listRegex.Match(item);

                        if (m.Success)
                        {
                            //Logger.WriteLine(m.Groups[1].Value.Trim() + " -> " + m.Groups[2].Value.Trim());

                            int startPort = int.Parse(m.Groups[1].Value.Trim());
                            int endPort = int.Parse(m.Groups[2].Value.Trim());

                            result = result.Concat(Enumerable.Range(startPort, endPort));
                        }
                    }
                });

            return result;
        }

        public ScanOptions(Options options)
        {
            Addresses = ExtractIPs(options.IpItemsToScan);

            //foreach (var ip in Addresses)
            {
                //Logger.WriteLine("\t" + ip.ToString());
            }
            //Addresses. ToList().ForEach(ip => Logger.WriteLine("\t" + ip.ToString()));

            Ports = ExtractPorts(options.PortsToScan);
            this.RawOptions = options;
            //Ports.ToList().ForEach(port => Logger.WriteLine("\t" + port.ToString()));
        }

        public long TotalEndpoints
        {
            get
            {
                long result = (long)Ports.Count() * (long)Addresses.Count();
                return result;
            }
        }

        public IEnumerable<IPEndPoint> GetEndpoints()
        {
            IEnumerable<int> ports;
            Random rng = new Random();
            if (RawOptions.Randomize)
            {
                ports = Ports.Shuffle(rng).ToList();
            } else
            {
                ports = Ports.ToList();
            }

            IEnumerable<IPAddress> addresses;
            if (RawOptions.Randomize)
            {
                addresses = Addresses.Shuffle(rng).ToList();
            } else
            {
                addresses = Addresses.ToList();
            }

            foreach (int port in ports)
            {
                foreach (IPAddress address in addresses)
                {
                    yield return new IPEndPoint(address, port);
                }
            }
        }
    }
}
