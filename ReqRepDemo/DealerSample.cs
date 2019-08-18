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
        public static void AsyncSrv_Client(ZContext context, int i)
        {
            using (var client = new ZSocket(context, ZSocketType.DEALER))
            {
                // Set identity to make tracing easier
                client.Identity = Encoding.UTF8.GetBytes("CLIENT" + i);
                // Connect
                client.Connect("tcp://127.0.0.1:5570");

                var poll = ZPollItem.CreateReceiver();

                var requests = 0;
                while (true)
                {
                    // Tick once per second, pulling in arriving messages
                    ZError error;
                    for (var centitick = 0; centitick < 100; ++centitick)
                    {
                        if (!client.PollIn(poll, out var incoming, out error, TimeSpan.FromMilliseconds(10)))
                        {
                            if (Equals(error, ZError.EAGAIN))
                            {
                                Thread.Sleep(1);
                                continue;
                            }
                            if (Equals(error, ZError.ETERM))
                                return;    // Interrupted
                            throw new ZException(error);
                        }
                        using (incoming)
                        {
                            var messageText = incoming[0].ReadString();
                            Console.WriteLine("[CLIENT{0}] {1}", centitick, messageText);
                        }
                    }
                    using (var outgoing = new ZMessage())
                    {
                        outgoing.Add(new ZFrame(client.Identity));
                        outgoing.Add(new ZFrame("request " + (++requests)));

                        if (client.Send(outgoing, out error)) continue;
                        if (Equals(error, ZError.ETERM))
                            return;    // Interrupted
                        throw new ZException(error);
                    }
                }
            }
        }
    }
}
