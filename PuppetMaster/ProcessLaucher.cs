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
        private Dictionary<string, Process> processes;
        private Dictionary<string, IPuppetMasterLauncher> pcs;
        private string serverURL;
        public ProcessLaucher()
        {
            processes = new Dictionary<string, Process>();
            pcs = new Dictionary<string, IPuppetMasterLauncher>();
        }

        public void exec(string className, string ip, string port, string args)
        {
            if (ip.Equals("localhost") || ip.Equals("127.0.0.1") || ip.Equals(Util.GetLocalIPAddress()))
            {
                if (Util.IsLinux)
                {
                    processes.Add(args.Split(' ')[0], Process.Start("mono",
                        string.Join(" ", Util.PROJECT_ROOT + className +
                        Util.EXE_PATH + className + ".exe", args)));
                }
                else
                {
                    Console.WriteLine("Localhost Launching Process... "+ Util.PROJECT_ROOT + className +
                        Util.EXE_PATH + className+"\r\n"+args);
                    processes.Add(args.Split(' ')[0], Process.Start(Util.PROJECT_ROOT + className +
                        Util.EXE_PATH + className, args));
                }
            }
            else
            {
                string url = Util.MakeUrl("tcp", ip, Util.portPCS.ToString(), Util.PCS);
                IPuppetMasterLauncher launcher = Activator.GetObject(
                    typeof(IPuppetMasterLauncher), url)
                    as IPuppetMasterLauncher;
                pcs.Add(args.Split(' ')[0], launcher);
                Console.WriteLine("Launching Process on other PC: "+url);
                try
                {
                    launcher.LaunchProcess(className, args);
                }catch(Exception e)
                {
                    Console.WriteLine("Connect to other pc fail: " + e);
                }
            }
        }

        public void startServer(string[] input)
        {
            Console.WriteLine("StartServer");
            string argv = input[1] + " " + input[2] + " " + input[3] + " " + input[4] + " " + input[5];
            
            serverURL = input[3].Trim();
            try
            {
                if (Util.ExtractIPFromURL(input[3]).Equals("localhost")||
                    Util.ExtractIPFromURL(input[3]).Equals("127.0.0.1"))
                {
                    exec("Server", Util.GetLocalIPAddress(), Util.ExtractPortFromURL(input[3]), argv);

                }else
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

            if (serverURL != null)
            {
                if(Util.ExtractIPFromURL(serverURL).Equals("localhost") ||
                    Util.ExtractIPFromURL(serverURL).Equals("127.0.0.1")){
                    argv += " " + "tcp://"+Util.GetLocalIPAddress()+":"+ Util.ExtractPortFromURL(serverURL) + "/Server";
                }else
                argv += " " + serverURL;
            }
            if (input.Length > 6)
                argv += " " + input[6];
            try
            {
                if (Util.ExtractIPFromURL(input[3]).Equals("localhost") ||
                    Util.ExtractIPFromURL(input[3]).Equals("127.0.0.1"))
                {
                    exec("Client", Util.GetLocalIPAddress(), Util.ExtractPortFromURL(input[3]), argv);
                }else
                exec("Client", Util.ExtractIPFromURL(input[3]), Util.ExtractPortFromURL(input[3]), argv);
            }
            catch (Exception e)
            {
                Console.WriteLine("Create PCS or Client error--> " + e.ToString());
            }

        }

        public void checkGlobalState()
        {
            foreach (KeyValuePair<string, Process> entry in processes)
            {
                try
                {
                    if (entry.Value.Responding)
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
        public void freezeProcess(string pid)
        {

        }
        public void unfreezeProcess(string pid)
        {

        }
        public void crash(string pid)
        {
            if(processes[pid]!=null)
                processes[pid].Kill();
            else if(pcs[pid]!=null)
                pcs[pid].crashProcess(pid);
        }
        public void check()
        {
            Console.WriteLine("Process: " + processes.Count + " created");
            foreach (KeyValuePair<string, Process> entry in processes)
            {
                Console.WriteLine(entry.Key + ":" + entry.Value.MachineName + "\r\n");
            }
        }
        public void killAllProcesses()
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
            foreach (KeyValuePair<string, IPuppetMasterLauncher> entry in pcs)
            {
                try
                {
                    entry.Value.ExitAllProcesses();
                }
                catch
                {
                    continue;
                }
            }
        }

    }
}
