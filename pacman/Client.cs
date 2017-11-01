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
        ClientChat ownClient = new ClientChat();

        ClientForm form;
        


        public RemoteClient(string nick, ClientForm form)
        {
            msgLog = new Dictionary<string, List<int>>();
            ownClient.nick = nick;
            ownClient.url = "tcp://localhost:" + form.port + "/ChatClient";
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

        //I'm new member, and I'm receiving from old members
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void receiveClient(ClientChat cc)
        {
            foreach (KeyValuePair<ClientChat, IClient> entry in form.clients)
            {
                if (cc.nick.Equals(entry.Key.nick))
                {
                    return;
                }
            }
            IClient clientProxy = (IClient)Activator.GetObject(
                    typeof(IClient),
                    cc.url);

            // Registro do cliente
            RemoteClient rmc = new RemoteClient(cc.nick, form);
            String clientServiceName = "ChatClient";

            // ## dont know what this does
            RemotingServices.Marshal(
                rmc,
                clientServiceName,
                typeof(RemoteClient)
            );
            Console.WriteLine("[IF] dictionary added from newclient: " + cc.nick);
            form.clients.Add(cc, clientProxy);
        }

        //Receiving new members
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void broadcastClientURL(String clientChat)
        {
            
            if (!File.Exists(clientChat))
            {
                Console.WriteLine("client didn't be found" + clientChat);
            }
            
            try
            {
                FileStream stream = new FileStream(clientChat,
                          FileMode.Open,
                          FileAccess.Read,
                          FileShare.Read
                          );
                IFormatter formatter = new BinaryFormatter();
                try
                {
                    ClientChat client = (ClientChat)formatter.Deserialize(stream);
                    foreach (KeyValuePair<ClientChat, IClient> entry in form.clients)
                    {
                        if (client.nick.Equals(entry.Key.nick))
                        {
                            throw new Exception("Client already exists!");
                        }
                    }

                    IClient clientProxy = (IClient)Activator.GetObject(
                    typeof(IClient),
                    client.url);

                    // Registro do cliente
                    RemoteClient rmc = new RemoteClient(client.nick, form);
                    String clientServiceName = "ChatClient";

                    // ## dont know what this does
                    RemotingServices.Marshal(
                        rmc,
                        clientServiceName,
                        typeof(RemoteClient)
                    );
                    Console.WriteLine("dictionary added: " + client.nick);
                    if (!client.Equals(ownClient))
                        form.clients.Add(client, clientProxy);
                        
                    clientProxy.receiveClient(ownClient);
                }
                catch (SerializationException es)
                {
                    Console.WriteLine("Failed to deserialize.Reason: " + es.Message);
                }
                finally{ stream.Close(); }
                
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
            foreach (KeyValuePair<ClientChat, IClient> entry in form.clients)
            {
                Console.WriteLine("Delivering to client: " + entry.Key.nick);
                if (!entry.Key.nick.Equals(nick))
                {
                    try
                    {
                        Console.WriteLine("[IF] Delivering to client: " + entry.Key.nick);
                        entry.Value.broadcast(clientMessageId, nick, msg);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception on server send");
                    }
                }
            }
        }
    }
}
