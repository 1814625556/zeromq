using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZeroMQ;

namespace ReqRepDemo
{
    public class ReqSample
    {
        /// <summary>
        /// 简单模式
        /// </summary>
        public static void HwClient0(ZContext context)
        {
            var endpoint = "tcp://127.0.0.1:5555";
            using (var requester = new ZSocket(context, ZSocketType.REQ))
            {
                // Connect
                requester.Connect(endpoint);

                for (int n = 0; n < 10; ++n)
                {
                    Thread.Sleep(1000);
                    var requestText = "Hello";
                    Console.Write("Sending {0}…", requestText);

                    // Send
                    requester.Send(new ZFrame(requestText));

                    // Receive
                    using (var reply = requester.ReceiveFrame())
                    {
                        Console.WriteLine(" Received: {0} {1}!", requestText, reply.ReadString());
                    }
                }
            }
        }

        /// <summary>
        /// 这是多贞message模式
        /// </summary>
        /// <param name="context"></param>
        public static void HwClient1(ZContext context)
        {
            var endpoint = "tcp://127.0.0.1:5555";
            using (var requester = new ZSocket(context, ZSocketType.REQ))
            {
                // Connect
                requester.Connect(endpoint);

                for (int n = 0; n < 10; ++n)
                {
                    Thread.Sleep(1000);
                    var requestText = "Hello";

                    // Send
                    requester.Send(new ZFrame(requestText));

                    // Receive
                    using (var reply = requester.ReceiveMessage())
                    {
                        Console.WriteLine("==================HwClient1 receive==================");
                        for (var i = 0; i < reply.Count;i++)
                        {
                            Console.WriteLine($"{i}:{reply[i].ReadString()}");
                        }
                        Console.WriteLine("==================HwClient1 receive==================");
                    }
                }
            }
        }

        // Basic request-reply client using REQ socket
        public static void LBBroker_Client(ZContext context, int i)
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
    }
}
