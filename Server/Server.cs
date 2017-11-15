using ComLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Server
{
    public delegate void delImageVisible(int playerNumber);
    //client in server
    class Client
    {
        public string nick;
        public int playernumber;
        public string url;
        public IClient clientProxy;
        public int port;
        // TODO defined at end
        public bool connected;
        public int score;
       
    }

    class Server
    {
        private int MSECROUND = Program.MSSEC; //game speed [communication refresh time]
        const int PORT = 8000;
        static TcpChannel channel = new TcpChannel(PORT);

        public static string PATH = @".."+ Path.DirectorySeparatorChar+".."+ Path.DirectorySeparatorChar+
            ".."+ Path.DirectorySeparatorChar+"Server"+ Path.DirectorySeparatorChar+
            "bin"+ Path.DirectorySeparatorChar+"Log.txt";

        public Server()
        {
            /* TODO FOR LINUX / WINDOWS
            Console.WriteLine("Path.PathSeparator={0}", 
                Path.PathSeparator);
            */

            
            createConnection();

            
            // Create a file to write to.
            using (StreamWriter sw = File.CreateText(PATH))
            {
                sw.WriteLine("Server Started!");
            }
            

        }


        
        private void createConnection()
        {
            ChannelServices.RegisterChannel(channel, false);
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(RemoteServer),
                "ChatServer",
                WellKnownObjectMode.Singleton
            );
        }

        
    }
    public class RemoteServer : MarshalByRefObject, IServer
    {
        internal List<Client> clientList = new List<Client>();
        private Dictionary<string, int> player_image_hashmap = new Dictionary<string, int>();
        public int numberPlayersConnected = 0;
        public delegate void delProcess(int playerNumber, string move);


        public ServerForm serverForm;
        private ArrayList deadPlayers = new ArrayList();

        //TODO PROBLEM WITH MS INPUT
        public RemoteServer()
        {
            Thread thread = new Thread(() => createServerForm());
            thread.Start();
        }


        private void createServerForm()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(serverForm = new ServerForm(this));
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void connect(string nick, int port)
        {
            
            Client c = new Client();
            string url = "tcp://localhost:" + port + "/ChatClient";
            IClient clientProxy = (IClient)Activator.GetObject(
                typeof(IClient),
                url
            );

            numberPlayersConnected++;

            c.nick = nick;
            c.url = url;
            c.port = port;
            c.clientProxy = clientProxy;
            c.playernumber = numberPlayersConnected;

            clientList.Add(c);
            
            using (StreamWriter sw = File.AppendText(Server.PATH))
            {
                sw.WriteLine("Conneted: " + nick + " at port: " + port);
            }

            foreach (Client client in clientList)
            {

                Console.WriteLine("Client : " + nick + " Client to sent:" + client.nick);
                if (!client.nick.Equals(nick))
                {
                    client.clientProxy.broadcastClientURL(numberPlayersConnected, nick, port);
                }
                else
                {
                    foreach (Client oldClient in clientList)
                    {
                        Thread thread = new Thread(() => client.clientProxy.broadcastClientURL(oldClient.playernumber, oldClient.nick, oldClient.port));
                        thread.Start();
                    }
                }
            }

            //Creates a correspondence Nick - Player Number i.e. John - Player1

            assignPlayer(c);
            sendStartGame();
        }

        public void sendMove(string nick, string move)
        {
            /*
            int playerNumber=0;
            
            foreach(Client c in clientList)
            {
                if (c.nick.Equals(nick))
                    playerNumber = c.playernumber;
            }
            
            Console.WriteLine("player"+ playerNumber + ": " + nick + "\t receives: " + move);
            */
            int pl_number = player_image_hashmap[nick];

            try
            {
                this.serverForm.listMove.Add(pl_number, move);
                using (StreamWriter sw = File.AppendText(Server.PATH))
                {
                    sw.WriteLine(nick + "[Player" + +pl_number + "]" + " move: " + move + ".");
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("Send a Move Bug: " + e.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("Client already sent a move in this round...\r\n");
            }

            //this.serverForm.Invoke(new delProcess(serverForm.processMove), new object[] { pl_number, move });
            

        }


        private void assignPlayer(Client c)
        {
            player_image_hashmap.Add(c.nick, c.playernumber);

            foreach (KeyValuePair<string, int> entry in player_image_hashmap)
            {
                Console.WriteLine("INFO: " + entry.Key + " is " + entry.Value);
            }

        }

        public void sendPlayerDead(int playerNumber)
        {
            deadPlayers.Add(playerNumber);
            //TODO Remove Disconnected Client #1
            List<Client> tmpClient = new List<Client>();
            foreach (Client c in clientList)
            {
                try
                {
                    Console.WriteLine("Debug: "+c.nick);
                    c.clientProxy.playerDead(playerNumber);
                }
                catch (SocketException e)
                {
                    //TODO Remove Disconnected Client #2
                    tmpClient.Add(c);
                    Console.WriteLine("Debug: " + e.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception on server sendPlayerDead()");
                }
            }
            //TODO Remove Disconnected Client #3
            if (tmpClient.Count != 0)
            {
                foreach (Client c in tmpClient)
                {
                    if (clientList.Contains(c))
                        clientList.Remove(c);
                }
            }
            string nick = player_image_hashmap.FirstOrDefault(x => x.Value == playerNumber).Key;
            using (StreamWriter sw = File.AppendText(Server.PATH))
            {
                sw.WriteLine(nick + "[Player" + +playerNumber + "]" + " dead.");
            }
        }

        public void sendCoinEaten(int playerNumber, string coinName)
        {
            foreach (Client c in clientList)
            {
                try
                {
                    c.clientProxy.coinEaten(playerNumber, coinName);
                }
                catch (SocketException e)
                {
                    Console.WriteLine("Debug: " + e.ToString());
                }
                catch
                {
                    Console.WriteLine("Exception on server sendCoinEaten()");
                }

            }
            string nick = player_image_hashmap.FirstOrDefault(x => x.Value == playerNumber).Key;
            using (StreamWriter sw = File.AppendText(Server.PATH))
            {
                sw.WriteLine(nick + "[Player" + +playerNumber + "]" + " ate a coin.");
            }
        }

        public void sendStartGame()
        {
            
            if (numberPlayersConnected == Program.PLAYERNUMBER) {
                this.serverForm.Invoke(new delImageVisible(serverForm.startGame), new object[] { numberPlayersConnected });
                foreach (Client c in clientList)
                {
                    try
                    {
                        c.clientProxy.startGame(numberPlayersConnected);
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
                using (StreamWriter sw = File.AppendText(Server.PATH))
                {
                    sw.WriteLine("Game started!");
                }
                Console.WriteLine("Game started!");
            }
            
            

        }
    }
}
