﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZeroMQ;

namespace ProxyREP_REQ
{
    class Router_Req
    {
        static int RTReq_Workers = 10;

        public static void RTReq()
        {
            //
            // ROUTER-to-REQ example
            //
            // While this example runs in a single process, that is only to make
            // it easier to start and stop the example. Each thread has its own
            // context and conceptually acts as a separate process.
            //
            // Author: metadings
            //

            using (var context = new ZContext())
            using (var broker = new ZSocket(context, ZSocketType.ROUTER))
            {
                broker.Bind("tcp://*:5671");

                for (var i = 0; i < RTReq_Workers; i++)
                {
                    var j = i;//这里是很有必要加上的，因为多线程的原因会导致i的争夺
                    new Thread(() => RTReq_Worker(j)).Start();
                }

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                // Run for five seconds and then tell workers to end
                int workers_fired = 0;
                while (true)
                {
                    // Next message gives us least recently used worker
                    using (ZMessage identity = broker.ReceiveMessage())
                    {
                        broker.SendMore(identity[0]);
                        broker.SendMore(new ZFrame());

                        // Encourage workers until it's time to fire them
                        if (stopwatch.Elapsed < TimeSpan.FromSeconds(5))
                        {
                            broker.Send(new ZFrame("Work harder!"));
                        }
                        else
                        {
                            broker.Send(new ZFrame("Fired!"));

                            if (++workers_fired == RTReq_Workers)
                            {
                                Console.WriteLine($"workers_fired:{workers_fired}");
                                break;
                            }
                        }
                    }
                }
            }
        }

        static void RTReq_Worker(int i)
        {
            using (var context = new ZContext())
            using (var worker = new ZSocket(context, ZSocketType.REQ))
            {
                worker.IdentityString = "PEER" + i;    // Set a printable identity
                worker.Connect("tcp://127.0.0.1:5671");

                int total = 0;
                while (true)
                {
                    // Tell the broker we're ready for work
                    worker.Send(new ZFrame("Hi Boss"));

                    // Get workload from broker, until finished
                    using (ZFrame frame = worker.ReceiveFrame())
                    {
                        bool finished = (frame.ReadString() == "Fired!");
                        if (finished)
                        {
                            break;
                        }
                    }

                    total++;

                    // Do some random work
                    Thread.Sleep(10);
                }

                Console.WriteLine("Completed: PEER{0}, {1} tasks", i, total);
            }
        }
    }
}
