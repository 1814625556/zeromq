using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZeroMQ;

namespace OtherSocket
{
    public class DealerToDealer
    {
        public static void Test()
        {
            using (var ztx = new ZContext())
            {
                for (var i = 0; i < 22; i++)
                {
                    new Thread(() => { ClientDealer(ztx,i); }).Start();
                }
                new Thread(() => { ServerDealer(ztx); }).Start();
                Console.ReadKey();
            }
        }

        static void ServerDealer(ZContext ztx)
        {
            using (var worker = new ZSocket(ztx, ZSocketType.DEALER))
            {
                worker.Identity = Encoding.UTF8.GetBytes($"worker");
                worker.Bind("inproc://backend");
                int num = 0;
                while (true)
                {
                    if (num > 20) break;
                    using (var response = new ZMessage())
                    {
                        response.Add(new ZFrame("xxxxxxxxx"));
                        if (worker.Send(response, out var error)) continue;
                        if (Equals(error, ZError.ETERM)) return;
                    }
                    var receiveMsg = worker.ReceiveMessage();
                    Console.WriteLine($"---------------WORKER---------------");
                    for (var i = 0; i < receiveMsg.Count; i++)
                    {
                        Console.WriteLine($"{i}" + receiveMsg[i].ReadString());
                    }
                    Console.WriteLine($"---------------WORKER---------------");
                    num++;
                }
            }
        }

        static void ClientDealer(ZContext ztx,int num=0)
        {
            using (var client = new ZSocket(ztx, ZSocketType.DEALER))
            {
                client.Identity = Encoding.UTF8.GetBytes($"Client{num}");
                client.Connect("inproc://backend");

                ZMessage req = new ZMessage();
                var msg = client.ReceiveMessage();
                Console.WriteLine($"---------------Client{num}---------------");
                for (var i = 0; i < msg.Count; i++)
                {
                    Console.WriteLine($"{i}"+msg[i].ReadString());
                }
                Console.WriteLine($"---------------Client{num}---------------");
                ZMessage sendMsg = new ZMessage();
                sendMsg.Add(new ZFrame("AAA"));
                sendMsg.Add(new ZFrame("bbb"));
                sendMsg.Add(new ZFrame($"{num}"));
                client.SendMessage(sendMsg);
            }
        }
    }
}
