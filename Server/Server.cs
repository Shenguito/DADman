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
        public bool dead;
        public bool connected;
        public int score;
       
    }

    public class Server
    {
        private int MSECROUND = Program.MSSEC; //game speed [communication refresh time]

        private TcpChannel channel;
        
        public static string DIRECTORY = Util.PROJECT_ROOT + "Server" + Path.DirectorySeparatorChar+"bin"+ Path.DirectorySeparatorChar + Program.SERVERNAME;
        
        public Server()
        {
            channel = new TcpChannel(Program.PORT);
            if (Directory.Exists(DIRECTORY))
            {
                Directory.Delete(DIRECTORY, true);
                Console.WriteLine("Deleted: " + DIRECTORY);
            }

            Directory.CreateDirectory(DIRECTORY);
            createConnection();
        }
        
        private void createConnection()
        {
            ChannelServices.RegisterChannel(channel, false);
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(RemoteServer),
                "Server",
                WellKnownObjectMode.Singleton
            );
        }
        
    }

    public class RemoteServer : MarshalByRefObject, IServer, IServerReplication, IGeneralControlServices
    {
        internal List<Client> clientList = new List<Client>();
        private Dictionary<string, int> player_image_hashmap = new Dictionary<string, int>();
        public int numberPlayersConnected = 0;
        public delegate void delProcess(int playerNumber, string move);

        public ServerForm serverForm;
        public Dictionary<string, IServerReplication> serversConnected = new Dictionary<string, IServerReplication>();
        public bool firstServer=true;

        private static int i = 1;

        public RemoteServer()
        {
            serverForm = new ServerForm(this);
            Thread thread = new Thread(() => createServerForm());
            thread.Start();
        }

        private void createServerForm()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(serverForm);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void connectClient(string nick, string url)
        {
            Client c = new Client();
            
            Console.WriteLine("SERVER: cliente tenta ligar com url "+url+".");
           

            IClient clientProxy = (IClient)Activator.GetObject(
                typeof(IClient),
                url
            );

            try { Console.WriteLine("SERVER: " + clientProxy.ToString()); }
            catch (Exception e){ Console.WriteLine(e); }

            numberPlayersConnected++;

            //Create a connected client with below parameters:
            c.nick = nick;
            c.url = url;
            c.clientProxy = clientProxy;
            c.playernumber = numberPlayersConnected;
            c.dead = false;
            c.connected = true;
            c.score = 0;

            clientList.Add(c);

            assignPlayer(c);
            if (numberPlayersConnected == Program.PLAYERNUMBER)
            {
                //TODO3, fix when a server start don't startgame before knowing if he is the first
                Thread thread = new Thread(() =>  sendStartGame());
                thread.Start();
            }

        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void sendMove(string nick, string move)
        {
            //TODO, RECONNECT THE CLIENT, IF NEEDED
            /*
            foreach (Client c in clientList)
            {
                if (c.nick.Equals(nick) && c.connected == false)
                {
                    c.connected = true;
                    //TODO Sheng
                }
            }
            */
            
            int pl_number = player_image_hashmap[nick];
            try
            {
                this.serverForm.listMove.Add(pl_number, move);
                Console.WriteLine(i+"added: " + move + " to player " + pl_number);
                i++;
            }
            catch
            {
                //TODO error, solution, threadpool
                Console.WriteLine("Client sent a move exception (Server sendMove(...))");
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
            foreach (Client c in clientList)
            {
                if (c.playernumber == playerNumber)
                {
                    c.dead = true;
                }
                try
                {
                    c.clientProxy.playerDead(playerNumber);
                }
                catch
                {
                    c.connected = false;
                    Console.WriteLine("Exception on server sendPlayerDead()");
                }
            }
            string nick = player_image_hashmap.FirstOrDefault(x => x.Value == playerNumber).Key;
        }

        public void sendRoundUpdate(int roundID, string players_arg, string dead_arg, string monster_arg, string coins_arg) 
        {
            
            foreach (Client c in clientList)
            {
                //TODO1, WHETHER NEED A THREAD TO RECEIVE ROUND UPDATE, probably a delegate
                //new Thread(() => 
                //if(c.connected)
                try
                {
                    c.clientProxy.receiveRoundUpdate(roundID, players_arg, dead_arg, monster_arg, coins_arg);
                }
                catch
                {
                    c.connected = false;
                }
                //).Start();
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
                catch
                {
                    c.connected = false;
                    Console.WriteLine("Exception on server sendCoinEaten()");
                }

            }
            string nick = player_image_hashmap.FirstOrDefault(x => x.Value == playerNumber).Key;
            
        }

        
        public void sendStartGame()
        {
            Thread.Sleep(1000);
            if (firstServer)
            {
                string arg = " ";
                foreach (Client c in clientList)
                {
                    arg += "-" + c.nick + "_" + c.playernumber + "_" + c.url;
                }
                foreach (Client c in clientList)
                {
                    Console.WriteLine("SERVER: startGame:" + arg);
                    try
                    {
                        c.clientProxy.startGame(numberPlayersConnected, arg);
                    }

                    catch (Exception e)
                    {
                        Console.WriteLine("Exception on sendStartGame: " + e);
                    }
                }

                this.serverForm.Invoke(new delImageVisible(serverForm.startGame), new object[] { numberPlayersConnected });
                Console.WriteLine("Game started!");
                
            }
        }

        public void receiveServer(string name, string url, int roundId, string players, string monsters, string atecoins, string deadplayers)
        {
            Console.WriteLine("ConnectServer........");
            firstServer = false;
            UpdateBoard(roundId, players, monsters, atecoins, deadplayers);
            try
            {
                IServerReplication serverProxy = (IServerReplication)Activator.GetObject(
                typeof(IServerReplication),
                url);
                serversConnected.Add(name, serverProxy);
            }
            catch
            {
                Console.WriteLine("Received Server error...");
            }
        }

        public void connectServer(string name, string url)
        {
            IServerReplication serverProxy = null;
            while (serverProxy == null)
            {
                serverProxy = (IServerReplication)Activator.GetObject(
                typeof(IServerReplication),
                url);
                try
                {
                    Console.WriteLine("ConnectServer........");
                    serversConnected.Add(name, serverProxy);
                    serverProxy.receiveServer(Program.SERVERNAME, "tcp://" + Util.GetLocalIPAddress() + ":" + Util.ExtractPortFromURL(url) + "/Server",
                        serverForm.roundID, serverForm.playerLocation(),
                        serverForm.lastMonster, serverForm.atecoin, serverForm.lastDeadPlayer);
                    break;
                }
                catch
                {       
                    Thread.Sleep(500);
                }
            }
            
        }

        public void requestRound(int id)
        {
            throw new NotImplementedException();
        }

        public void Freeze()
        {
            //TODO
            throw new NotImplementedException();
        }

        public void Unfreeze()
        {
            //TODO
            throw new NotImplementedException();
        }

        public void InjectDelay(string pid1, string pid2)
        {
            //TODO
            throw new NotImplementedException();
        }

        public void newServerCreated(string serverName, string serverURL)
        {
            new Thread(() => connectServer(serverName, serverURL)).Start();
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void SendFirstRound(int roundID, string player, string monsters_arg, string atecoin)
        {
            //NOT USED YET
            /*TODO, fix the problem
            foreach (KeyValuePair<IServer, bool> entry in serversConnected)
            {
                if (entry.Value == true)
                {
                    entry.Key.UpdateBoard(roundID, player, monsters_arg, atecoin);
                }
            }
            */
            /*
            var keys = serversConnected.Keys.ToList();
            for (int i = 0; i < keys.Count; i++)
            {
                serversConnected[keys[i]] = false;
            }
            foreach (IServer key in serversConnected.Keys.ToList())
                serversConnected[key] = false;
                */
        }

        public void UpdateBoard(int roundID, string pl, string monst, string coin, string deadplayers)
        {
            Console.WriteLine("player: " + pl);
            Console.WriteLine("monster: " + monst);
            Console.WriteLine("coin: " + coin);
            Console.WriteLine("dead player: " + deadplayers);
            serverForm.UpdateBoard(roundID,pl,monst,coin, deadplayers);
        }
        //debug function
        public void CheckUserScore()
        {
            foreach(Client c in clientList)
            {
                Console.WriteLine("client: "+c.nick+"=> score="+c.score);
            }
        }
    }
}
