using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PuppetMaster
{
    class PuppetMaster
    {
        // Handle the ApplicationExit event to know when the application is exiting.
        //ArrayList processes = new ArrayList();
        private Dictionary<string, int> processes = new Dictionary<string, int>();

        private ProcessLaucher processLaucher = new ProcessLaucher();
        private string path = Directory.GetCurrentDirectory();
        
        
        
        public PuppetMaster()
        {

            Console.WriteLine("Welcome!");
            
            ProcessManager manager = new ProcessManager();

        }
    }
}
