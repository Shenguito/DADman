using ComLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PuppetMaster
{
    class ProcessLaucher
    {
        /*  a unique identiﬁer PID (of type String) via the PCS listening at the url: PCS URL
        * 
        *  The parameters MSEC PER ROUND and NUM PLAYERS specify the time duration of a round
        *  in msecs and the number of players in each game 
        * 
        *  The server shall expose its services at the address SERVER URL
        *  
        *  The client shall expose its services at the address CLIENT URL.
        *  If the optional parameter ﬁlename is speciﬁed,
        *  the client shall feed its actions from the speciﬁed trace ﬁle.
        *  Else, commands are read from keyboard
        * 
        */
        private Dictionary<string, int> processes;

        public ProcessLaucher()
        {
            processes = new Dictionary<string, int>();
        }

        public void exec(string className, string ip, string port, string args)
        {
            /*
            Process process = new Process();


            process.StartInfo.Arguments = args;
            process.StartInfo.WorkingDirectory = Util.PROJECT_ROOT + className +
                        Util.EXE_PATH + className + ".exe";
            process.Start();
            */
            //processes.Add(input[1], process.Id);

            if (ip.Equals("localhost") || ip.Equals("127.0.0.1"))
            {
                if (Util.IsLinux)
                {
                    Process.Start("mono",
                        string.Join(" ", Util.PROJECT_ROOT + className +
                        Util.EXE_PATH + className + ".exe", args));
                }
                else
                {
                    Process.Start(Util.PROJECT_ROOT + className +
                        Util.EXE_PATH + className, args);
                    Console.WriteLine("Process started with: " + args);
                }
            }
            else
            {
                string url = Util.MakeUrl("tcp",
                    ip, Util.portPCS.ToString(), Util.PCS);
                IPuppetMasterLauncher launcher = Activator.GetObject(
                    typeof(IPuppetMasterLauncher), url)
                    as IPuppetMasterLauncher;
                launcher.LaunchProcess(className, args);
            }
        }

        public void startServer(string[] input)
        {
            Console.WriteLine("StartServer");
            string argv = input[1] + " " + input[2] + " " + input[3] + " " + input[4] + " " + input[5];
            /*
            processLaucher.AddNode(input[1], Util.ExtractIPFromURL(input[2]),
                Util.ExtractPortFromURL(input[2]));

            processLaucher.AddNode(input[1], Util.ExtractIPFromURL(input[3]),
                Util.ExtractPortFromURL(input[3]));
            */
            //Console.WriteLine("Path: " + path);
            try
            {
                exec("ProcessLauncherServer", Util.ExtractIPFromURL(input[2]), Util.ExtractPortFromURL(input[2]), " ");
                Console.WriteLine("Server step1 passed");
                exec("Server", Util.ExtractIPFromURL(input[3]), Util.ExtractPortFromURL(input[3]), argv);
            }
            catch (Exception e)
            {
                Console.WriteLine("Create PCS or Server error-> " + e.ToString());
            }



        }
        public void startClient(string[] input)
        {

            Console.WriteLine("StartClient");
            string argv = input[1] + " " + input[2] + " " + input[3] + " " + input[4] + " " + input[5];

            if (input.Length > 6)
                argv += " " + input[6];
            try
            {
                exec("ProcessLauncherServer", Util.ExtractIPFromURL(input[2]), Util.ExtractPortFromURL(input[2]), " ");
                Console.WriteLine("Client step1 passed");
                exec("Client", Util.ExtractIPFromURL(input[3]), Util.ExtractPortFromURL(input[3]), argv);
            }
            catch (Exception e)
            {
                Console.WriteLine("Create PCS or Client error--> " + e.ToString());
            }

        }

        public void checkGlobalState()
        {
            foreach (KeyValuePair<string, int> entry in processes)
            {
                try
                {
                    if (Process.GetProcessById(entry.Value).Responding)
                        Console.WriteLine(entry.Key + " is alive\r\n");
                    else
                        Console.WriteLine(entry.Key + " is not responding\r\n");
                }
                catch
                {
                    Console.WriteLine(entry.Key + " is not responding\r\n");
                }
            }
        }

        public void checkLocalState(string[] input)
        {
            try
            {
                if (input[1][0] == 'S')
                {
                    string checkLog = @".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar +
                        ".." + Path.DirectorySeparatorChar + "Server" + Path.DirectorySeparatorChar +
                        "bin" + Path.DirectorySeparatorChar + "Debug" + Path.DirectorySeparatorChar +
                        "log" + Path.DirectorySeparatorChar + input[2];
                    try
                    {
                        using (StreamReader sr = File.OpenText(checkLog))
                        {
                            string s = "";
                            while ((s = sr.ReadLine()) != null)
                            {
                                Console.WriteLine(s);
                            }
                        }
                    }
                    catch
                    {

                    }
                }
                else if (input[1][0] == 'C')
                {
                    string checkLog = @".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar +
                        ".." + Path.DirectorySeparatorChar + "Client" + Path.DirectorySeparatorChar +
                        "bin" + Path.DirectorySeparatorChar + "Debug" + Path.DirectorySeparatorChar +
                        "log" + input[1][1] + Path.DirectorySeparatorChar + input[2];
                    try
                    {
                        using (StreamReader sr = File.OpenText(checkLog))
                        {
                            string s = "";
                            while ((s = sr.ReadLine()) != null)
                            {
                                Console.WriteLine(s);
                            }
                        }
                    }
                    catch
                    {

                    }
                }
            }
            catch
            {
                Console.WriteLine("Invalid PID");
            }
        }
        public void freezeProcess(int pid)
        {

        }
        public void unfreezeProcess(int pid)
        {

        }
        public void check()
        {
            Console.WriteLine("Process: " + processes.Count + " created");
            foreach (KeyValuePair<string, int> entry in processes)
            {
                Console.WriteLine(entry.Key + ":" + entry.Value + "\r\n");
            }
        }
        public void killAllProcesses()
        {
            foreach (KeyValuePair<string, int> entry in processes)
            {
                try
                {
                    Process.GetProcessById(entry.Value).Kill();
                }
                catch
                {
                    continue;
                }
            }
        }

    }
}
