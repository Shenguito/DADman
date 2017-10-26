using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComLibrary
{
    public interface IServer
    {
        //void send(String nick, String msg);
        void connect(String nick, String url);
        void sendMove(String nick, String direction);
    }
    public interface IClient
    {
        void receiveClient(ClientChat clientChat);
        void broadcastClientURL(ClientChat clientChat);
        void send(String nick, String msg);
        void broadcast(String nick, String msg);
        void movePlayer(int numberPlayer, string movement);
    }
    [Serializable]
    public class ClientChat {
        public string nick;
        public string url;
        public IClient clientProxy;
        
        public ClientChat(String nick, string url, IClient clientProxy)
        {
            this.nick = nick;
            this.url = url;
            this.clientProxy = clientProxy;
        }

        
    }
}
