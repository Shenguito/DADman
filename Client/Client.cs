using ComLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Client
{

    public delegate void deluc(string nick, string msg);
    public delegate void delmove(int playernumber, string move);
    public delegate void delmoveGhost(int g1, int g2, int g3x, int g3y);
    public delegate void delDead(int playerNumber);
    public delegate void delCoin(string pictureBoxName);
    public delegate void delImageVisible(int playerNumber);
    public delegate void delDebug(string msg);
    public delegate void delLider(ConnectedClient nick);


    class Client
    {

        public static string DIRECTORY = Util.PROJECT_ROOT + "Client" + Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar + Program.PLAYERNAME;
        public Client()
        {
            if (Directory.Exists(DIRECTORY))
            {
                Directory.Delete(DIRECTORY, true);
            }

            Directory.CreateDirectory(DIRECTORY);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ClientForm());
        }

    }

    public class ConnectedClient
    {
        public string nick;
        public int playernumber;
        public string url;
        public IClient clientProxy;
        public bool connected;
        public ConnectedClient(string nick, int playernumber, string url, IClient clientProxy)
        {
            this.nick = nick;
            this.playernumber = playernumber;
            this.url = url;
            this.clientProxy = clientProxy;
            connected = true;
        }
    }


    public class RemoteClient : MarshalByRefObject, IClient, IGeneralControlServices
    {
        //CREATE DELEGATE
        private delegate void GetCustomerByIdDelegate(int intCustId);

        public static int clientMessageId = 1;
        string nick;
        public int totalMessageId = 0;
        ConnectedClient lider;
        Dictionary<int, string> nickLog;
        Dictionary<int, string> msgLog;
        Dictionary<int, string> msgQueue;
        bool activeThread = false;

        public ClientForm form;

        bool freeze = false;
        bool delay = false;
        Dictionary<int, string> updateLog;

        private delegate void ReceiveDelegate(string param1, int param2, ArrayList list);
        public RemoteClient(ClientForm form)
        {
            
            updateLog = new Dictionary<int, string>();
            msgLog = new Dictionary<int, string>();
            nickLog = new Dictionary<int, string>();
            msgQueue = new Dictionary<int, string>();
            this.form = form;
            nick = Program.PLAYERNAME;
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(RemoteClient),
                "Client",
                WellKnownObjectMode.Singleton
            );
        }

        public void setForm(ClientForm f)
        {
            form = f;
        }

        //TODO CHAT, shows up the message to chat according to fault tolerance
        public void broadcast(int id, string nick, string msg)
        {
            if (!(msgLog.ContainsKey(id)))
            {
                msgLog.Add(id, msg);
                nickLog.Add(id, nick);
                if (id == clientMessageId)
                {
                    clientMessageId++;
                    this.form.Invoke(new deluc(form.updateChat), new object[] { nick, msg });
                }
                else
                {
                    msgQueue.Add(id, msg);
                    Action act = () =>
                    {
                        searchLogs(id);

                    };
                    Thread thread2 = new Thread((new ThreadStart(act)));
                    thread2.Start();
                    if (!activeThread)
                    {
                        activeThread = true;
                        Thread thread = new Thread(() => chatThread());
                        thread.Start();
                    }
                }
            }

        }

        public void movePlayer(int roundID, string players_arg, string dead_arg)
        {
            if (players_arg != "")
            {
                string[] tok_moves = players_arg.Split('-');
                for (int i = 1; i < tok_moves.Length; i++)
                {
                    this.form.Invoke(new delmove(form.updateMove), new object[] { Int32.Parse(tok_moves[i].Split(':')[0]), tok_moves[i].Split(':')[1] });
                }
            }
            if (dead_arg != "")
            {
                string[] tok_dead = dead_arg.Split('-');
                for (int i = 1; i < tok_dead.Length; i++)
                {
                    this.form.Invoke(new delDead(form.updateDead), new object[] { Int32.Parse(tok_dead[i]) });
                }
            }

        }
        public void moveGhost(int roundID, string monster_arg)
        {
            string[] monst_tok = monster_arg.Split(':');
            form.Invoke(new delmoveGhost(form.updateGhostsMove), new object[] { Int32.Parse(monst_tok[0]), Int32.Parse(monst_tok[1]), Int32.Parse(monst_tok[2]), Int32.Parse(monst_tok[3]) });
        }

        public void coinEaten(int roundID, string coins_arg)
        {
            string[] coin_tok = coins_arg.Split('-');

            for (int i = 1; i < coin_tok.Length; i++)
            {
                form.Invoke(new delCoin(form.updateCoin), new object[] { coin_tok[i] });
            }
        }

        //TODO CHAT, every connected client receive the message, and then decide which message show broadcast
        public void send(string nick, string msg, int mId)
        {

            foreach (ConnectedClient connectedClient in form.clients)
            {
                if (!connectedClient.nick.Equals(nick) && connectedClient.connected)
                {
                    try
                    {
                        connectedClient.clientProxy.broadcast(mId, nick, msg);
                    }
                    catch (SocketException e)
                    {
                        //Client Disconnected
                        connectedClient.connected = false;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Debug: " + e.ToString());
                    }
                }
                else
                {
                    Thread thread = new Thread(() => broadcast(mId, nick, msg));
                    thread.Start();
                }
            }
        }

        public void playerDead(int playerNumber)
        {
            this.form.Invoke(new delDead(form.updateDead), new object[] { playerNumber });
        }

        public void startGame(int numberPlayersConnected, string arg)
        {
            // Remember: in the server we sent arg as:
            // "-" +c.nick+":"+ c.playernumber + ":" + c.url
            // in for nick=c[0] playernumber=c[1] url=c[2]
            if (arg != null&&!form.started)
            {

                string[] rawClient = arg.Split('-');

                this.form.debugFunction("debug:\r\n" + arg);

                for (int i = 1; i < rawClient.Length; i++)
                {
                    try
                    {
                        string[] c = rawClient[i].Split('_');


                        if (this.nick.Equals(c[0]))
                        {
                            form.myNumber = Int32.Parse(c[1]);
                        }

                        /*
                        RemoteClient rmc = new RemoteClient(form);
                        string clientServiceName = "Client";

                        RemotingServices.Marshal(
                            rmc,
                            clientServiceName,
                            typeof(RemoteClient)
                        );
                        */
                        IClient clientProxy = (IClient)Activator.GetObject(
                        typeof(IClient),
                        c[2]);

                        form.clients.Add(new ConnectedClient(c[0], Int32.Parse(c[1]), c[2], clientProxy));

                    }
                    catch
                    {

                    }
                }
                form.Invoke(new delImageVisible(form.startGame), new object[] { numberPlayersConnected });
            }
        }

        public void receiveRoundUpdate(int roundID, string players_arg, string dead_arg, string monster_arg, string coins_arg)
        {
            /*TODO1, thread
             * if freeze, add to queue 
             * if !freeze && updateLog.Count != 0 && !updateLog.ContainsKey(roundID) wait
             * if !freeze run delegate function
             */
            while (!freeze && updateLog.Count != 0 && !updateLog.ContainsKey(roundID))
            {
                form.debugFunction("\r\n Let's Sleep");
                Thread.Sleep(1);
                form.debugFunction("\r\nSleeping");
                if (!freeze && updateLog.Count == 0)
                {
                    break;
                }
            }
            if (!freeze)
            {
                //if not null is inside of below function
                movePlayer(roundID, players_arg, dead_arg);
                if (monster_arg != "")
                    moveGhost(roundID, monster_arg);
                if (coins_arg != "")
                    coinEaten(roundID, coins_arg);
            }
            else if (freeze)
            {
                updateLog.Add(roundID, players_arg + " " + dead_arg + " " + monster_arg + " " + coins_arg);
                return;
            }

            //TODO roundID received by server
            form.writeToFile(roundID);

        }

        public void chatThread()
        {
            //tempo de espera maximo por uma mensagem
            int waitTime = 3000;
            while (msgQueue.Count != 0)
            {
                int max = 1;
                foreach (KeyValuePair<int, string> entry in msgQueue)
                {
                    max = Math.Max(max, entry.Key);
                }
                Thread.Sleep(waitTime);
                int i = 1;
                while (i <= max)
                {
                    //TODO
                    if (!msgLog.ContainsKey(i))
                    {
                        msgLog.Add(i, "");
                        clientMessageId++;
                    }
                    else if (msgQueue.ContainsKey(i))
                    {
                        msgQueue.Remove(i);
                        clientMessageId++;
                        this.form.Invoke(new deluc(form.updateChat), new object[] { nickLog[i], msgLog[i] });
                    }
                    i++;
                }
            }
            activeThread = false;
        }
        public int getId()
        {
            totalMessageId++;
            return totalMessageId;
        }
        public void sendLider(int next)
        {

            foreach (ConnectedClient connectedClient in form.clients)
            {
                if (connectedClient.playernumber == next)
                {
                    if (connectedClient.nick.Equals(this.nick))
                    {
                        int i = topMessageId();
                        i = Math.Max(clientMessageId, i);
                        i -= 1;
                        this.totalMessageId = i;
                        Action act = () =>
                        {
                            searchLogs(i);
                        };
                        Thread thread = new Thread((new ThreadStart(act)));
                        thread.Start();
                    }
                    this.lider = connectedClient;
                    this.form.Invoke(new delLider(form.updateLider), new object[] { connectedClient });

                }
            }
        }
        public void askMessage(string nick, int id)
        {
            if (msgLog.ContainsKey(id))
            {
                string msg = msgLog[id];
                string nick1 = nickLog[id];
                foreach (ConnectedClient connectedClient in form.clients)
                {

                    if (connectedClient.nick.Equals(nick))
                    {
                        try
                        {
                            connectedClient.clientProxy.broadcast(id, nick1, msg);
                        }
                        catch (SocketException exception)
                        {
                            //Client Disconnected
                            connectedClient.connected = false;
                            Console.WriteLine("Debug: " + exception.ToString());


                        }
                    }
                }
            }

        }
        public void searchLogs(int current)
        {
            int i = 1;
            while (i < current)
            {
                if (!msgLog.ContainsKey(i))
                {
                    foreach (ConnectedClient connectedClient in form.clients)
                    {

                        if (!connectedClient.nick.Equals(this.nick) && connectedClient.connected)
                        {
                            try
                            {
                                connectedClient.clientProxy.askMessage(nick, i);
                            }
                            catch (SocketException e)
                            {
                                //Client Disconnected
                                connectedClient.connected = false;
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Debug: " + e.ToString());
                            }

                        }

                    }
                }
                i++;
            }
        }
        public int topMessageId()
        {
            int temp = 1;
            foreach (ConnectedClient connectedClient in form.clients)
            {
                try
                {
                    if (!connectedClient.nick.Equals(this.nick) && connectedClient.connected)
                    {
                        int temp2 = connectedClient.clientProxy.getClientMessageId();
                        if (temp2 > temp)
                            temp = temp2;
                    }
                }
                catch (SocketException exception)
                {
                    //Client Disconnected
                    connectedClient.connected = false;
                    Console.WriteLine("Debug: " + exception.ToString());
                }

            }
            return temp;
        }
        public int getClientMessageId()
        {
            return clientMessageId;
        }
        public void Freeze()
        {
            freeze = true;
            form.freeze = true;
            form.debugFunction("\r\nFreezed");
        }
        public void Unfreeze()
        {
            form.debugFunction("\r\nUnfreezed");
            freeze = false;
            form.freeze = false;
            foreach (KeyValuePair<int, string> entry in updateLog)
            {
                receiveRoundUpdate(entry.Key, entry.Value.Split(' ')[0],
                    entry.Value.Split(' ')[1], entry.Value.Split(' ')[2],
                    entry.Value.Split(' ')[3]);
            }
            updateLog = new Dictionary<int, string>();
        }
        public void InjectDelay(string pid1, string pid2)
        {
            delay = true;
            form.debugFunction("\r\nInjected Delay from " + pid1 + " to " + pid2);
        }
        public void newServerCreated(string servername, string serverURL)
        {
            new Thread(() => this.form.connectToServer(servername, serverURL)).Start();
        }
    }
}