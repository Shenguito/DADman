using ComLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Server
{
    //client in server
    class Client
    {
        public string nick;
        public int playernumber;
        public string url;
        public IClient clientProxy;
        public int port;
        // defined at end
        public int score;
        internal int playerNumber;
       
    }

    class Server
    {
        private int MSECROUND = 10; //game speed [communication refresh time]
        const int PORT = 8000;
        static TcpChannel channel = new TcpChannel(PORT);
       

        public Server()
        {
            /*
            Console.WriteLine("Path.PathSeparator={0}", 
                Path.PathSeparator);
            */

            
            
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
        private Dictionary<string, int> player_image_hashmap = new Dictionary<string, int>();
        public int numberPlayersConnected = 0;

        //TODO NUNES THIS IS FOR YOU
        public ServerForm serverForm;
        public RemoteServer()
        {
            Thread thread = new Thread(() => createServerForm());
            thread.Start();
        }


        private void createServerForm()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(serverForm = new ServerForm());
        }

        public void connect(string nick, int port)
        {
            
            Client c = new Client();
            string url = "tcp://localhost:" + port + "/ChatClient";
            IClient clientProxy = (IClient)Activator.GetObject(
                typeof(IClient),
                url
            );

            numberPlayersConnected++;

            c.nick = nick;
            c.url = url;
            c.port = port;
            c.clientProxy = clientProxy;
            c.playernumber = numberPlayersConnected;

            clientList.Add(c);
            
            foreach (Client client in clientList)
            {
                
                Console.WriteLine("Client : " + nick+" Client to sent:" + client.nick);
                if (!client.nick.Equals(nick))
                {
                    client.clientProxy.broadcastClientURL(numberPlayersConnected, nick, port);
                }
                else
                {
                    foreach (Client oldClient in clientList)
                    {
                        Thread thread = new Thread(() => client.clientProxy.broadcastClientURL(oldClient.playernumber, oldClient.nick, oldClient.port));
                        thread.Start();
                    }
                }
            }

            //Creates a correspondence Nick - Player Number i.e. John - Player1
            
            assignPlayer(c); 
        }

        public void sendMove(string nick, string move)
        {
            int playerNumber=0;
            
            foreach(Client c in clientList)
            {
                if (c.nick.Equals(nick))
                    playerNumber = c.playernumber;
            }
            
            Console.WriteLine("player"+ playerNumber + ": " + nick + "receives: " + move);

            int pl_number = player_image_hashmap[nick];
            

            foreach (Client c in clientList)
            {
                try
                {
                    //Console.WriteLine("reach client foreach");
                    c.clientProxy.movePlayer(playerNumber, move);
                    //Console.WriteLine("player:" + c.playernumber + " suposely receives: " + move);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception on server sendMove");
                }
                
            }
        }

       
        private void assignPlayer(Client c)
        {
            player_image_hashmap.Add(c.nick,c.playernumber);

            foreach (KeyValuePair<string, int> entry in player_image_hashmap)
            {
                Console.WriteLine("INFO: " + entry.Key + " is " + entry.Value);
            }

        }
        
    }
}
