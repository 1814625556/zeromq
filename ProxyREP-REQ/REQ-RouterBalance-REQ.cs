﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZeroMQ;

namespace ProxyREP_REQ
{
    public class REQ_RouterBalance_REQ
    {
        static int LBBroker_Clients = 10;
        static int LBBroker_Workers = 3;

        // Basic request-reply client using REQ socket
        static void LBBroker_Client(ZContext context, int i)
        {
            // Create a socket
            using (var client = new ZSocket(context, ZSocketType.REQ))
            {
                // Set a printable identity
                client.IdentityString = "CLIENT" + i;

                // Connect
                client.Connect("inproc://frontend");

                using (var request = new ZMessage())
                {
                    request.Add(new ZFrame("Hello"));

                    // Send request
                    client.Send(request);
                }

                // Receive reply
                using (ZMessage reply = client.ReceiveMessage())
                {
                    Console.WriteLine("CLIENT{0}: {1}", i, reply[0].ReadString());
                }
            }
        }

        static void LBBroker_Worker(ZContext context, int i)
        {
            // This is the worker task, using a REQ socket to do load-balancing.

            // Create socket
            using (var worker = new ZSocket(context, ZSocketType.REQ))
            {
                // Set a printable identity
                worker.IdentityString = "WORKER" + i;

                // Connect
                worker.Connect("inproc://backend");

                // Tell broker we're ready for work
                using (var ready = new ZFrame("READY"))
                {
                    worker.Send(ready);
                }

                ZError error;
                ZMessage request;

                while (true)
                {
                    // Get request
                    if (null == (request = worker.ReceiveMessage(out error)))
                    {
                        // We are using "out error",
                        // to NOT throw a ZException ETERM
                        if (error == ZError.ETERM)
                            break;

                        throw new ZException(error);
                    }

                    using (request)
                    {
                        string worker_id = request[0].ReadString();

                        string requestText = request[2].ReadString();
                        Console.WriteLine("WORKER{0}: {1}", i, requestText);

                        // Send reply
                        using (var commit = new ZMessage())
                        {
                            commit.Add(new ZFrame(worker_id));
                            commit.Add(new ZFrame());
                            commit.Add(new ZFrame("OK"));

                            worker.Send(commit);
                        }
                    }
                }
            }
        }

        public static void LBBroker(string[] args)
        {
            using (var context = new ZContext())
            using (var frontend = new ZSocket(context, ZSocketType.ROUTER))
            using (var backend = new ZSocket(context, ZSocketType.ROUTER))
            {
                // Bind
                frontend.Bind("inproc://frontend");
                // Bind
                backend.Bind("inproc://backend");

                int clients = 0;
                for (; clients < LBBroker_Clients; ++clients)
                {
                    int j = clients;
                    new Thread(() => LBBroker_Client(context, j)).Start();
                }
                for (int i = 0; i < LBBroker_Workers; ++i)
                {
                    int j = i;
                    new Thread(() => LBBroker_Worker(context, j)).Start();
                }
                var worker_queue = new List<string>();

                ZMessage incoming;
                ZError error;
                var poll = ZPollItem.CreateReceiver();

                while (true)
                {
                    if (backend.PollIn(poll, out incoming, out error, TimeSpan.FromMilliseconds(64)))
                    {
                        string worker_id = incoming[0].ReadString();
                        worker_queue.Add(worker_id);
                        string client_id = incoming[2].ReadString();
                        if (client_id != "READY")
                        {
                            // incoming[3] is empty

                            // incoming[4] is reply
                            string reply = incoming[4].ReadString();

                            using (var outgoing = new ZMessage())
                            {
                                outgoing.Add(new ZFrame(client_id));
                                outgoing.Add(new ZFrame());
                                outgoing.Add(new ZFrame(reply));

                                // Send
                                frontend.Send(outgoing);
                            }

                            if (--clients == 0)
                            {
                                // break the while (true) when all clients said Hello
                                break;
                            }
                        }
                    }
                    if (worker_queue.Count > 0)
                    {
                        // Poll frontend only if we have available workers

                        if (frontend.PollIn(poll, out incoming, out error, TimeSpan.FromMilliseconds(64)))
                        {
                            // Here is how we handle a client request

                            // incoming[0] is client_id
                            string client_id = incoming[0].ReadString();

                            // incoming[1] is empty

                            // incoming[2] is request
                            string requestText = incoming[2].ReadString();

                            using (var outgoing = new ZMessage())
                            {
                                outgoing.Add(new ZFrame(worker_queue[0]));
                                outgoing.Add(new ZFrame());
                                outgoing.Add(new ZFrame(client_id));
                                outgoing.Add(new ZFrame());
                                outgoing.Add(new ZFrame(requestText));

                                // Send
                                backend.Send(outgoing);
                            }

                            // Dequeue the next worker identity
                            worker_queue.RemoveAt(0);
                        }
                    }
                }
            }
        }
    }
}
