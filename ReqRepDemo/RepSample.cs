using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZeroMQ;

namespace ReqRepDemo
{
    public class RepSample
    {
        /// <summary>
        /// 这是基础模式
        /// </summary>
        /// <param name="args"></param>
        public static void HwServer0(string[] args)
        {
            // Create
            using (var context = new ZContext())
            using (var responder = new ZSocket(context, ZSocketType.REP))
            {
                // Bind
                responder.Bind("tcp://127.0.0.1:5555");

                //responder.Send(new ZFrame("chenchang"));//这里会报错，因为req套接字只能先receive才能发送

                while (true)
                {
                    // Receive
                    using (ZFrame request = responder.ReceiveFrame())
                    {
                        Console.WriteLine("Received {0}", request.ReadString());

                        // Do some work
                        Thread.Sleep(1);

                        // Send
                        responder.Send(new ZFrame("World"));
                    }
                }
            }
        }

        /// <summary>
        /// 多帧模式--确实只会得到一帧
        /// </summary>
        /// <param name="args"></param>
        public static void HwServer1(ZContext context)
        {
            // Create
            using (var responder = new ZSocket(context, ZSocketType.REP))
            {
                // Bind
                responder.Bind("tcp://127.0.0.1:5555");

                while (true)
                {
                    // Receive
                    using (var request = responder.ReceiveMessage())
                    {
                        Console.WriteLine("==================HwServer1 receive==================");
                        for (var i = 0; i < request.Count; i++)
                        {
                            Console.WriteLine($"{i}:{request[i].ReadString()}");
                        }
                        Console.WriteLine("==================HwServer1 receive==================");
                        // Do some work
                        Thread.Sleep(1);

                        // Send
                        responder.Send(new ZFrame("World"));
                    }
                }
            }
        }
    }
}
