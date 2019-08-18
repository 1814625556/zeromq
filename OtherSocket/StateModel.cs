using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZeroMQ;

namespace OtherSocket
{
    public class StateModel
    {
        public static void Peering1(string[] args)
        {
            // First argument is this broker's name
            // Other arguments are our peers' names
            //
            if (args == null || args.Length < 2)
            {
                Console.WriteLine();
                Console.WriteLine("Usage: {0} Peering1 World Receiver0", AppDomain.CurrentDomain.FriendlyName);
                Console.WriteLine("       {0} Peering1 Receiver0 World", AppDomain.CurrentDomain.FriendlyName);
                Console.WriteLine();
                return;
            }
            Console.WriteLine("I: preparing broker as {0}", args[0]);

            using (var context = new ZContext())
            using (var backend = new ZSocket(context, ZSocketType.PUB))
            using (var frontend = new ZSocket(context, ZSocketType.SUB))
            {
                // Bind backend to endpoint
                backend.Bind("tcp://127.0.0.1:" + args[0]);
                // Connect frontend to all peers
                frontend.SubscribeAll();
                for (int i = 1; i < args.Length; ++i)
                {
                    string peer = args[i];
                    Console.WriteLine("I: connecting to state backend at {0}", peer);
                    frontend.Connect("tcp://127.0.0.1:" + args[i]);
                }
                ZError error;
                ZMessage incoming;
                var poll = ZPollItem.CreateReceiver();
                var rnd = new Random();

                while (true)
                {
                    // Poll for activity, or 1 second timeout
                    if (!frontend.PollIn(poll, out incoming, out error, TimeSpan.FromSeconds(2)))
                    {
                        if (error == ZError.EAGAIN)
                        {
                            using (var output = new ZMessage())
                            {
                                output.Add(new ZFrame(args[0]));

                                var outputNumber = ZFrame.Create(4);
                                outputNumber.Write(rnd.Next(10));
                                output.Add(outputNumber);

                                backend.Send(output);
                            }

                            continue;
                        }
                        if (Equals(error, ZError.ETERM))
                            return;

                        throw new ZException(error);
                    }
                    using (incoming)
                    {
                        string peer_name = incoming[0].ReadString();
                        int available = incoming[1].ReadInt32();
                        Console.WriteLine("{0} - {1} workers free", peer_name, available);
                    }
                }
            }
        }

    }
}
