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
        void connect(string nick, string url);
        void sendMove(string nick, string direction);
    }
    public interface IClient
    {
        void receiveClient(ClientChat clientChat);
        void broadcastClientURL(string clientChat);
        void send(string nick, string msg);
        void broadcast(int id, string nick, string msg);
        void movePlayer(int numberPlayer, string movement);
    }
    [Serializable]
    public class ClientChat
    {
        public string nick;
        public string url;
        //public IClient clientProxy;
        public ClientChat()
        {

        }
        /*
        public ClientChat(String nick, string url, IClient clientProxy)
        {
            this.nick = nick;
            this.url = url;
            this.clientProxy = clientProxy;
        }*/
    }

    [Serializable]
    public class ClientNotFoundException : ApplicationException
    {

        public ClientNotFoundException() { }

        public ClientNotFoundException(string msg)
            : base(msg)
        {
        }
        public ClientNotFoundException(System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context)
        : base(info, context)
        {
        }
        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
