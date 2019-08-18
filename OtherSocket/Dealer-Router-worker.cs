using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZeroMQ;
using Newtonsoft.Json;

namespace OtherSocket
{
    public class DealerRouterWorker
    {
        /// <summary>
        /// Client
        /// </summary>
        /// <param name="context"></param>
        /// <param name="i"></param>
        private static void AsyncSrv_Client(ZContext context, int i)
        {
            using (var client = new ZSocket(context, ZSocketType.DEALER))
            {
                client.Identity = Encoding.UTF8.GetBytes("Client" + i);
                client.Connect("tcp://127.0.0.1:5570");
                var poll = ZPollItem.CreateReceiver();
                var requests = 0;
                while (true)
                {
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
                            if (Equals(error, ZError.ETERM))continue;    // Interrupted
                            throw new ZException(error);
                        }
                        using (incoming)
                        {
                            Console.WriteLine("--------------client-----------------");
                            for (var j=0;j<incoming.Count;j++)
                            {
                                Console.WriteLine($"    {j}:{incoming[j].ReadString()}");
                            }
                            Console.WriteLine("--------------client-----------------");
                        }
                    }
                    using (var outgoing = new ZMessage())
                    {
                        outgoing.Add(new ZFrame("c-xxx"));
                        outgoing.Add(new ZFrame("request " + (++requests)));

                        if (client.Send(outgoing, out error)) continue;
                        if (Equals(error, ZError.ETERM))return;    // Interrupted
                        throw new ZException(error);
                    }
                }
            }
        }

        /// <summary>
        /// 这是代理
        /// </summary>
        /// <param name="context"></param>
        static void AsyncSrv_ServerTask(ZContext context)
        {
            using (var frontend = new ZSocket(context, ZSocketType.ROUTER))
            using (var backend = new ZSocket(context, ZSocketType.DEALER))
            {
                frontend.Bind("tcp://*:5570");
                backend.Bind("inproc://backend");
                for (int i = 0; i < 1; ++i)
                {
                    int j = i; new Thread(() => AsyncSrv_ServerWorker(context, j)).Start();
                }
                if (ZContext.Proxy(frontend, backend, out var error)) return;
                if (Equals(error, ZError.ETERM))return;   
                throw new ZException(error);
            }
        }
        /// <summary>
        /// 代理2
        /// </summary>
        /// <param name="context"></param>
        public static void RRBroker(ZContext context)
        {
            using (var ctx = new ZContext())
            using (var frontend = new ZSocket(ctx, ZSocketType.ROUTER))
            using (var backend = new ZSocket(ctx, ZSocketType.DEALER))
            {
                frontend.Bind("tcp://*:5570");
                backend.Bind("inproc://backend");
                Thread.Sleep(2000);
                //backend.SendFrame(new ZFrame("hahah"));
               
                var poll = ZPollItem.CreateReceiver();
                while (true)
                {
                    if (frontend.PollIn(poll, out var message, out var error, TimeSpan.FromMilliseconds(64)))
                    {
                        backend.Send(message);
                    }
                    else
                    {
                        if (Equals(error, ZError.ETERM))return;    // Interrupted
                        if (!Equals(error, ZError.EAGAIN)) throw new ZException(error);
                    }

                    if (backend.PollIn(poll, out message, out error, TimeSpan.FromMilliseconds(64)))
                    {
                        frontend.Send(message);
                    }
                    else
                    {
                        if (Equals(error, ZError.ETERM))return;    // Interrupted
                        if (!Equals(error, ZError.EAGAIN))throw new ZException(error);
                    }
                }
            }
        }

        static void AsyncSrv_ServerWorker(ZContext context, int i)
        {
            using (var worker = new ZSocket(context, ZSocketType.DEALER))
            {
                worker.Identity = Encoding.UTF8.GetBytes($"worker{i}");
                worker.Connect("inproc://backend");
                while (true)
                {
                    ZMessage request;
                    if (null == (request = worker.ReceiveMessage(out var error)))
                    {
                        if (Equals(error, ZError.ETERM))return; 
                        throw new ZException(error);
                    }
                    using (request)
                    {
                        Console.WriteLine("----------------------server---------------------");
                        var identity = request[0].ReadString();//这里读了之后就request中就删除了
                        for (var j = 0; j < request.Count; j++)
                        {
                            Console.WriteLine($"{j}:{request[j].ReadString()}");
                        }
                        Console.WriteLine("----------------------server---------------------");
                        for (var reply = 0; reply < 3; ++reply)
                        {
                            Thread.Sleep(100);
                            using (var response = new ZMessage())
                            {
                                response.Add(new ZFrame(identity));
                                response.Add(new ZFrame("xxxxxxxxx"));
                                if (worker.Send(response, out error)) continue;
                                if (Equals(error, ZError.ETERM))return;
                                throw new ZException(error);
                            }
                        }
                    }
                }
            }
        }

        //调用
        public static void AsyncSrv()
        {
            // The main thread simply starts several clients and a server, and then
            // waits for the server to finish.

            using (var context = new ZContext())
            {
                for (int i = 0; i < 1; ++i)
                {
                    int j = i; new Thread(() => AsyncSrv_Client(context, j)).Start();
                    new Thread(() => { AsyncSrv_ServerWorker(context, j); }).Start();
                }
                Thread.Sleep(1000);
                //new Thread(() => AsyncSrv_ServerTask(context)).Start();
                new Thread(() => RRBroker(context)).Start();

                // Run for 5 seconds then quit
                Thread.Sleep(5 * 1000);
            }
        }
    }
}
