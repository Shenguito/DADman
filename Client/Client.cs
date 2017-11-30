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
    public delegate void delCoin(int playerNumber, string coinName);
    public delegate void delImageVisible(int playerNumber);
    public delegate void delDebug(string msg);
 




    class Client
    {

       
        public Client()
        {

           
             createClientForm();
           


        }

        public void createClientForm()
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

        
        
        public RemoteClient()
        {
            msgLog = new Dictionary<string, List<int>>();
     
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

        public void movePlayer(int playernumber, string move)
        {
            Console.WriteLine(nick + " received info that player " + playernumber + " moved " + move);
            this.form.Invoke(new delmove(form.updateMove), new object[] { playernumber, move });
        }

        public void moveGhost(List<int> ghostMove)
        {

            this.form.Invoke(new delmoveGhost(form.updateGhostsMove), new object[] { ghostMove[0], ghostMove[1], ghostMove[2], ghostMove[3] });
        }

        //TODO CHAT, every connected client receive the message, and then decide which message show broadcast
        public void send(string nick, string msg)
        {
            Console.WriteLine("Client sending: "+nick+":"+msg);
            clientMessageId++;
            
            foreach (ConnectedClient connectedClient in form.clients)
            {
                Console.WriteLine("Delivering to client: " + connectedClient.nick);
                if (!connectedClient.nick.Equals(nick))
                {
                    try
                    {
                        Console.WriteLine("[IF] Delivering to client: " + connectedClient.nick);
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
        public void coinEaten(int playerNumber, string coinName)
        {
            this.form.Invoke(new delCoin(form.updateCoin), new object[] { playerNumber, coinName });
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
                
                    this.form.debugFunction("wsafsdagf");
                
               
                for (int i = 1; i < rawClient.Length; i++)
                {
                    try
                    {
                        string[] c = rawClient[1].Split(':');


                        if (this.nick.Equals(c[0]))
                        {
                            form.myNumber = Int32.Parse(c[1]);
                        }


                        RemoteClient rmc = new RemoteClient();
                        string clientServiceName = "Client";

                        RemotingServices.Marshal(
                            rmc,
                            clientServiceName,
                            typeof(RemoteClient)
                        );

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
            }
            //form.Invoke(new delImageVisible(form.startGame), new object[] { numberPlayersConnected });
        }
    }
}
