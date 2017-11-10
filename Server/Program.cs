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
        public static int MSSEC = 0;
        public static int PLAYERNUMBER = 0;
        static void Main(string[] args)
        {
            if (args.Length != 0)
            {
                if (args.Length == 2)
                {
                    MSSEC = Int32.Parse(args[0].Trim());
                    PLAYERNUMBER = Int32.Parse(args[1].Trim());
                }
            }
            new Server();
            Console.ReadLine();
        }
    }
}
