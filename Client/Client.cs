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


        public Client()
        {
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


    public class RemoteClient : MarshalByRefObject, IClient
    {

        public static int clientMessageId = 1;
        String nick;
        public static int totalMessageId = 1;
        ConnectedClient lider;
        Dictionary<int, string> nickLog;
        Dictionary<int, string> msgLog;

        public ClientForm form;

        //CREATED
        bool freeze = false;
        Dictionary<int, string> updateLog;


        public RemoteClient(ClientForm form)
        {
            //CREATED
            updateLog = new Dictionary<int, string>();

            msgLog = new Dictionary<int, string>();
            nickLog = new Dictionary<int, string> ();
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
        public void broadcast(int id, string nick, string msg, Dictionary<string, int> delayLog)
        {
            if (!(msgLog.ContainsKey(id)))
            {
                nickLog.Add(id, nick);
                msgLog.Add(id, msg);
                if (id <= clientMessageId && !delayLog.ContainsKey(this.nick))
                {
                    clientMessageId++;
                    this.form.Invoke(new deluc(form.updateChat), new object[] { nick, msg });
                }
                else if (delayLog.ContainsKey(this.nick))
                {
                    Thread thread = new Thread(() => chatThread(nick, msg, delayLog));
                    thread.Start();
                }
                else
                {
                    //searchLogs(id);
                    Thread thread = new Thread(() => chatThread(nick, msg, delayLog));
                    thread.Start();
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
            this.form.Invoke(new delmoveGhost(form.updateGhostsMove), new object[] { Int32.Parse(monst_tok[0]), Int32.Parse(monst_tok[1]), Int32.Parse(monst_tok[2]), Int32.Parse(monst_tok[3]) });
        }

        //TODO CHAT, every connected client receive the message, and then decide which message show broadcast
        public void send(string nick, string msg, int mId, Dictionary<string, int> delayLog)
        {

            foreach (ConnectedClient connectedClient in form.clients)
            {
                if (!connectedClient.nick.Equals(nick))
                {
                    try
                    {
                        connectedClient.clientProxy.broadcast(mId, nick, msg, delayLog);
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
                    Thread thread = new Thread(() => broadcast(mId, nick, msg, delayLog));
                    thread.Start();
                }
            }
        }

        public void coinEaten(int roundID, string coins_arg)
        {
            string[] coin_tok = coins_arg.Split('-');

            for (int i = 1; i < coin_tok.Length; i++)
            {
                this.form.Invoke(new delCoin(form.updateCoin), new object[] { coin_tok[i] });
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
            if (arg != null)
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
            //CREATED
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
            }
        }

        public void chatThread(string nick, string msg, Dictionary<string, int> delayLog)
        {
            //tempo de espera maximo por uma mensagem
            int waitTime = 1000;
            if (delayLog.ContainsKey(this.nick))
                waitTime += delayLog[this.nick];

            Thread.Sleep(waitTime);
            clientMessageId++;
            this.form.Invoke(new deluc(form.updateChat), new object[] { nick, msg });
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
                     /*if (connectedClient.nick.Equals(this.nick))
                     {
                         int i = topMessageId();
                         i = Math.Max(clientMessageId, i);
                         searchLogs(i);
                         totalMessageId = i;
                     }*/
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
                        connectedClient.clientProxy.broadcast(id, nick1, msg, new Dictionary<string, int>());



                }
            }

        }
        public void searchLogs(int current)
        {
            int i = 1;
            while ( i < current)
            {
                if (!msgLog.ContainsKey(i))
                {
                    foreach (ConnectedClient connectedClient in form.clients)
                    {

                        if (!connectedClient.nick.Equals(this.nick))
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
            }
        }
        public int topMessageId()
        {
            int temp = 0;
            foreach (ConnectedClient connectedClient in form.clients)
            {
                if (!connectedClient.nick.Equals(this.nick))
                {
                    int temp2 = connectedClient.clientProxy.getClientMessageId();
                    if (temp2 > temp)
                        temp = temp2;
                }

            }
            return temp-1;
        }
        public int getClientMessageId()
        {
            return clientMessageId;
        }
        //CREATED
        public void Freeze()
        {
            freeze = true;
            form.freeze = true;
            form.debugFunction("\r\nFreezed");
        }
        //CREATED
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
    }
}
