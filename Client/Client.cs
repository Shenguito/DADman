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

        Dictionary<string, List<int>> msgLog;
        int clientMessageId=0;
        String nick;

        public ClientForm form;

        
        
        public RemoteClient(ClientForm form)
        {
            msgLog = new Dictionary<string, List<int>>();
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
            List<int> lista = new List<int>();
            if (!(msgLog.ContainsKey(nick))) {
                lista.Add(id);
                msgLog.Add(nick, lista);
                this.form.Invoke(new deluc(form.updateChat), new object[] { nick, msg });
            }else
            {
                lista = msgLog[nick];
                if (!lista.Contains(id))
                {
                    lista.Add(id);
                    msgLog[nick] = lista;
                    this.form.Invoke(new deluc(form.updateChat), new object[] { nick, msg });
                }
            }
        }

        public void movePlayer(int roundID, string players_arg, string dead_arg)
        {
            form.debugFunction("player: " + players_arg);
            if (players_arg != "")
            {
                string[] tok_moves = players_arg.Split('-');
                for (int i = 1; i < tok_moves.Length; i++)
                {
                    this.form.Invoke(new delmove(form.updateMove), new object[] { tok_moves[i].Split(':')[0], tok_moves[i].Split(':')[1] });
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
        public void send(string nick, string msg)
        {
            clientMessageId++;
            
            foreach (ConnectedClient connectedClient in form.clients)
            {
                if (!connectedClient.nick.Equals(nick))
                {
                    try
                    {
                        connectedClient.clientProxy.broadcast(clientMessageId, nick, msg);
                    }
                    catch (SocketException e)
                    {
                        //Client Disconnected
                        connectedClient.connected = false;
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine("Debug: "+e.ToString());
                    }
                }
                else
                {
                    Thread thread = new Thread(() => broadcast(clientMessageId, nick, msg));
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

                this.form.debugFunction("debug:\r\n"+arg);

                for (int i = 1; i < rawClient.Length; i++)
                {
                    try
                    {
                        string[] c = rawClient[i].Split(':');


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

                        form.clients.Add(new ConnectedClient(c[0], form.myNumber, c[2], clientProxy));


                        //TODO client move by file
                        if (Program.FILENAME != null)
                        {
                            form.sendMoveByFile(Program.FILENAME);
                        }
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
            
            //if not null is inside of below function
            movePlayer(roundID, players_arg, dead_arg);
            if(monster_arg!="")
                moveGhost(roundID, monster_arg);
            if (coins_arg != "")
                coinEaten(roundID, coins_arg);
        }
    }
}
