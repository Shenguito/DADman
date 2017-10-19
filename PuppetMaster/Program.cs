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
            String text = Console.ReadLine();
            while (!text.Equals("exit"))
            {
                Console.WriteLine(text);
                text = Console.ReadLine();
            }
        }
    }
}
