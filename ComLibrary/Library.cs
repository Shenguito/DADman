using System;

namespace ComLibrary
{
    public interface IServer
    {
        void send(String nick, String msg);
        void connect(String nick, String url);
    }
    public interface IClient
    {
        void broadcast(String nick, String msg);
    }
}
