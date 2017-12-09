using ComLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Server
{
    public delegate void delImageVisible(int playerNumber);

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
        public List<ConnectedClient> clientList = new List<ConnectedClient>();
        List<int> disconnectedPlayers = new List<int>();
        public int numberPlayersConnected = 0;
        public delegate void delProcess(int playerNumber, string move);

        public ServerForm serverForm;
        public Dictionary<string, IServerReplication> serversConnected = new Dictionary<string, IServerReplication>();
        

        public bool freeze=false;
        public bool delay=false;
        

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
            Console.WriteLine("SERVER: cliente tenta ligar com url "+url+".");
            IClient clientProxy = (IClient)Activator.GetObject(
                typeof(IClient),
                url
            );

            try { Console.WriteLine("SERVER: " + clientProxy.ToString()); }
            catch (Exception e){ Console.WriteLine(e); }

            numberPlayersConnected++;
            ConnectedClient c = new ConnectedClient(nick, numberPlayersConnected, url, clientProxy);
            //Create a connected client with below parameters:

            clientList.Add(c);
            
            if (numberPlayersConnected == Program.PLAYERNUMBER&&Program.FIRSTSERVER)
            {
                //TODO3, fix when a server start don't startgame before knowing if he is the first
                Thread thread = new Thread(() =>  sendStartGame());
                thread.Start();
            }

        }

        public void sendMove(Movement move)
        {
            //testing round
            //Console.WriteLine(move.roundID + "-round");
            this.serverForm.ReceivingMove(move);
        }
        
        public void sendPlayerDead(int playerNumber)
        {
            foreach (ConnectedClient c in clientList)
            {
                if (c.playernumber == playerNumber)
                {
                    serverForm.deadPlayer.Add(playerNumber);
                    c.dead = true;
                }
                if(!disconnectedPlayers.Contains(c.playernumber))
                try
                {
                    c.clientProxy.playerDead(playerNumber);
                }
                catch
                {
                    Console.WriteLine("Exception on server sendPlayerDead()");
                }
            }
        }
        //SEND TO CLIENT
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void sendRoundUpdate(BoardInfo board) 
        {
            /*
            Console.WriteLine("start:");
            Console.WriteLine("round: " + board.RoundID);
            Console.WriteLine("player: " + board.Players + " : " + board.PlayerDead);
            Console.WriteLine("Monster: " + board.Monsters);
            Console.WriteLine("AteCoins: " + board.AteCoins);
            Console.WriteLine("Coins: " + board.Coins);
            Console.WriteLine("end...");
            */
            foreach (ConnectedClient c in clientList)
            {
                //TODO1, WHETHER NEED A THREAD TO RECEIVE ROUND UPDATE, probably a delegate
                //new Thread(() => 
                //if(c.connected)
                if(!disconnectedPlayers.Contains(c.playernumber))
                try
                {
                    c.clientProxy.receiveRoundUpdate(board);
                }
                catch(Exception e)
                {
                    disconnectedPlayers.Add(c.playernumber);
                    Console.WriteLine("Client "+c.nick+" receiving is down...");
                }
                //).Start();
            }
        }
        
        
        public void sendCoinEaten(int playerNumber, string coinName)
        {
            foreach (ConnectedClient c in clientList)
            {
                if(!disconnectedPlayers.Contains(c.playernumber))
                try
                {
                    c.clientProxy.coinEaten(playerNumber, coinName);
                }
                catch
                {
                    disconnectedPlayers.Add(c.playernumber);
                    Console.WriteLine("Exception on server sendCoinEaten()");
                }

            }
        }
        
        
        public void sendStartGame()
        {
            string arg = " ";
            foreach (ConnectedClient c in clientList)
            {
                arg += "-" + c.nick + "_" + c.playernumber + "_" + c.url;
            }
            foreach (ConnectedClient c in clientList)
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

        //RECEIVE CONNECTION FROM SERVER
        public void receiveServer(string name, string url, BoardInfo board)
        {
            Console.WriteLine("ConnectedServer........");
            UpdateBoard(board);
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

        //CONNECT TO SERVER
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
                    Console.WriteLine("ConnectServer........"+name);
                    Console.WriteLine("ConnectServer........" + url);
                    if(!serversConnected.ContainsKey(name))
                        serversConnected.Add(name, serverProxy);
                    serverProxy.receiveServer(Program.SERVERNAME,
                        "tcp://" + Util.GetLocalIPAddress() + ":" + Program.PORT + "/Server",
                        serverForm.boardByRound[serverForm.boardByRound.Count-1]);
                    Console.WriteLine("ConnectedServer........");
                    break;
                }
                catch(Exception e)
                {
                    Console.WriteLine("Exception" + e);
                    Thread.Sleep(10000);
                    serverProxy = null;
                }
            }
            
        }

        public void Freeze()
        {
            //TODO
            freeze = true;
        }

        public void Unfreeze()
        {
            //TODO
            freeze = false;
        }

        public void InjectDelay(string pid2)
        {
            //TODO
            delay = true;
        }

        public void newServerCreated(string serverName, string serverURL)
        {
            Console.WriteLine("Enter new server: "+serverName);
            new Thread(() => connectServer(serverName, serverURL)).Start();
        }
        

        public void UpdateBoard(BoardInfo board)
        {
            Console.WriteLine("round: " + board.RoundID);
            Console.WriteLine("player: " + board.Players);
            Console.WriteLine("monster: " + board.Monsters);
            Console.WriteLine("coin: " + board.Coins);
            serverForm.UpdateBoard(board);
        }


        public BoardInfo getLocalState(int roundID)
        {
            return serverForm.getLocalState(roundID);
        }

        //debug function
        public void CheckUserScore()
        {
            foreach(ConnectedClient c in clientList)
            {
                Console.WriteLine("client: "+c.nick+"=> score="+c.score);
            }
        }
        
    }
}
