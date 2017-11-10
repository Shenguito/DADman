using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Client {
    static class Program {

        public static int MSSEC = 0;
        public static int PLAYERNUMBER = 0;
        static void Main(string[] args)
        {

            if (args.Length != 0)
            {
                if (args.Length == 2) {
                    MSSEC = Int32.Parse(args[0]);
                    PLAYERNUMBER = Int32.Parse(args[1]);
                }
            }
            new Client();
        }
    }
}
