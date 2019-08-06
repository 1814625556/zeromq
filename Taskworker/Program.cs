using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZeroMQ;

namespace Taskworker
{
    class Program
    {
        static void Main(string[] args)
        {
            TaskWork(args);
        }
        public static void TaskWork(string[] args)
        {
            using (var context = new ZContext())
            using (var receiver = new ZSocket(context, ZSocketType.PULL))
            using (var sink = new ZSocket(context, ZSocketType.PUSH))
            {
                receiver.Connect("tcp://127.0.0.1:5557");
                sink.Connect("tcp://127.0.0.1:5558");

                while (true)
                {
                    var replyBytes = new byte[4];
                    receiver.ReceiveBytes(replyBytes, 0, replyBytes.Length);
                    int workload = BitConverter.ToInt32(replyBytes, 0);
                    Console.WriteLine("{0}.", workload);    // Show progress

                    Thread.Sleep(workload);    // Do the work

                    sink.Send(new byte[0], 0, 0);    // Send results to sink
                }
            }
        }
    }
}
