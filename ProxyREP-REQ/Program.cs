using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZeroMQ;

namespace ProxyREP_REQ
{
    class Program
    {
        static void Main(string[] args)
        {
            //Dealer_REP.Demo();
            //ProxyDemo();
            //Router_Req.RTReq();
            Router_Dealer.RTDealer();
            Console.ReadKey();
        }

        public static void ProxyDemo()
        {
            using (ZContext ctx = new ZContext())
            {
                for (var i = 0; i < 3; i++)
                {
                    new Thread(() => RRClient(ctx)).Start();
                    new Thread(() => RRWorker(ctx)).Start();
                }
                RRBroker(ctx);
                Console.WriteLine("============success over============");
                Console.ReadKey();
            }
        }

        /// <summary>
        /// 精简版代理模式
        /// </summary>
        /// <param name="args"></param>
        public static void MsgQueue(string[] args)
        {
            //
            // Simple message queuing broker
            // Same as request-reply broker but using QUEUE device
            //
            // Author: metadings
            //

            // Socket facing clients and
            // Socket facing services
            using (var context = new ZContext())
            using (var frontend = new ZSocket(context, ZSocketType.ROUTER))
            using (var backend = new ZSocket(context, ZSocketType.DEALER))
            {
                frontend.Bind("tcp://*:5559");
                backend.Bind("tcp://*:5560");

                // Start the proxy
                ZContext.Proxy(frontend, backend);
            }
        }

        /// <summary>
        /// 客户端
        /// </summary>
        public static void RRClient(ZContext context)
        {
            using (var requester = new ZSocket(context, ZSocketType.REQ))
            {
                requester.IdentityString = DateTime.Now.ToString();
                requester.Connect("tcp://127.0.0.1:5559");

                for (var n = 0; n < 10; ++n)
                {
                    requester.Send(new ZFrame("Hello"));

                    using (ZFrame reply = requester.ReceiveFrame())
                    {
                        Console.WriteLine("Hello {0}!", reply.ReadString());
                    }
                }
            }
        }

        /// <summary>
        /// 服务端
        /// </summary>
        public static void RRWorker(ZContext context)
        {
            // Socket to talk to clients
            using (var responder = new ZSocket(context, ZSocketType.REP))
            {
                responder.Connect("tcp://127.0.0.1:5560");

                while (true)
                {
                    using (var request = responder.ReceiveFrame())
                    {
                        Console.Write("{0} ", request.ReadString());

                        Thread.Sleep(1);

                        Console.WriteLine("{0}… ", "World");
                        responder.Send(new ZFrame("World"));
                    }
                }
            }
        }

        /// <summary>
        /// 代理模式
        /// </summary>
        /// <param name="args"></param>
        public static void RRBroker(ZContext ctx)
        {
            using (var frontend = new ZSocket(ctx, ZSocketType.ROUTER))
            using (var backend = new ZSocket(ctx, ZSocketType.DEALER))
            {
                frontend.Bind("tcp://*:5559");
                backend.Bind("tcp://*:5560");

                // Initialize poll set
                var poll = ZPollItem.CreateReceiver();

                // Switch messages between sockets
                ZError error;
                ZMessage message;
                int num = 0;
                while (true)
                {
                    if (num > 100) break;
                    if (frontend.PollIn(poll, out message, out error, TimeSpan.FromMilliseconds(64)))
                    {
                        // Process all parts of the message
                        Console.WriteLine("frontend", 2, message);
                        backend.Send(message);
                    }
                    else
                    {
                        if (error == ZError.ETERM)
                            return;    // Interrupted
                        if (error != ZError.EAGAIN)
                            throw new ZException(error);
                    }

                    if (backend.PollIn(poll, out message, out error, TimeSpan.FromMilliseconds(64)))
                    {
                        // Process all parts of the message
                        Console.WriteLine(" backend", 2, message);
                        frontend.Send(message);
                    }
                    else
                    {
                        if (error == ZError.ETERM)
                            return;    // Interrupted
                        if (error != ZError.EAGAIN)
                            throw new ZException(error);
                    }
                    num++;
                }
            }
        }


    }
}
