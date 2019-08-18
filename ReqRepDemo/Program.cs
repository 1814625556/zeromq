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
                new Thread(()=> { RepSample.HwServer1(context); }).Start();
                new Thread(()=> { ReqSample.HwClient1(context); }).Start();
                Console.ReadKey();
            }

        }
        public static void AsyncSrv()
        {
            // The main thread simply starts several clients and a server, and then
            // waits for the server to finish.

            using (var context = new ZContext())
            {
                for (int i = 0; i < 5; ++i)
                {
                    int j = i; new Thread(() => DealerSample.AsyncSrv_Client(context, j)).Start();
                }
                new Thread(() => AsyncSrv_ServerTask(context)).Start();

                // Run for 5 seconds then quit
                Thread.Sleep(5 * 1000);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        static void AsyncSrv_ServerTask(ZContext context)
        {
            using (var frontend = new ZSocket(context, ZSocketType.ROUTER))
            using (var backend = new ZSocket(context, ZSocketType.DEALER))
            {
                // Frontend socket talks to clients over TCP
                frontend.Bind("tcp://*:5570");
                // Backend socket talks to workers over inproc
                backend.Bind("inproc://backend");

                // Launch pool of worker threads, precise number is not critical
                for (int i = 0; i < 5; ++i)
                {
                    int j = i; new Thread(() => DealerSample.AsyncSrv_Client(context, j)).Start();
                }

                // Connect backend to frontend via a proxy
                ZError error;
                if (!ZContext.Proxy(frontend, backend, out error))
                {
                    if (error == ZError.ETERM)
                        return;    // Interrupted
                    throw new ZException(error);
                }
            }
        }
    }
}
