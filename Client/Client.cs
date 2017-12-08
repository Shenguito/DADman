﻿using ComLibrary;
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

    class MessageInfo
    {
        public string nick;
        public string msg;
        public int id;
        public bool isSend;
    }
    public class RemoteClient : MarshalByRefObject, IClient, IGeneralControlServices
    {
        //CREATE DELEGATE
        private delegate void GetCustomerByIdDelegate(int intCustId);

        public static int clientMessageId = 1;
        string nick;
        public int totalMessageId = 0;
        public int holderCount = 0;
        ConnectedClient lider;
        Dictionary<int, string> nickLog;
        Dictionary<int, string> msgLog;
        Dictionary<int, string> msgQueue;
        Dictionary<string, int> delayLog;

        bool activeThread = false;

        public ClientForm form;

        bool freeze = false;
        Dictionary<int, BoardInfo> temporaryBoard;
        Dictionary<int, MessageInfo> messageHolder;

        private delegate void ReceiveDelegate(string param1, int param2, ArrayList list);
        public RemoteClient(ClientForm form)
        {
            
            temporaryBoard = new Dictionary<int, BoardInfo>();
            messageHolder = new Dictionary<int, MessageInfo>();
            msgLog = new Dictionary<int, string>();
            nickLog = new Dictionary<int, string>();
            msgQueue = new Dictionary<int, string>();
            delayLog = new Dictionary<string, int>();
            this.form = form;
            nick = Program.PLAYERNAME;
            /*
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(RemoteClient),
                "Client",
                WellKnownObjectMode.Singleton
            );
            */
        }

        public void setForm(ClientForm f)
        {
            form = f;
        }

        //TODO CHAT, shows up the message to chat according to fault tolerance
        public void broadcast(int id, string nick, string msg)
        {
            if (!freeze)
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
            else
            {
                MessageInfo mInf = new MessageInfo();
                mInf.nick = nick;
                mInf.id = id;
                mInf.msg = msg;
                mInf.isSend = false;
                holderCount++;
                messageHolder.Add(holderCount, mInf);
            }

        }

        public void movePlayer(int roundID, string players_arg)
        {
            
            string[] tok_moves = players_arg.Split('-');
            for (int i = 1; i < tok_moves.Length; i++)
            {
                if(!tok_moves[i].Split(':')[1].Equals("null"))
                this.form.Invoke(new delmove(form.updateMove), new object[] { Int32.Parse(tok_moves[i].Split(':')[0]), tok_moves[i].Split(':')[1] });
            }
            /*
            if (dead_arg != "")
            {
                string[] tok_dead = dead_arg.Split('-');
                for (int i = 1; i < tok_dead.Length; i++)
                {
                    this.form.Invoke(new delDead(form.updateDead), new object[] { Int32.Parse(tok_dead[i]) });
                }
            }
            */

        }
        public void moveGhost(int roundID, string monster_arg)
        {
            string[] monst_tok = monster_arg.Split(':');
            form.Invoke(new delmoveGhost(form.updateGhostsMove), new object[] { Int32.Parse(monst_tok[0]), Int32.Parse(monst_tok[2]), Int32.Parse(monst_tok[4]), Int32.Parse(monst_tok[5]) });
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
            if (!freeze)
            {
                foreach (ConnectedClient connectedClient in form.clients)
                {
                    if (delayLog.ContainsKey(connectedClient.nick) && connectedClient.connected)
                    {
                        Thread thread = new Thread(() => delayThread(nick, msg, mId, connectedClient));
                        thread.Start();
                    }
                    else if (!connectedClient.nick.Equals(nick) && connectedClient.connected)
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
            else
            {
                MessageInfo mInf = new MessageInfo();
                mInf.nick = nick;
                mInf.id = mId;
                mInf.msg = msg;
                mInf.isSend = true;
                holderCount++;
                messageHolder.Add(holderCount, mInf);
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

        public void receiveRoundUpdate(BoardInfo board)
        {
            /*TODO1, thread
             * if freeze, add to queue 
             * if !freeze && updateLog.Count != 0 && !updateLog.ContainsKey(roundID) wait
             * if !freeze run delegate function
             */
            if (form.boardByRound.ContainsValue(board))
                return;
            while (!freeze && temporaryBoard.Count != 0 && !temporaryBoard.ContainsKey(board.RoundID))
            {
                form.debugFunction("\r\n Let's Sleep");
                Thread.Sleep(1);
                form.debugFunction("\r\nSleeping");
                if (!freeze && temporaryBoard.Count == 0)
                {
                    break;
                }
            }
            if (!freeze)
            {
                //if not null is inside of below function
                //PROBLEM PLAYER MOVE DELEGATE
                if (board.Players.Split('_')[1] != "")
                    movePlayer(board.RoundID, board.Players.Split('_')[1]);
                if (board.Monsters != "")
                    moveGhost(board.RoundID, board.Monsters);
                if (board.Coins != "")
                    coinEaten(board.RoundID, board.Coins);
                form.roundID = board.RoundID+1;
                form.boardByRound.Add(board.RoundID, board);
            }
            else if (freeze)
            {
                temporaryBoard.Add(board.RoundID, board);
                return;
            }

            //TODO roundID received by server
            //form.writeToFile(roundID);

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
            foreach (KeyValuePair<int, BoardInfo> entry in temporaryBoard)
            {
                receiveRoundUpdate(entry.Value);
            }
            temporaryBoard = new Dictionary<int, BoardInfo>();
            foreach (KeyValuePair<int, MessageInfo> entry in messageHolder)
            {
                if(entry.Value.isSend)
                    send(entry.Value.nick, entry.Value.msg, entry.Value.id);
                else
                    broadcast(entry.Value.id, entry.Value.nick, entry.Value.msg);
            }
            messageHolder = new Dictionary<int, MessageInfo>();
        }
        public void InjectDelay(string pid1, string pid2)
        {
            int delay = 5000;
            if (!delayLog.ContainsKey(pid2))
            {
                delayLog.Add(pid2, delay);
            }
            else
            {
                delayLog[pid2] += delay;
            }

            this.form.injectDelay(pid2);
            form.debugFunction("\r\nInjected Delay to " + pid2);
        }

        public void delayThread(string nickname, string msg, int mId, ConnectedClient conn)
        {
            Thread.Sleep(delayLog[conn.nick]);
            try
            {
                conn.clientProxy.broadcast(mId, nickname, msg);
            }
            catch (SocketException exception)
            {
                //Client Disconnected
                conn.connected = false;
                Console.WriteLine("Debug: " + exception.ToString());

            }
        }

        public BoardInfo getLocalState(int roundID)
        {
            return form.getLocalState(roundID);
        }
        public void newServerCreated(string servername, string serverURL)
        {
            new Thread(() => this.form.connectToServer(servername, serverURL)).Start();
        }
    }
}