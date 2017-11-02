using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComLibrary
{
    public interface IServer
    {
        void connect(string nick, int port);
        void sendMove(string nick, string direction);
    }
    public interface IClient
    {
        void broadcastClientURL(int playerNumber, string nick, int port);
        void send(string nick, string msg);
        void broadcast(int id, string nick, string msg);
        void movePlayer(int numberPlayer, string movement);
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
