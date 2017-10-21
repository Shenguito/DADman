using System;
using System.Collections.Generic;
using System.Text;

namespace ComLibrary
{
    interface IServer
    {
        void send(String nick, String msg);
        void connect(String nick, String url);
    }
    interface IClient
    {
        void broadcast(String nick, String msg);
    }
}
