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
        
        static void Main(string[] args)
        {
            
            if (args.Length != 0)
            {
                SERVERNAME = args[0].Trim();
                PORT = Int32.Parse(Util.ExtractPortFromURL(args[2].Trim()).Trim());
                MSSEC = Int32.Parse(args[3].Trim());
                PLAYERNUMBER = Int32.Parse(args[4].Trim());
            }
            Console.WriteLine("info: "+ SERVERNAME);
            Console.WriteLine("info: " + PORT);
            Console.WriteLine("info: " + MSSEC);
            Console.WriteLine("info: " + PLAYERNUMBER);
            new Server();
            Console.ReadLine();


            /*TODO, TESTING THREADPOOL

            //ThrPool(thread, task);
            ThrPool tpool = new ThrPool(5, 10);
            //ThrWork work = null;
            for (int i = 0; i < 5; i++)
            {
                A a = new A(i);
                tpool.AssyncInvoke(new ThrWork(a.DoWorkA));
                B b = new B(i);
                tpool.AssyncInvoke(new ThrWork(b.DoWorkB));
            }
            Console.ReadLine();
            */
        }
    }
}
