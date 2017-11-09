using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Program
    {
        static int MSSEC = 0;
        static int PLAYERNUMBER = 0;
        static void Main(string[] args)
        {
            if (args.Length != 0)
            {
                MSSEC = Int32.Parse(args[0]);
                PLAYERNUMBER = Int32.Parse(args[1]);
            }
            new Server();
            Console.ReadLine();
        }
    }
}
