using ComLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessLauncherServer
{
    class ProcessLauncherServer : MarshalByRefObject, IPuppetMasterLauncher
    {
        private Dictionary<string, Process> processes=new Dictionary<string, Process>();

        public void LaunchProcess(string name, string args)
        {
            Console.WriteLine("Lauching....");

            if (args == null || name == null)
                return;
            if (Util.IsLinux)
            {
                processes.Add(args.Split(' ')[0], Process.Start("mono",
                string.Join(" ", Util.PROJECT_ROOT + name + Util.EXE_PATH + name + ".exe", args)));
            }
            else
            {
                processes.Add(args.Split(' ')[0], Process.Start(Util.PROJECT_ROOT + name + Util.EXE_PATH + name + ".exe", args));
                Console.WriteLine("Process " + name + " launched with args: " + args);
            }
            string[] argv = args.Split(' ');
            Console.WriteLine("{0} {1} launched..", name, argv[1]);
        }
        public void ExitAllProcesses()
        {
            foreach (KeyValuePair<string, Process> entry in processes)
            {
                try
                {
                    entry.Value.Kill();
                }
                catch
                {
                    continue;
                }
            }
        }
        public void crashProcess(string pid)
        {
            if (processes[pid] != null)
                processes[pid].Kill();
        }
    }
}
