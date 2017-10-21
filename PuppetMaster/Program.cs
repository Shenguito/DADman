using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetMaster
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome!");
            String text = Console.ReadLine();
            while (!text.Equals("exit"))
            {
                if (text.Split()[0].Equals("StartClient"))
                {
                    Console.WriteLine(text);
                }
                else if(text.Split()[0].Equals("StartServer"))
                {
                    Console.WriteLine(text);
                }
                else if (text.Split()[0].Equals("GlobalStatus"))
                {
                    Console.WriteLine(text);
                }
                else if (text.Split()[0].Equals("Crash"))
                {
                    Console.WriteLine(text);
                }
                else if (text.Split()[0].Equals("Freeze"))
                {
                    Console.WriteLine(text);
                }
                else if (text.Split()[0].Equals("Unfreeze"))
                {
                    Console.WriteLine(text);
                }
                else if (text.Split()[0].Equals("InjectDelay"))
                {
                    Console.WriteLine(text);
                }
                else if (text.Split()[0].Equals("LocalState"))
                {
                    Console.WriteLine(text);
                }
                




                Console.WriteLine(text);
                text = Console.ReadLine();
            }
        }
    }
}
