using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Client {
    static class Program {


        public static string PLAYERNAME = "";
        public static int PORT = 0;
        public static string FILENAME = "";
        public static string SERVERURL= "";
        //public static int MSSEC = 0;
        //public static int PLAYERNUMBER = 0;
        static void Main(string[] args)
        {
            if (args.Length != 0)
            {
                PLAYERNAME = args[0].Trim();
                PORT = Int32.Parse(args[2].Split(':')[2].Split('/')[0]);
                //MSSEC = Int32.Parse(args[3].Trim());
                //PLAYERNUMBER = Int32.Parse(args[4].Trim());
                SERVERURL = args[5].Trim();
                if (args.Length > 6)
                {
                    FILENAME = args[6].Trim();
                }
                new Client(PLAYERNAME, PORT);
            }
        }
    }
}
