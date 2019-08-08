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

        /// <summary>
        /// 接收完关闭模式
        /// </summary>
        /// <param name="args"></param>
        public static void TaskWork2(string[] args)
        {
            using (var context = new ZContext())
            using (var receiver = new ZSocket(context, ZSocketType.PULL))
            using (var sender = new ZSocket(context, ZSocketType.PUSH))
            using (var controller = new ZSocket(context, ZSocketType.SUB))
            {
                receiver.Connect("tcp://127.0.0.1:5557");
                sender.Connect("tcp://127.0.0.1:5558");

                controller.Connect("tcp://127.0.0.1:5559");
                controller.SubscribeAll();

                var poll = ZPollItem.CreateReceiver();

                ZError error;
                ZMessage message;
                while (true)
                {
                    // Process messages from either socket
                    if (receiver.PollIn(poll, out message, out error, TimeSpan.FromMilliseconds(64)))
                    {
                        int workload = message[0].ReadInt32();
                        Console.WriteLine("{0}.", workload);    // Show progress

                        Thread.Sleep(workload);    // Do the work

                        sender.Send(new byte[0], 0, 0);    // Send results to sink
                    }

                    // Any waiting controller command acts as 'KILL'
                    if (controller.PollIn(poll, out message, out error, TimeSpan.FromMilliseconds(64)))
                    {
                        break;    // Exit loop
                    }
                }
            }
        }

    }
}
