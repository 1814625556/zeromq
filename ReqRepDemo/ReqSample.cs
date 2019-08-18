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

        /// <summary>
        /// 针对 router模式--设置identity
        /// </summary>
        /// <param name="context"></param>
        public static void HwClient2(ZContext context)
        {
            var endpoint = "tcp://127.0.0.1:5555";
            using (var requester = new ZSocket(context, ZSocketType.REQ))
            {
                // Connect
                requester.IdentityString = "Client2";
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
                        for (var i = 0; i < reply.Count; i++)
                        {
                            Console.WriteLine($"{i}:{reply[i].ReadString()}");
                        }
                        Console.WriteLine("==================HwClient1 receive==================");
                    }
                }
            }
        }

        /// <summary>
        /// 发送多帧消息
        /// </summary>
        /// <param name="context"></param>
        public static void HwClient3(ZContext context)
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
                    requester.SendMore(new ZFrame("cc "));
                    requester.Send(new ZFrame(requestText));

                    // Receive
                    using (var reply = requester.ReceiveMessage())
                    {
                        Console.WriteLine("==================HwClient3 receive==================");
                        for (var i = 0; i < reply.Count; i++)
                        {
                            Console.WriteLine($"{i}:{reply[i].ReadString()}");
                        }
                        Console.WriteLine("==================HwClient3 receive==================");
                    }
                }
            }
        }


        /// <summary>
        /// 这是错误的不能先接收信息--只要运行 必然报错
        /// </summary>
        /// <param name="context"></param>
        public static void HwClient4(ZContext context)
        {
            var endpoint = "tcp://127.0.0.1:5555";
            using (var requester = new ZSocket(context, ZSocketType.REQ))
            {
                requester.Connect(endpoint);
                using (var reply = requester.ReceiveMessage())
                {
                    Console.WriteLine("==================HwClient1 receive==================");
                    for (var i = 0; i < reply.Count; i++)
                    {
                        Console.WriteLine($"{i}:{reply[i].ReadString()}");
                    }
                    Console.WriteLine("==================HwClient1 receive==================");
                }
            }
        }

    }
}
