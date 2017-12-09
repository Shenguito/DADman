using ComLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace PuppetMaster
{
    class ProcessLaucher
    {
        private Dictionary<string, Process> processes;
        private Dictionary<string, IPuppetMasterLauncher> pcs;
        private Dictionary<string, IGeneralControlServices> remotingProcesses;
        private string serverURL="";
        private string serverName = "";
        private string firstServer = "T";
        public ProcessLaucher()
        {
            processes = new Dictionary<string, Process>();
            pcs = new Dictionary<string, IPuppetMasterLauncher>();
            remotingProcesses = new Dictionary<string, IGeneralControlServices>();
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
            IGeneralControlServices service = Activator.GetObject(
                    typeof(IGeneralControlServices), args.Split(' ')[2])
                    as IGeneralControlServices;
            remotingProcesses.Add(args.Split(' ')[0], service);
        }

        public void startServer(string[] input)
        {
            Console.WriteLine("StartServer");
            string argv = input[1] + " " + input[2] + " " + input[3] + " " + input[4] + " " + input[5]+" "+ firstServer;
            firstServer = "F";

            serverURL = input[3].Trim();
            if (Util.ExtractIPFromURL(serverURL).Equals("localhost") ||
                    Util.ExtractIPFromURL(serverURL).Equals("127.0.0.1"))
            {
                serverURL = " " + "tcp://" + Util.GetLocalIPAddress() + ":" + Util.ExtractPortFromURL(serverURL) + "/Server";
            }
            serverName += "-" + input[1].Trim() + "_" + serverURL.Trim();
            foreach (KeyValuePair<string, IGeneralControlServices> entry in remotingProcesses)
            {
                if (!entry.Key.Equals(input[1]))
                {
                    try
                    {
                        entry.Value.newServerCreated(input[1].Trim(), serverURL.Trim());
                    }
                    catch
                    {
                        Console.WriteLine("new server error");
                    }
                }
            }
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

            if (serverURL != "")
            {
                argv += " " + serverName;
            }else
            {
                argv += " null";
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
                BoardInfo board = remotingProcesses[input[1].Trim()].getLocalState(Int32.Parse(input[2].Trim()));
                Console.WriteLine("localstate of "+ input[1].Trim() + ":");
                for (int i = 1; i < board.Players.Split('_').Length; i++)
                {
                    Console.WriteLine("player"+i+"=" + board.Players.Split('_')[i].Split(':')[0] + ":"+
                        board.Players.Split('_')[i].Split(':')[1]+" is "+ board.Players.Split('_')[i].Split(':')[2]);
                }
                for (int i = 0, j = 0; j < board.Monsters.Split(':').Length; i++, j += 2)
                {
                    Console.WriteLine("Monster" + i + "=" + board.Monsters.Split(':')[j] + ":" + board.Monsters.Split(':')[j + 1]);
                }
                for (int i = 1; i < board.Coins.Split('-').Length; i++)
                {
                    Console.WriteLine("Eaten Coins: " + board.Coins.Split('-')[i].Split('_')[0]);
                }
                Console.WriteLine();
            }
            catch
            {
                Console.WriteLine(input[1].Trim() + " localstate is not responding\r\n");
            }
            
        }
        public void freezeProcess(string pid)
        {
            remotingProcesses[pid].Freeze();
            Console.WriteLine("Freeze called");
        }
        public void unfreezeProcess(string pid)
        {
            try
            {
                remotingProcesses[pid].Unfreeze();
            }
            catch
            {
                if (remotingProcesses[pid] != null)
                {
                    Console.WriteLine("Remoting pid não é null");
                }
                else
                {
                    Console.WriteLine("Remoting pid é null");
                }
            }
        }
        public void delayProcess(string pid1, string pid2)
        {
            remotingProcesses[pid1].InjectDelay(pid2);
        }
        public void crash(string pid)
        {
            try
            {
                if (processes[pid] != null)
                {
                    try
                    {
                        processes[pid].Kill();
                    }
                    catch
                    {
                        Console.WriteLine("Already killed");
                    }
                }
            }
            catch
            {
                try
                {
                    pcs[pid].crashProcess(pid);
                }
                catch
                {
                    Console.WriteLine("Pid invalid...");
                }
            }
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
