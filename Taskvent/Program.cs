using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZeroMQ;

namespace Taskvent
{
    class Program
    {
        static void Main(string[] args)
        {
            TaskVent(args);
        }

        public static void TaskVent(string[] args)
        {
            using (var context = new ZContext())
            using (var sender = new ZSocket(context, ZSocketType.PUSH))
            using (var sink = new ZSocket(context, ZSocketType.PUSH))
            {
                sender.Bind("tcp://*:5557");
                sink.Connect("tcp://127.0.0.1:5558");
                Console.WriteLine("Press ENTER when the workers are ready…");
                Console.ReadKey(true);
                Console.WriteLine("Sending tasks to workers…");
                sink.Send(new byte[] { 0x00 }, 0, 1);
                var rnd = new Random();
                int i = 0;
                long total_msec = 0;  
                for (; i < 100; ++i)
                {
                    int workload = rnd.Next(100) + 1;
                    total_msec += workload;
                    byte[] action = BitConverter.GetBytes(workload);

                    Console.WriteLine("{0}", workload);
                    sender.Send(action, 0, action.Length);
                }
                Console.WriteLine("Total expected cost: {0} ms", total_msec);
            }
        }

    }
}
