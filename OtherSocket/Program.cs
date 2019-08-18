using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtherSocket
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                args = new string[] {"5000", "5001", "5002"};
            }
            StateModel.Peering1(args);
            Console.ReadKey(true);
        }
    }
}
