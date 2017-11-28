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
        public static string SERVERNAME = "";
        public static int PORT=0;
        public static int MSSEC = 0;
        public static int PLAYERNUMBER = 0;
        
        static void Main(string[] args)
        {
            if (args.Length != 0)
            {
                SERVERNAME = args[0].Trim();
                PORT= Int32.Parse(args[2].Split(':')[2].Split('/')[0]);
                MSSEC = Int32.Parse(args[3].Trim());
                PLAYERNUMBER = Int32.Parse(args[4].Trim());
            }
            Console.WriteLine("info: "+ args[0].Trim());
            Console.WriteLine("info: " + args[2].Split(':')[2].Split('/')[0]);
            Console.WriteLine("info: " + args[3].Trim());
            Console.WriteLine("info: " + args[4].Trim());
            new Server();
            Console.ReadLine();
        }
    }
}
