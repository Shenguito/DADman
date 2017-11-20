using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections;
using static System.Net.Mime.MediaTypeNames;
using System.Threading;

namespace PuppetMaster

{

    class Program
    {

        private static string pathLog = @".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar +
            ".." + Path.DirectorySeparatorChar + "ComLibrary" + Path.DirectorySeparatorChar +
            "bin" + Path.DirectorySeparatorChar + "Log.txt";

        

        // Handle the ApplicationExit event to know when the application is exiting.
        //ArrayList processes = new ArrayList();
        private static Dictionary<string, int> processes = new Dictionary<string, int>();

        private static ProcessLaucher processLaucher = new ProcessLaucher();
        private static string path = Directory.GetCurrentDirectory();

        private static int port = 8000;

        private bool freezed=false;
        static void Main(string[] args)
        {
            
            Console.WriteLine("Welcome!");
            String text = Console.ReadLine();

            Console.WriteLine(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())));
            while (!text.Equals("exit"))
            {

                init(text);
                try
                {
                    string inputFile = @".." + Path.DirectorySeparatorChar +
                            ".." + Path.DirectorySeparatorChar + "file" + Path.DirectorySeparatorChar + text.Split(' ')[0];
                    using (StreamReader sr = File.OpenText(inputFile))
                    {
                        string s = "";
                        while ((s = sr.ReadLine()) != null)
                        {
                            init(s);
                        }
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine("Init Error...\r\n"+e.ToString());
                }
                
                
                //Console.WriteLine(text);
                text = Console.ReadLine();
            }

            foreach (KeyValuePair<string, int> entry in processes)
            {
                try
                {
                    Process.GetProcessById(entry.Value).Kill();
                }catch
                {
                    continue;
                }

            }

        }


        public static void init(string text)
        {
            if (text.Split(' ')[0].Equals("StartClient"))
            {

                startClient(text.Split(' '));
                /*
                if (text.Split().Length == 6)
                    processLaucher.startClient(text.Split()[1], text.Split()[2], text.Split()[3], text.Split()[4], text.Split()[5], text.Split()[6]);
                else
                    Console.WriteLine("StartClient PID PCS_URL CLIENT_URL MSEC_PER_ROUND NUM_PLAYERS [filename]");
                */
            }
            else if (text.Split(' ')[0].Equals("StartServer"))
            {
                startServer(text.Split(' '));
                /* if (text.Split(' ').Length == 6)
                     processLaucher.startServer(text.Split(' ')[1], text.Split(' ')[2], text.Split(' ')[3], text.Split(' ')[4], text.Split(' ')[5]);
                 else
                     Console.WriteLine("StartServer PID PCS_URL SERVER_URL MSEC_PER_ROUND NUM_PLAYERS");
                  */
            }
            else if (text.Split(' ')[0].Equals("LocalState"))
            {
                checkLocalState(text.Split(' '));
            }
            else if (text.Split(' ')[0].Equals("GlobalStatus"))
            {
                checkGlobalState();
            }
            else if (text.Split(' ')[0].Equals("Crash"))
            {
                /* int pid = Int32.Parse(text.Split()[1]);
                 if (processes.ContainsValue(pid))
                 {

                     Process.GetProcessById(id).Kill();
                     var item = processes.First(kvp => kvp.Value == id);

                     processes.Remove(item.Key);
                 }*/
                Console.WriteLine(text);
            }
            else if (text.Split(' ')[0].Equals("Freeze"))
            {
                // https://stackoverflow.com/questions/71257/suspend-process-in-c-sharp
                int pid = Int32.Parse(text.Split(' ')[1]);
                if (processes.ContainsValue(pid))
                {
                    try
                    {
                        Process process2freeze = Process.GetProcessById(pid);
                        Console.WriteLine("1-Process has " + process2freeze.Threads.Count + " threads");
                        foreach (ProcessThread t in process2freeze.Threads)
                        {
                            Console.WriteLine("Thread pool? " + t.ToString() + "\r\n");
                            Console.WriteLine("Thread pool? " + t.ThreadState + "\r\n");
                            Console.WriteLine("Thread pool? " + t.WaitReason + "\r\n");
                        }

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(text);
                    }
                }
            }
            else if (text.Split(' ')[0].Equals("Unfreeze"))
            {
                int pid = Int32.Parse(text.Split(' ')[1]);
                if (processes.ContainsValue(pid))
                {
                    Console.WriteLine(text);
                }
            }
            else if (text.Split(' ')[0].Equals("InjectDelay"))
            {
                //injectDelay(text.Split()[1], text.Split()[2]);
                Console.WriteLine(text);
            }
            else if (text.Split()[0].Equals("Wait"))
            {
                try
                {
                    Console.WriteLine("Wait: "+ text.Split()[1]+" MSSEC");
                    System.Threading.Thread.Sleep(Int32.Parse(text.Split()[1]));
                }
                catch
                {
                    Console.WriteLine("Invalid MSSEC value...");
                }
            }

            else if (text.Split(' ')[0].Equals("Check"))
            {
                Console.WriteLine("Process: " + processes.Count + " created");
                foreach (KeyValuePair<string, int> entry in processes)
                {
                    Console.WriteLine(entry.Key + ":" + entry.Value + "\r\n");
                }
            }
            else if (text.Split(' ')[0].Equals("ServerLog"))
            {
                if (File.Exists(pathLog))
                {
                    // Open the file to read from.

                    using (StreamReader sr = File.OpenText(pathLog))
                    {

                        string s = "";
                        while ((s = sr.ReadLine()) != null)
                        {
                            Console.WriteLine(s);
                        }
                    }
                }
            }
        }


        //TODO
        public void freeze(ProcessThread processThread)
        {
            lock (processThread)
            {
                Monitor.Wait(processThread);
            }

        }
        //TODO
        public void unfreeze(ProcessThread processThread)
        {
            lock (processThread)
            {
                Monitor.Pulse(processThread);
            }
        }

        private static void startServer(string[] input)
        {

            Process process = new Process();
            //Configure the process using the StartInfo properties.
            process.StartInfo.FileName = @".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar
                + ".." + Path.DirectorySeparatorChar + "Server" + Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar
                + "Debug" + Path.DirectorySeparatorChar + "Server.exe";
            

            process.StartInfo.Arguments = input[1]+" "+input[2] + " " + port + " " + input[4]  + " " + input[5];
            port++;
            //Console.WriteLine("Path: " + path);
            process.StartInfo.WorkingDirectory = path;
            process.Start();
            processes.Add(input[1],process.Id);
        }
        private static void startClient(string[] input)
        {

            Process process = new Process();
            //Configure the process using the StartInfo properties.
            process.StartInfo.FileName = @".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar
                + ".." + Path.DirectorySeparatorChar + "Client" + Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar
                + "Debug" + Path.DirectorySeparatorChar + "Client.exe";

            process.StartInfo.Arguments = input[1]+" "+input[2] + " " + port + " " + input[4] + " " + input[5];
            port++;

            if(input.Length>6)
                process.StartInfo.Arguments += " " + input[6];
            

            process.StartInfo.WorkingDirectory = path;
            process.Start();
            processes.Add(input[1],process.Id);
        }

        private static void checkGlobalState()
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

        private static void checkLocalState(string[] input)
        {
            try
            {
                if (input[1][0] == 'S')
                {
                    string checkLog = @".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar +
                        ".." + Path.DirectorySeparatorChar + "Server" + Path.DirectorySeparatorChar +
                        "bin" + Path.DirectorySeparatorChar + "Debug"+ Path.DirectorySeparatorChar+
                        "log"+ Path.DirectorySeparatorChar+ input[2];
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
                        "log"+ input[1][1] + Path.DirectorySeparatorChar + input[2];
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

    }

}
