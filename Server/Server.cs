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



        public void connect(string nick, string url)
        {
            
            Client c = new Client();
            IClient clientProxy = (IClient)Activator.GetObject(
                typeof(IClient),
                url
            );

            Console.WriteLine("Connected client with url = " + url + " ; with nick = " + nick);

            numberPlayersConnected++;

            c.nick = nick;
            c.url = url;
            c.clientProxy = clientProxy;
            c.playernumber = numberPlayersConnected;

            clientList.Add(c);
            Console.WriteLine("Connected client " + c.nick + "player:" + c.playernumber);

            //Thread
            Console.WriteLine("broadcast to:" + c.nick + " => " + url);
            ClientChat clientChat = new ClientChat();
            clientChat.nick = c.nick;
            clientChat.url = c.url;
            String dir = @"..\..\..\ComLibrary\bin\";
            String clientInfo = dir + "player" + c.playernumber + ".bin";

            Stream stream = new FileStream(clientInfo,
                     FileMode.Create,
                     FileAccess.Write, FileShare.None
                     );
            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                formatter.Serialize(stream, clientChat);
            }
            catch (SerializationException es)
            {
                Console.WriteLine("Failed to serialize. Reason: " + es.Message);
            }
            finally
            {
                stream.Close();
            }
            foreach (Client client in clientList)
            {
                
                Console.WriteLine("Client : " + nick+" Client to sent:" + client.nick);
                
                try
                {
                    Console.WriteLine("Entrei na if: " + client.nick + " : " + clientInfo);
                    Thread thread = new Thread(() => client.clientProxy.broadcastClientURL(clientInfo));
                    thread.Start();
                    Console.WriteLine("broadcastClientURL sent: " + clientInfo);
                }
                catch (Exception e)
                {
                    Console.WriteLine("IMPORTANTE Exception: didn't send broadcastClientURL");
                }
                
                if (!File.Exists(clientInfo))
                {
                    Console.WriteLine("IMPORTANTE Exception: File Doesn't exist: "+ clientInfo);
                }
                else
                {
                    Console.WriteLine("IMPORTANTE Exception: File exists: "+ clientInfo);
                }
            }

            //Creates a correspondence Nick - Player Number i.e. John - Player1

            //unnecessary
            assignPlayer(c); 
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
