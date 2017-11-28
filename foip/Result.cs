using foip.CLI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace foip
{
    public class Result
    {
        public ScanOptions Options;
        public IPEndPoint Endpoint;
        public DateTime Date;

        public Result(ScanOptions options, IPEndPoint endpoint, DateTime date)
        {
            Options = options;
            Endpoint = endpoint;
            Date = date;

            FQDN = GetFQDN();
            Hostname = GetHostname();
            Scheme = GetScheme();
        }

        public string FQDN;
        private string GetFQDN()
        {
            string result = Endpoint.Address.ToString();

            try
            {
                IPHostEntry entry = Dns.GetHostEntry(Endpoint.Address);
                if (entry != null)
                {
                    result = entry.HostName;
                }
            }
            catch (SocketException ex)
            {
            }
            return result;
        }

        public string Hostname;
        private string GetHostname()
        {
            string result = Endpoint.Address.ToString();

            try
            {
                IPHostEntry entry = Dns.GetHostEntry(Endpoint.Address);
                if (entry != null)
                {
                    string fullName = entry.HostName;
                    if (fullName.Contains("."))
                    {
                        result = fullName.Substring(0, fullName.IndexOf('.'));
                    }
                }
            }
            catch (SocketException ex)
            {
            }


            return result;
        }

        public string Scheme;
        private string GetScheme()
        {
            string result;
            switch (Endpoint.Port)
            {
                case 80:
                    result = "http";
                    break;

                case 443:
                    result = "https";
                    break;

                default:
                    result = "ipv4";
                    break;
            }

            return result;
        }

        public string ToFormattedString()
        {
            string result = Options.RawOptions.Format;
            result = result.Replace("{" + Fields.Date.ToString().ToUpper() + "}", this.Date.ToString());

            string fqdnPlaceholder = "{" + Fields.FQDN.ToString().ToUpper() + "}";
            if (result.Contains(fqdnPlaceholder))
            {
                result = result.Replace(fqdnPlaceholder, FQDN);
            }

            string hostnamePlaceholder = "{" + Fields.Hostname.ToString().ToUpper() + "}";
            if (result.Contains(hostnamePlaceholder))
            {
                result = result.Replace(hostnamePlaceholder, Hostname);
            }

            result = result.Replace("{" + Fields.IP.ToString().ToUpper() + "}", Endpoint.Address.ToString());
            result = result.Replace("{" + Fields.Port.ToString().ToUpper() + "}", Endpoint.Port.ToString());

            string schemePlaceholder = "{" + Fields.Scheme.ToString().ToUpper() + "}";
            if (result.Contains(schemePlaceholder))
            {
                //TODO: Consider deducing the scheme using a more definitive approach. Perhaps inspect the connection contents. Perhaps use some library to determine port -> scheme.
                result = result.Replace(schemePlaceholder, Scheme);
            }

            return result;
        }
    }
}
