using foip.CLI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
            var allResults = new ResultList(Options.RawOptions.OrderBy);
            foreach (var ep in endpoints)
            {
                if (ep.Address.ToString().EndsWith(".255"))
                {
                    //skip these
                    continue;
                }

                semaphore.WaitOne();

                //Console.WriteLine(ep);
                var task = Task.Factory.StartNew(() =>
                {
                    try
                    {
                        //Console.WriteLine(ep);

                        int connectionAttempts = 0;

                        while (connectionAttempts <= Options.RawOptions.RetryCount)
                        {
                            using (TcpClient client = new TcpClient())
                            {
                                try
                                {
                                    if (client.ConnectAsync(ep.Address, ep.Port).Wait(Options.RawOptions.TimeoutMilliseconds))
                                    {
                                        Result hit = new Result(Options, ep, DateTime.Now);

                                        string hitStr = hit.ToFormattedString();
                                        Console.WriteLine(hitStr);

                                        lock (allResults)
                                        {
                                            allResults.Add(hit);

                                            if (!string.IsNullOrEmpty(Options.RawOptions.OutputFilename))
                                            {
                                                var sortedResults = allResults.OrderAsRequested();

                                                var outputText = sortedResults
                                                    .Select(or => or.ToFormattedString())
                                                    .ToList();

                                                File.WriteAllLines(Options.RawOptions.OutputFilename, outputText);
                                            }
                                        }                           

                                        break; //connected successfully; we can stop retrying
                                    }
                                    else
                                    {
                                        //the connection timed out. It could be because the machine was too busy. Let's retry
                                    }
                                }
                                catch (Exception ex)
                                {
                                    if (ex.ToString().Contains("actively refused"))
                                    {
                                        //the machine said nothing is running on that port. Don't retry
                                        break;
                                    }
                                    else
                                    {
                                        Debug.WriteLine(ex);
                                    }
                                }
                            }

                            connectionAttempts++;
                        }

                    }
                    finally
                    {
                        semaphore.Release();
                    }
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
