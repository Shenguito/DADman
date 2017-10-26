using ComLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Channels.Tcp;

namespace pacman
{

    public delegate void deluc(string nick, string msg);
    public delegate void delmove(int playernumber, string move);

    class Client
    {
        TcpChannel channel;
        IServer serverProxy;
        String nick;
        int port;
        
    }


    class RemoteClient : MarshalByRefObject, IClient
    {

        TcpChannel channel;
        IServer server;
        String nick;

        ClientForm form;

        ArrayList clientList = new ArrayList();


        public RemoteClient(string nick, ClientForm form)
        {
            this.nick = nick;
            this.form = form;
            
        }

        public void broadcast(string nick, string msg)
        {
            this.form.Invoke(new deluc(form.updateChat), new object[] { nick, msg });
            //being here does not work too
            //this.form.Invoke(new delmove(form.updateMove), new object[] { 1, msg });
        }

        //not used yet
        public void receiveClient(ClientChat cc)
        {
            //clientList.Add(cc);
        }

        public void broadcastClientURL(ClientChat clientChat)
        {
            Console.WriteLine("client add: " + clientChat.nick);
            form.clients.Add(clientChat);
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
            foreach (ClientChat c in clientList)
            {
                Console.WriteLine("Delivering to client: " + c.nick);
                if (!c.nick.Equals(nick))
                {
                    try
                    {
                        c.clientProxy.broadcast(nick, msg);
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
