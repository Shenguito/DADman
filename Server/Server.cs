using ComLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    //client in server
    class Client
    {
        public string nick;
        public int playernumber;
        public string url;
        public IClient clientProxy;
        // defined at end
        public int score;
    }

    class Server
    {
        private int MSECROUND = 10; //game speed [communication refresh time]
        const int PORT = 8000;
        static TcpChannel channel = new TcpChannel(PORT);
        

        public Server()
        {
            createConnection();
        }

        
        private void createConnection()
        {
            ChannelServices.RegisterChannel(channel, false);
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(RemoteServer),
                "ChatServer",
                WellKnownObjectMode.Singleton
            );
        }

        
    }
    class RemoteServer : MarshalByRefObject, IServer
    {
        ArrayList clientList = new ArrayList();
        //private Dictionary<string, string> player_image_hashmap = new Dictionary<string, string>();
        public int numberPlayersConnected = 0;


        public void connect(string nick, string url)
        {
            
            Client c = new Client();
            IClient clientProxy = (IClient)Activator.GetObject(
                typeof(IClient),
                url
            );

            Console.WriteLine("Connected client with url = " + url + " ; with nick = " + nick);

            numberPlayersConnected++;
            //TODO nickname duplicate problem
            //TODO max players = 6
            c.nick = nick;
            c.url = url;
            c.clientProxy = clientProxy;
            c.playernumber = numberPlayersConnected;

            clientList.Add(c);
            Console.WriteLine("Connected client " + nick+"player:"+numberPlayersConnected);

            //Thread
            foreach (Client client in clientList)
            {
                Console.WriteLine("broadcast to:" + client.nick + " => " + url);
                if (!client.nick.Equals(nick)) {
                    ClientChat clientChat = new ClientChat(nick, url, clientProxy);
                    try
                    {
                        client.clientProxy.broadcastClientURL(clientChat);
                    }catch(Exception e)
                    {
                        Console.WriteLine("Exception: "+e.ToString());
                    }
                }
            }

            //Creates a correspondence Nick - Player Number i.e. John - Player1

            //unnecessary
            //assignPlayer(c); 
        }
        /*
        public void send(string nick, string msg)
        {
            Console.WriteLine("Sending message = " + msg + " ; from nick = " + nick);
            // alternativa é lançar uma thread
            foreach (Client c in clientList)
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
        */
        public void sendMove(string nick, string move)
        {
            Console.WriteLine("player: " + nick + "receives: " + move);

            foreach (Client c in clientList)
            {
                if (!c.nick.Equals(nick))
                {
                    try
                    {
                        //Console.WriteLine("reach client foreach");
                        c.clientProxy.movePlayer(c.playernumber, move);
                        //Console.WriteLine("player:" + c.playernumber + " suposely receives: " + move);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception on server send");
                    }
                }
            }
        }

        private static void sendMove2Player(Client c)
        {

        }

        /*
         * unnecessary
        private void assignPlayer(Client c)
        {
            player_image_hashmap.Add(c.nick, "Player" + numberPlayersConnected);

            foreach (KeyValuePair<string, string> entry in player_image_hashmap)
            {
                Console.WriteLine("INFO: " + entry.Key + " is " + entry.Value);
            }

        }
        */
    }
}
