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

        private bool freezed=false;
        static void Main(string[] args)
        {

            // Handle the ApplicationExit event to know when the application is exiting.
            //ArrayList processes = new ArrayList();
            Dictionary<string, int> processes = new Dictionary<string, int>();

            ProcessLaucher processLaucher = new ProcessLaucher();
            Console.WriteLine("Welcome!");
            String text = Console.ReadLine();

            string path = Directory.GetCurrentDirectory();
            while (!text.Equals("exit"))
            {
                if (text.Split(' ')[0].Equals("StartClient"))
                {
                    Process process = new Process();
                    //Configure the process using the StartInfo properties.
                    
                    process.StartInfo.FileName = @".."+ Path.DirectorySeparatorChar + ".."+ Path.DirectorySeparatorChar 
                        + ".."+ Path.DirectorySeparatorChar + "pacman"+ Path.DirectorySeparatorChar + "bin"+ Path.DirectorySeparatorChar 
                        + "Debug"+ Path.DirectorySeparatorChar + "pacman.exe";

                    //process.StartInfo.Arguments = text.Split()[1] + " " + text.Split()[2] + " " + text.Split()[3] + " " + text.Split()[4] + " " + text.Split()[5] + " " + text.Split()[6];
                    process.StartInfo.WorkingDirectory = path;

                    process.Start();
                    processes.Add(text.Split(' ')[1],process.Id);

                        /*
                        if (text.Split().Length == 6)
                            processLaucher.startClient(text.Split()[1], text.Split()[2], text.Split()[3], text.Split()[4], text.Split()[5], text.Split()[6]);
                        else
                            Console.WriteLine("StartClient PID PCS_URL CLIENT_URL MSEC_PER_ROUND NUM_PLAYERS [filename]");
                        */
                    }
                    else if (text.Split(' ')[0].Equals("StartServer"))
                {
                    
                    Process process = new Process();
                    //Configure the process using the StartInfo properties.

                    process.StartInfo.FileName = @".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar
                       + ".." + Path.DirectorySeparatorChar + "Server" + Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar
                       + "Debug" + Path.DirectorySeparatorChar + "Server.exe";                     //process.StartInfo.Arguments = "-n";
                    process.StartInfo.WorkingDirectory = path;
                    //process.StartInfo.Arguments = text.Split()[1] + " " + text.Split()[2] + " " + text.Split()[3] + " " + text.Split()[4] + " " + text.Split()[5] + " " + text.Split()[6];
                    process.Start();
                    processes.Add(text.Split(' ')[1], process.Id);

                   /* if (text.Split(' ').Length == 6)
                        processLaucher.startServer(text.Split(' ')[1], text.Split(' ')[2], text.Split(' ')[3], text.Split(' ')[4], text.Split(' ')[5]);
                    else
                        Console.WriteLine("StartServer PID PCS_URL SERVER_URL MSEC_PER_ROUND NUM_PLAYERS");
                     */
                }
                else if (text.Split(' ')[0].Equals("GlobalStatus"))
                {
                    //foreach (int id in processes)
                    //{
                    //    Process p = Process.GetProcessById(id);
                    //    foreach (ProcessThread pT in p.Threads)
                    //    {
                    //        if ((pT.ThreadState & System.Diagnostics.ThreadState.Running) == System.Diagnostics.ThreadState.Running)
                    //        {
                    //            Console.WriteLine("present?");
                    //        }
                    //    }
                    //}
                    Console.WriteLine(text);
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
                else if (text.Split(' ')[0].Equals("LocalState"))
                {
                    /* string[] lines = File.ReadAllLines(path + ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar
                        + ".." + Path.DirectorySeparatorChar + "pacman" + Path.DirectorySeparatorChar + "logs" + text.Split(' ')[1] + "-" + text.Split(' ')[2]);
                    foreach (string line in lines)
                    {
                        // Use a tab to indent each line of the file.
                        Console.WriteLine("\t" + line);
                    }

                    File.Create(path + "LocalState-" + text.Split(' ')[1] + "-" + text.Split(' ')[2]);
                    File.WriteAllLines(path + "LocalState-" + text.Split(' ')[1] + "-" + text.Split(' ')[2], lines);
                    Console.WriteLine(text);
                    */
                }

                else if (text.Split()[0].Equals("Wait"))
                {
                    System.Threading.Thread.Sleep(Int32.Parse(text.Split()[1]));
                    Console.WriteLine(text);
                }

                else if (text.Split(' ')[0].Equals("Check"))
                {
                    Console.WriteLine("Process: "+processes.Count+" created");
                    foreach (KeyValuePair<string, int> entry in processes)
                    {
                        Console.WriteLine(entry.Key+":"+entry.Value+"\r\n");
                    }
                }




                Console.WriteLine(text);
                text = Console.ReadLine();
            }

            foreach (KeyValuePair<string, int> entry in processes)
            {
                try
                {
                    Process.GetProcessById(entry.Value).Kill();
                }catch(Exception e)
                {

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

    }

}
