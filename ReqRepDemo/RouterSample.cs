using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZeroMQ;

namespace ReqRepDemo
{
    public class RouterSample
    {
        public static void AsyncSrv_ServerWorker(ZContext context, int i)
        {
            using (var worker = new ZSocket(context, ZSocketType.DEALER))
            {
                worker.Connect("inproc://backend");

                var rnd = new Random();

                while (true)
                {
                    ZMessage request;
                    if (null == (request = worker.ReceiveMessage(out var error)))
                    {
                        if (error == ZError.ETERM)
                            return;    // Interrupted
                        throw new ZException(error);
                    }
                    using (request)
                    {
                        // The DEALER socket gives us the reply envelope and message
                        var identity = request[1].ReadString();
                        var content = request[2].ReadString();

                        // Send 0..4 replies back
                        int replies = rnd.Next(5)+1;
                        for (int reply = 0; reply < replies; ++reply)
                        {
                            // Sleep for some fraction of a second
                            Thread.Sleep(rnd.Next(1000) + 1);

                            using (var response = new ZMessage())
                            {
                                response.Add(new ZFrame(identity));
                                response.Add(new ZFrame(content));

                                if (!worker.Send(response, out error))
                                {
                                    if (error == ZError.ETERM)
                                        return;    // Interrupted
                                    throw new ZException(error);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
