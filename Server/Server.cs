using ComLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    //client in server
    class Client
    {
        public string nick;
        public string url;
        public IClient clientProxy;
    }

    class Server
    {

        private ArrayList playersAlive;
        private ArrayList playersDead;
        private ArrayList coins;
        private ArrayList monsters;
        private ArrayList walls;
        private ArrayList inputs;
        private int MSECROUND = 10; //game speed [communication refresh time]

        public Server()
        {

        }

        const int PORT = 8086;
        static TcpChannel channel = new TcpChannel(PORT);
        
        //not called
        private void createConnection()
        {
            ChannelServices.RegisterChannel(channel, false);
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(RemoteServer),
                "ChatServer",
                WellKnownObjectMode.Singleton
            );
            Console.ReadLine();
        }

        public void run()
        {
            while (true)
            {
                System.Threading.Thread.Sleep(MSECROUND);
                UpdateBoard();
                ResetInputsReceived();
            }
        }

        public void receiveInputs()
        {
            //TODO receber inputs dos clientes e atualizar parametros Inputs
        }

        private void ResetInputsReceived()
        {
            inputs = new ArrayList();
        }

        private void UpdateBoard()
        {
            Move();
            CheckDeadPlayers();
            CheckCoinsRetrieved();
            CheckGameEnd();

        }

        private void CheckGameEnd()
        {
            throw new NotImplementedException();
        }

        private void CheckCoinsRetrieved()
        {
            throw new NotImplementedException();
        }

        private void CheckDeadPlayers()
        {
            throw new NotImplementedException();
        }

        private void Move()
        {
            throw new NotImplementedException();
        }
    }
    class RemoteServer : MarshalByRefObject, IServer
    {
        ArrayList clientList = new ArrayList();
        public void connect(string nick, string url)
        {
            Client c = new Client();
            IClient clientProxy = (IClient)Activator.GetObject(
                typeof(IClient),
                url
            );

            c.nick = nick;
            c.url = url;
            c.clientProxy = clientProxy;

            clientList.Add(c);

            Console.WriteLine("Connected client with url = " + url + " ; with nick = " + nick);

        }

        public void send(string nick, string msg)
        {
            Console.WriteLine("Sending message = " + msg + " ; from nick = " + nick);
            // alternativa é lançar uma thread
            foreach (Client c in clientList)
            {
                Console.WriteLine("Client: " + c.nick);
                if (c.nick != nick)
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
