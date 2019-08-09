using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZeroMQ;

namespace REQClient
{
    class Program
    {
        static void Main(string[] args)
        {
            //HWClient(args);
            RTTest.RTDealer(args);
            Console.ReadKey();
        }
        /// <summary>
        /// 简单模式
        /// </summary>
        /// <param name="args"></param>
        public static void HWClient(string[] args)
        {
            //
            // Hello World client
            //
            // Author: metadings
            //

            if (args == null || args.Length < 1)
            {
                Console.WriteLine();
                Console.WriteLine("Usage: ./{0} HWClient [Endpoint]", AppDomain.CurrentDomain.FriendlyName);
                Console.WriteLine();
                Console.WriteLine("    Endpoint  Where HWClient should connect to.");
                Console.WriteLine("              Default is tcp://127.0.0.1:5555");
                Console.WriteLine();
                args = new string[] { "tcp://127.0.0.1:5555" };
            }

            string endpoint = args[0];

            // Create
            using (var context = new ZContext())
            using (var requester = new ZSocket(context, ZSocketType.REQ))
            {
                // Connect
                requester.Connect(endpoint);

                for (int n = 0; n < 10; ++n)
                {
                    Thread.Sleep(1000);
                    string requestText = "Hello";
                    Console.Write("Sending {0}…", requestText);

                    // Send
                    requester.Send(new ZFrame(requestText));

                    // Receive
                    using (ZFrame reply = requester.ReceiveFrame())
                    {
                        Console.WriteLine(" Received: {0} {1}!", requestText, reply.ReadString());
                    }
                }
            }
        }

        /// <summary>
        /// 代理模式
        /// </summary>
        /// <param name="args"></param>
        public static void RRClient(string[] args)
        {
            // Socket to talk to server
            using (var context = new ZContext())
            using (var requester = new ZSocket(context, ZSocketType.REQ))
            {
                requester.Connect("tcp://127.0.0.1:5559");

                for (int n = 0; n < 10; ++n)
                {
                    requester.Send(new ZFrame($"Hello{n}"));

                    using (ZFrame reply = requester.ReceiveFrame())
                    {
                        Console.WriteLine("Hello {0}!", reply.ReadString());
                    }
                    Thread.Sleep(1000);
                }
            }
        }

    }
}
