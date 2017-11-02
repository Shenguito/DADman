using ComLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Xml.Serialization;

namespace pacman
{

    public delegate void deluc(string nick, string msg);
    public delegate void delmove(int playernumber, string move);

    class RemoteClient : MarshalByRefObject, IClient
    {

        Dictionary<string, List<int>> msgLog;
        int clientMessageId=0;
        String nick;

        ClientForm form;
        


        public RemoteClient(string nick, ClientForm form)
        {
            msgLog = new Dictionary<string, List<int>>();
            this.nick = nick;
            this.form = form;
            //TODO
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(RemoteClient),
                "ChatClient",
                WellKnownObjectMode.Singleton
            );

        }

        public void broadcast(int id, string nick, string msg)
        {
            //TODO
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
        
        //Receiving new members
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void broadcastClientURL(int playerNumber, string nick, int port)
        {
            
            try
            {
                foreach (KeyValuePair<string, IClient> entry in form.clients)
                {
                    if (nick.Equals(entry.Key))
                    {
                        throw new Exception("Client already exists!");
                    }
                }
                string url = "tcp://localhost:" + port + "/ChatClient";
                IClient clientProxy = (IClient)Activator.GetObject(
                typeof(IClient),
                url);
                /*
                // Registro do cliente
                RemoteClient rmc = new RemoteClient(nick, form);
                String clientServiceName = "ChatClient";

                // ## dont know what this does
                RemotingServices.Marshal(
                    rmc,
                    clientServiceName,
                    typeof(RemoteClient)
                );
                */

                Console.WriteLine("dictionary added: " + nick);

                form.clients.Add(nick, clientProxy);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception Started:\r\n "+e.ToString()+"\r\nException ending");
                //throw new ClientNotFoundException(e.Message);
            }
            
        }

        public void movePlayer(int playernumber, string move)
        {
            //TODO function crash
            Console.WriteLine(nick + " received info that player " + playernumber + " moved " + move);
            //this.form.updateMove(playernumber, move);
            //this.form.Invoke(new deluc(form.updateChat), new object[] { "sheng", move });
            this.form.Invoke(new delmove(form.updateMove), new object[] { playernumber, move });

        }

        public void send(string nick, string msg)
        {
            // alternativa é lançar uma thread
            Console.WriteLine("Client sending: "+nick+":"+msg);
            clientMessageId++;
            foreach (KeyValuePair<string, IClient> entry in form.clients)
            {
                Console.WriteLine("Delivering to client: " + entry.Key);
                if (!entry.Key.Equals(nick))
                {
                    try
                    {
                        Console.WriteLine("[IF] Delivering to client: " + entry.Key);
                        entry.Value.broadcast(clientMessageId, nick, msg);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception on server send");
                    }
                }
                else
                {
                    Thread thread = new Thread(() => broadcast(clientMessageId, nick, msg));
                    thread.Start();
                }
            }
        }
    }
}
