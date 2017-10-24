using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComLibrary
{
    public interface IServer
    {
        void send(String nick, String msg);
        void connect(String nick, String url);
        void sendMove(String nick, String direction);
    }
    public interface IClient
    {
        void broadcast(String nick, String msg);
        void movePlayer(int numberPlayer, string movement);
    }
}
