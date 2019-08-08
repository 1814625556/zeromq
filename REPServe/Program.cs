using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZeroMQ;

namespace REPServe
{
    class Program
    {
        static void Main(string[] args)
        {
            MTServer(args);
        }
        /// <summary>
        /// 这是基础模式
        /// </summary>
        /// <param name="args"></param>
        public static void HwServer(string[] args)
        {
            if (args == null || args.Length < 1)
            {
                Console.WriteLine();
                Console.WriteLine("Usage: ./{0} HWServer [Name]", AppDomain.CurrentDomain.FriendlyName);
                Console.WriteLine();
                Console.WriteLine("    Name   Your name. Default: World");
                Console.WriteLine();
                args = new string[] { "World" };
            }

            string name = args[0];

            // Create
            using (var context = new ZContext())
            using (var responder = new ZSocket(context, ZSocketType.REP))
            {
                // Bind
                responder.Bind("tcp://*:5555");

                while (true)
                {
                    // Receive
                    using (ZFrame request = responder.ReceiveFrame())
                    {
                        Console.WriteLine("Received {0}", request.ReadString());

                        // Do some work
                        Thread.Sleep(1);

                        // Send
                        responder.Send(new ZFrame(name));
                    }
                }
            }
        }

        /// <summary>
        /// 代理模式
        /// </summary>
        /// <param name="args"></param>
        public static void RRWorker(string[] args)
        {
            if (args == null || args.Length < 2)
            {
                Console.WriteLine();
                Console.WriteLine("Usage: ./{0} RRWorker [Name] [Endpoint]", AppDomain.CurrentDomain.FriendlyName);
                Console.WriteLine();
                Console.WriteLine("    Name      Your Name");
                Console.WriteLine("    Endpoint  Where RRWorker should connect to.");
                Console.WriteLine("              Default is tcp://127.0.0.1:5560");
                Console.WriteLine();
                if (args.Length < 1)
                {
                    args = new string[] { "World", "tcp://127.0.0.1:5560" };
                }
                else
                {
                    args = new string[] { args[0], "tcp://127.0.0.1:5560" };
                }
            }

            string name = args[0];

            string endpoint = args[1];

            // Socket to talk to clients
            using (var context = new ZContext())
            using (var responder = new ZSocket(context, ZSocketType.REP))
            {
                responder.Connect(endpoint);

                while (true)
                {
                    // Wait for next request from client
                    using (ZFrame request = responder.ReceiveFrame())
                    {
                        Console.Write("{0} ", request.ReadString());

                        // Do some 'work'
                        Thread.Sleep(1);

                        // Send reply back to client
                        Console.WriteLine("{0}… ", name);
                        responder.Send(new ZFrame(name));
                    }
                }
            }
        }

        /// <summary>
        /// 多线程模式
        /// </summary>
        /// <param name="args"></param>
        public static void MTServer(string[] args)
        {
            //
            // Multithreaded Hello World server
            //
            // Author: metadings
            //

            // Socket to talk to clients and
            // Socket to talk to workers
            using (var ctx = new ZContext())
            using (var clients = new ZSocket(ctx, ZSocketType.ROUTER))
            using (var workers = new ZSocket(ctx, ZSocketType.DEALER))
            {
                clients.Bind("tcp://*:5555");
                workers.Bind("inproc://workers");

                // Launch pool of worker threads
                for (int i = 0; i < 5; ++i)
                {
                    new Thread(() => MTServer_Worker(ctx)).Start();
                }

                // Connect work threads to client threads via a queue proxy
                ZContext.Proxy(clients, workers);
            }
        }

        static void MTServer_Worker(ZContext ctx)
        {
            // Socket to talk to dispatcher
            using (var server = new ZSocket(ctx, ZSocketType.REP))
            {
                server.Connect("inproc://workers");

                while (true)
                {
                    using (ZFrame frame = server.ReceiveFrame())
                    {
                        Console.Write("Received: {0}", frame.ReadString());

                        // Do some 'work'
                        Thread.Sleep(1000);

                        // Send reply back to client
                        string replyText = "World";
                        Console.WriteLine(", Sending: {0}", replyText);

                        Console.WriteLine($"threadId:{Thread.CurrentThread.ManagedThreadId},time:{DateTime.Now.ToString()}");

                        server.Send(new ZFrame(replyText));
                    }
                }
            }
        }

    }
}
