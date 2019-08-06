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
                sink.Bind("tcp://*:5558");
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
    }
}
