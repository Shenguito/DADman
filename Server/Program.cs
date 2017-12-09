using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComLibrary;

namespace Server
{
    class Program
    {

        public static string SERVERNAME = "";
        public static int PORT=0;
        public static int MSSEC = 0;
        public static int PLAYERNUMBER = 0;
        public static bool FIRSTSERVER = false;

        static void Main(string[] args)
        {
            
            if (args.Length != 0)
            {
                SERVERNAME = args[0].Trim();
                PORT = Int32.Parse(Util.ExtractPortFromURL(args[2].Trim()).Trim());
                MSSEC = Int32.Parse(args[3].Trim());
                PLAYERNUMBER = Int32.Parse(args[4].Trim());
                if (args[5].Trim().Equals("T"))
                    FIRSTSERVER = true;

            }
            new Server();
            Console.ReadLine();
        }
    }
}
