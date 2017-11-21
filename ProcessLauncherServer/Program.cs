using ComLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;

namespace ProcessLauncherServer
{
    class Program
    {
        static void Main(string[] args)
        {
            ProcessLauncherServer server = new ProcessLauncherServer();
            TcpChannel channel = new TcpChannel(Util.portPCS);
            ChannelServices.RegisterChannel(channel, false);
            RemotingServices.Marshal(server, Util.PCS,
                typeof(ProcessLauncherServer));
            Console.WriteLine("Waiting to launch processes........");
            Console.ReadLine();
        }
    }
}
