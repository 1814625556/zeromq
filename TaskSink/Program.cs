using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZeroMQ;

namespace TaskSink
{
    class Program
    {
        static void Main(string[] args)
        {
            TaskSink(args);
        }

        public static void TaskSink(string[] args)
        {
            using (var context = new ZContext())
            using (var sink = new ZSocket(context, ZSocketType.PULL))
            {
                sink.Bind("tcp://*:5558");//这里无论是Pull还是Push都可以绑定
                sink.ReceiveFrame();
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                for (int i = 0; i < 100; ++i)
                {
                    sink.ReceiveFrame();

                    if ((i / 10) * 10 == i)
                        Console.Write(":");
                    else
                        Console.Write(".");
                }
                stopwatch.Stop();
                Console.WriteLine("Total elapsed time: {0} ms", stopwatch.ElapsedMilliseconds);
            }
        }

        public static void TaskSink2(string[] args)
        {
            using (var context = new ZContext())
            using (var receiver = new ZSocket(context, ZSocketType.PULL))
            using (var controller = new ZSocket(context, ZSocketType.PUB))
            {
                receiver.Bind("tcp://*:5558");
                controller.Bind("tcp://*:5559");

                // Wait for start of batch
                receiver.ReceiveFrame();

                // Start our clock now
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                // Process 100 confirmations
                for (int i = 0; i < 100; ++i)
                {
                    receiver.ReceiveFrame();

                    if ((i / 10) * 10 == i)
                        Console.Write(":");
                    else
                        Console.Write(".");
                }

                stopwatch.Stop();
                Console.WriteLine("Total elapsed time: {0} ms", stopwatch.ElapsedMilliseconds);

                // Send kill signal to workers
                controller.Send(new ZFrame("KILL"));
            }
        }

    }
}
