using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZeroMQ;

namespace ReqRepDemo
{
    public class DealerSample
    {
        /// <summary>
        /// 这是多贞message模式
        /// </summary>
        /// <param name="context"></param>
        public static void HwClient1(ZContext context)
        {
            var endpoint = "tcp://127.0.0.1:5555";
            using (var requester = new ZSocket(context, ZSocketType.DEALER))
            {
                // Connect
                requester.Connect(endpoint);

                for (int n = 0; n < 10; ++n)
                {
                    Thread.Sleep(1000);
                    var requestText = "Hello";

                    // Send
                    requester.SendMore(new ZFrame());//这里需要模拟req的帧格式
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
    }
}
