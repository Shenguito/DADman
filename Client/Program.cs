using ComLibrary;
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
        static void Main(string[] args)
        {
            if (args.Length != 0)
            {
                PLAYERNAME = args[0].Trim();
                PORT = Int32.Parse(Util.ExtractPortFromURL(args[2]).Trim());
                SERVERURL = args[5].Trim();
                if (args.Length > 6)
                {
                    FILENAME = args[6].Trim();
                }
                new Client();
            }
        }
    }
}
