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
            try
            {
                IClient clientProxy = (IClient)Activator.GetObject(
                typeof(IClient),
                url
                );

                numberPlayersConnected++;
                ConnectedClient c = new ConnectedClient(nick, numberPlayersConnected, url, clientProxy);

                clientList.Add(c);
            }
            catch (Exception e) { Console.WriteLine(e);
            }
            if (numberPlayersConnected == Program.PLAYERNUMBER&&Program.FIRSTSERVER)
            {
                Thread thread = new Thread(() =>  sendStartGame());
                thread.Start();
            }

        }

        public void sendMove(Movement move)
        {
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
            foreach (ConnectedClient c in clientList)
            {
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
                    if(!serversConnected.ContainsKey(name))
                        serversConnected.Add(name, serverProxy);
                    serverProxy.receiveServer(Program.SERVERNAME,
                        "tcp://" + Util.GetLocalIPAddress() + ":" + Program.PORT + "/Server",
                        serverForm.boardByRound[serverForm.boardByRound.Count-1]);
                    break;
                }
                catch(Exception e)
                {
                    Console.WriteLine("Exception" + e);
                    Thread.Sleep(200);
                    serverProxy = null;
                }
            }
            
        }

        public void Freeze()
        {
            serverForm.freeze = true;
        }

        public void Unfreeze()
        {
            serverForm.freeze = false;
        }

        public void InjectDelay(string pid2)
        {
            serverForm.delay = true;
        }

        public void newServerCreated(string serverName, string serverURL)
        {
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
        
        public void CheckUserScore()
        {
            foreach(ConnectedClient c in clientList)
            {
                Console.WriteLine("client: "+c.nick+"=> score="+c.score);
            }
        }
        
    }
}
