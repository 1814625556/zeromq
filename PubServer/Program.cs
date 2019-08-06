using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZeroMQ;

namespace PubServer
{
    class Program
    {
        static void Main(string[] args)
        {
            WUServer(args);
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
    }
}
