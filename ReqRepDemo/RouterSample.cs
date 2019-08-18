using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZeroMQ;

namespace ReqRepDemo
{
    public class RouterSample
    {
        /// <summary>
        /// 这是测试多帧的时候用到的
        /// </summary>
        /// <param name="context"></param>
        public static void HwServer1(ZContext context)
        {
            // Create
            using (var responder = new ZSocket(context, ZSocketType.ROUTER))
            {
                // Bind
                responder.Bind("tcp://127.0.0.1:5555");

                while (true)
                {
                    // Receive
                    using (var request = responder.ReceiveMessage())
                    {
                        Console.WriteLine("==================HwServer1 receive==================");
                        var indentity = request[0].ReadString();
                        Console.WriteLine($"0:{indentity}");
                        for (var i = 1; i < request.Count; i++)
                        {
                            Console.WriteLine($"{i}:{request[i].ReadString()}");
                        }
                        Console.WriteLine("==================HwServer1 receive==================");
                        // Do some work
                        Thread.Sleep(1);

                        // Send
                        responder.SendMore(new ZFrame("Client2"));
                        responder.SendMore(new ZFrame());
                        responder.Send(new ZFrame("World"));
                    }
                }
            }
        }
    }
}
