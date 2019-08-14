using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZeroMQ;

namespace ProxyREP_REQ
{
    public class Dealer_REP
    {
        public static void Demo2()
        {
            using (ZContext ztx = new ZContext())
            {
                new Thread(() => Req(ztx)).Start();
                for (var i = 0; i < 3; i++)
                {
                    new Thread(() => Rep(ztx)).Start();
                }
                Console.WriteLine("=========demo over===========");
                Console.ReadKey();
            }
        }

        /// <summary>
        /// dealer-to-rep
        /// </summary>
        public static void Demo()
        {
            using (ZContext ztx = new ZContext())
            {
                new Thread(() => Dealer(ztx)).Start();
                for (var i = 0; i < 3; i++)
                {
                    new Thread(() => RepDealer(ztx,i)).Start();
                }
                Console.WriteLine("=========demo over===========");
                Console.ReadKey();
            }
        }

        static void Req(ZContext ztx)
        {
            using (var requester = new ZSocket(ztx, ZSocketType.REQ))
            {
                // Connect
                requester.Connect("tcp://127.0.0.1:5555");

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

        static void Dealer(ZContext ztx)
        {
            using (var requester = new ZSocket(ztx, ZSocketType.DEALER))
            {
                // Connect
                requester.Bind("inproc://workers");
                for (var i = 0; i < 10; i++)
                {
                    // Send
                    ZMessage msg = new ZMessage();
                    msg.Add(new ZFrame($"aaa{i}"));
                    msg.Add(new ZFrame());
                    msg.Add(new ZFrame("hello"));
                    requester.Send(msg);
                    // Receive
                    using (var reply = requester.ReceiveMessage())
                    {
                        Console.WriteLine($"Dealer Received:{reply[2].ToString()}");
                    }
                }
            }
        }

        static void Rep(ZContext ztx)
        {
            // Create
            using (var responder = new ZSocket(ztx, ZSocketType.REP))
            {
                // Bind
                responder.Bind("tcp://127.0.0.1:5555");

                while (true)
                {
                    // Receive
                    using (ZFrame request = responder.ReceiveFrame())
                    {
                        Console.WriteLine("Received {0}", request.ReadString());

                        // Do some work
                        Thread.Sleep(1);

                        // Send
                        responder.Send(new ZFrame("world"));
                    }
                }
            }

        }

        static void RepDealer(ZContext ztx,int num=0)
        {
            using (var server = new ZSocket(ztx, ZSocketType.REP))
            {
                server.Connect("inproc://workers");
                while (true)
                {
                    using (ZFrame frame = server.ReceiveFrame())
                    {
                        Thread.Sleep(1);
                        Console.WriteLine("num:{0}, REP Received: {1}", num, frame.ReadString());
                        server.Send(new ZFrame("chenchang"));
                    }
                }
            }
        }

    }
}
