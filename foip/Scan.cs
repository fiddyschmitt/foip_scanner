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
            DateTime startTime = DateTime.Now;

            long endpointsProcessed = 0;
            long totalEndpoints = Options.TotalEndpoints;
            Logger.WriteLine(string.Format("Total endpoints to scan: {0:N0}", totalEndpoints));

            Boolean allEndpointsFinished = false;
            bool finalLoop = false;

            double percentage = 0;
            TimeSpan elapsed = TimeSpan.FromSeconds(0);

            Task progressTask = null;
            if (Options.RawOptions.ShowProgress)
            {
                progressTask = Task.Factory.StartNew(() =>
                {
                    DateTime? lastProgressCalculationTime = null;
                    string line = "";
                    double endpointsPerSecond = 0;

                    while (true)
                    {
                        if (!lastProgressCalculationTime.HasValue || (DateTime.Now - lastProgressCalculationTime.Value).TotalMilliseconds > 1000 || finalLoop)
                        {
                            lastProgressCalculationTime = DateTime.Now;

                            percentage = (double)endpointsProcessed / (double)totalEndpoints * 100;
                            elapsed = DateTime.Now - startTime;

                            endpointsPerSecond = (double)endpointsProcessed / (double)elapsed.TotalSeconds;

                            long endpointsLeft = totalEndpoints - endpointsProcessed;


                            string timeLeftFriendly;
                            string estimatedFinishTimeString;
                            if (endpointsPerSecond == 0)
                            {
                                timeLeftFriendly = "Infinite";
                                estimatedFinishTimeString = "Never";
                            }
                            else
                            {
                                double secondsLeft = (double)endpointsLeft / endpointsPerSecond;
                                TimeSpan timeLeft = TimeSpan.FromSeconds(secondsLeft);
                                timeLeftFriendly = timeLeft.ToFriendlyDisplay(1);
                                DateTime estimatedFinishTime = DateTime.Now + timeLeft;
                                estimatedFinishTimeString = estimatedFinishTime.ToString();
                            }

                            line = string.Format("{0:N2}%   {1:N0}/{2:N0}   {3:N0} endpoints/second.   Elapsed: {4}   Remaining: {5}   ETA: {6}",
                                percentage,
                                endpointsProcessed,
                                totalEndpoints,
                                endpointsPerSecond,
                                elapsed.ToFriendlyDisplay(1),
                                timeLeftFriendly,
                                estimatedFinishTimeString
                                );
                        }

                        Logger.Write(string.Format("\r{0}", line));

                        Thread.Sleep(100);

                        if (finalLoop)
                        {
                            break;
                        }

                        if (allEndpointsFinished)
                        {
                            finalLoop = true;
                        }
                    }

                    string finishLine = string.Format("Finished.   {0:N2}%   {1:N0}/{2:N0}   {3:N0} endpoints/second.   Total duration: {4}   Finish time: {5}",
                                percentage,
                                endpointsProcessed,
                                totalEndpoints,
                                endpointsPerSecond,
                                elapsed.ToFriendlyDisplay(1),
                                DateTime.Now
                                );

                    Logger.WriteLine(string.Empty);
                    Logger.WriteLine(finishLine);
                });
            }

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
                                Logger.WriteLine(ep);
                            }
                            else
                            {

                            }
                        }
                        catch (Exception ex)
                        {
                            //Logger.WriteLine(ex);
                        }
                    }
                });
                */


            /*
            var tasks = endpoints
                .Select(ep => Task.Factory.StartNew(() =>
                {
                    //Logger.WriteLine(ep);
                    using (TcpClient client = new TcpClient())
                    {
                        try
                        {
                            if (client.ConnectAsync(ep.Address, ep.Port).Wait(1000))
                            {
                                Logger.WriteLine(ep);
                            }
                            else
                            {

                            }
                        }
                        catch(Exception ex) {
                            //Logger.WriteLine(ex);
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

                //Logger.WriteLine(ep);
                var task = Task.Factory.StartNew(() =>
                {
                    try
                    {
                        //Logger.WriteLine(ep);

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
                                        Logger.WriteLine(hitStr);

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

                    Interlocked.Increment(ref endpointsProcessed);

                }, TaskCreationOptions.LongRunning);

                tasks.Add(task);
            }
            Task.WaitAll(tasks.ToArray());
            allEndpointsFinished = true;

            if (progressTask != null)
            {
                progressTask.Wait();
            }

            allEndpointsFinished = true;

            /*
            var semaphore = new Semaphore(Options.RawOptions.MaxSimultaneousConnections, Options.RawOptions.MaxSimultaneousConnections);
            var tasks = new List<Thread>();
            foreach (var ep in endpoints)
            {
                semaphore.WaitOne();

                //Logger.WriteLine(ep);
                Thread t = new Thread(new ThreadStart(() =>
                {
                    //Logger.WriteLine(ep);
                    using (TcpClient client = new TcpClient())
                    {
                        try
                        {
                            if (client.ConnectAsync(ep.Address, ep.Port).Wait(Options.RawOptions.TimeoutMilliseconds))
                            {
                                Logger.WriteLine(ep);
                            }
                            else
                            {

                            }
                        }
                        catch (Exception ex)
                        {
                            //Logger.WriteLine(ex);
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
