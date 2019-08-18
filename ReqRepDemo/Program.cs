using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZeroMQ;

namespace ReqRepDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var context = new ZContext())
            {
                new Thread(()=> { RepSample.HwServer2(context); }).Start();
                new Thread(()=> { ReqSample.HwClient4(context); }).Start();
                Console.ReadKey();
            }

        }
    }
}
