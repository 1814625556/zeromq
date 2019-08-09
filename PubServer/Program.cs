using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZeroMQ;

namespace PubServer
{
    class Program
    {
        static void Main(string[] args)
        {
            PSEnvPub(args);
        }

        public static void WUServer(string[] args)
        {
            using (var context = new ZContext())
            using (var publisher = new ZSocket(context, ZSocketType.PUB))
            {
                string address = "tcp://*:5556";
                Console.WriteLine("I: Publisher.Bind'ing on {0}", address);
                publisher.Bind(address);

                var rnd = new Random();

                while (true)
                {
                    int zipcode = rnd.Next(99999);
                    int temperature = rnd.Next(-55, +45);
                    var update = string.Format("{0:D5} {1}", zipcode, temperature);
                    using (var updateFrame = new ZFrame(update))
                    {
                        publisher.Send(updateFrame);
                    }
                }
            }
        }

        //zframe格式，发送消息--可以过滤
        public static void PSEnvPub(string[] args)
        {
            //
            // Pubsub envelope publisher
            //
            // Author: metadings
            //

            // Prepare our context and publisher
            using (var context = new ZContext())
            using (var publisher = new ZSocket(context, ZSocketType.PUB))
            {
                publisher.Linger = TimeSpan.Zero;
                publisher.Bind("tcp://*:5563");

                int published = 0;
                while (true)
                {
                    // Write two messages, each with an envelope and content

                    using (var message = new ZMessage())
                    {
                        published++;
                        message.Add(new ZFrame($"A {published}"));
                        message.Add(new ZFrame(string.Format(" We don't like to see this.")));
                        Thread.Sleep(1000);

                        publisher.Send(message);
                    }

                    using (var message = new ZMessage())
                    {
                        published++;
                        message.Add(new ZFrame($"B {published}"));
                        message.Add(new ZFrame(string.Format(" We do like to see this.")));
                        Thread.Sleep(1000);

                        publisher.Send(message);
                    }
                }
            }
        }
    }
}
