using foip.CLI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace foip
{
    public class Scan
    {
        public Scan(ScanOptions options)
        {
            Options = options;
        }

        public ScanOptions Options { get; }

        public void Start()
        {
            long totalIPs = Options.TotalEndpoints;
            Console.WriteLine("Total endpoints to scan: {0:N0}", totalIPs);


            var endpoints = Options.GetEndpoints();

            //This is slow because it has to evaluate the whole list
            /*
            Random rng = new Random();
            if (Options.RawOptions.Randomize)
            {
                endpoints = endpoints.Shuffle(rng);
            }
            */

            /*
            endpoints
                .AsParallel()
                //.WithDegreeOfParallelism(1000)
                .ForAll(ep =>
                {
                    using (TcpClient client = new TcpClient())
                    {
                        try
                        {
                            if (client.ConnectAsync(ep.Address, ep.Port).Wait(1000))
                            {
                                Console.WriteLine(ep);
                            }
                            else
                            {

                            }
                        }
                        catch (Exception ex)
                        {
                            //Console.WriteLine(ex);
                        }
                    }
                });
                */


            /*
            var tasks = endpoints
                .Select(ep => Task.Factory.StartNew(() =>
                {
                    //Console.WriteLine(ep);
                    using (TcpClient client = new TcpClient())
                    {
                        try
                        {
                            if (client.ConnectAsync(ep.Address, ep.Port).Wait(1000))
                            {
                                Console.WriteLine(ep);
                            }
                            else
                            {

                            }
                        }
                        catch(Exception ex) {
                            //Console.WriteLine(ex);
                        }
                    }
                }));

            Task.WaitAll(tasks.ToArray());
            */

            var semaphore = new Semaphore(Options.RawOptions.MaxSimultaneousConnections, Options.RawOptions.MaxSimultaneousConnections);

            var tasks = new List<Task>();
            foreach (var ep in endpoints)
            {
                semaphore.WaitOne();

                //Console.WriteLine(ep);
                var task = Task.Factory.StartNew(() =>
                {
                    //Console.WriteLine(ep);
                    using (TcpClient client = new TcpClient())
                    {
                        try
                        {
                            if (client.ConnectAsync(ep.Address, ep.Port).Wait(Options.RawOptions.TimeoutMilliseconds))
                            {
                                //Console.WriteLine(ep);

                                string result = Options.RawOptions.Format;
                                result = result.Replace("{" + Fields.Date.ToString().ToUpper() + "}", DateTime.Now.ToString());

                                string fqdnPlaceholder = "{" + Fields.FQDN.ToString().ToUpper() + "}";
                                if (result.Contains(fqdnPlaceholder))
                                {
                                    string fqdn = ep.Address.ToString();

                                    try
                                    {
                                        IPHostEntry entry = Dns.GetHostEntry(ep.Address);
                                        if (entry != null)
                                        {
                                            fqdn = entry.HostName;
                                        }
                                    }
                                    catch (SocketException ex)
                                    {
                                    }

                                    result = result.Replace(fqdnPlaceholder, fqdn);
                                }

                                string hostnamePlaceholder = "{" + Fields.Hostname.ToString().ToUpper() + "}";
                                if (result.Contains(hostnamePlaceholder))
                                {
                                    string hostname = ep.Address.ToString();

                                    try
                                    {
                                        IPHostEntry entry = Dns.GetHostEntry(ep.Address);
                                        if (entry != null)
                                        {
                                            string fullName = entry.HostName;
                                            hostname = fullName.Substring(0, fullName.IndexOf('.'));
                                        }
                                    }
                                    catch (SocketException ex)
                                    {
                                    }

                                    result = result.Replace(hostnamePlaceholder, hostname);
                                }

                                result = result.Replace("{" + Fields.IP.ToString().ToUpper() + "}", ep.Address.ToString());
                                result = result.Replace("{" + Fields.Port.ToString().ToUpper() + "}", ep.Port.ToString());

                                string schemePlaceholder = "{" + Fields.Scheme.ToString().ToUpper() + "}";
                                if (result.Contains(schemePlaceholder))
                                {
                                    //TODO: Consider deducing the scheme using a more definitive approach. Perhaps inspect the connection contents.

                                    string scheme;

                                    switch (ep.Port)
                                    {
                                        case 80:
                                            scheme = "http";
                                            break;

                                        case 443:
                                            scheme = "https";
                                            break;

                                        default:
                                            scheme = "ipv4";
                                            break;
                                    }

                                    result = result.Replace(schemePlaceholder, scheme);
                                }

                                Console.WriteLine(result);
                            }
                            else
                            {

                            }
                        }
                        catch (Exception ex)
                        {
                            //Console.WriteLine(ex);
                        }
                    }

                    semaphore.Release();
                }, TaskCreationOptions.LongRunning);

                tasks.Add(task);
            }
            Task.WaitAll(tasks.ToArray());

            /*
            var semaphore = new Semaphore(Options.RawOptions.MaxSimultaneousConnections, Options.RawOptions.MaxSimultaneousConnections);
            var tasks = new List<Thread>();
            foreach (var ep in endpoints)
            {
                semaphore.WaitOne();

                //Console.WriteLine(ep);
                Thread t = new Thread(new ThreadStart(() =>
                {
                    //Console.WriteLine(ep);
                    using (TcpClient client = new TcpClient())
                    {
                        try
                        {
                            if (client.ConnectAsync(ep.Address, ep.Port).Wait(Options.RawOptions.TimeoutMilliseconds))
                            {
                                Console.WriteLine(ep);
                            }
                            else
                            {

                            }
                        }
                        catch (Exception ex)
                        {
                            //Console.WriteLine(ex);
                        }
                    }

                    semaphore.Release();
                }));

                t.IsBackground = true;
                t.Start();
                tasks.Add(t);
            }
            tasks.ForEach(t => t.Join());
            */
        }
    }
}
