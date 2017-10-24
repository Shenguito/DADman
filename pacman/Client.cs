using ComLibrary;
using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Channels.Tcp;

namespace pacman
{

    public delegate void deluc(String nick, String msg);
    public delegate void delmove(int playernumber, string move);

    class Client
    {
        TcpChannel channel;
        IServer serverProxy;
        String nick;
        int port;
        
    }


    class RemoteClient : MarshalByRefObject, IClient
    {

        TcpChannel channel;
        IServer server;
        String nick;

        ClientForm form;

        public RemoteClient(string nick, ClientForm form)
        {
            this.nick = nick;
            this.form = form;
            
        }

        public void broadcast(string nick, string msg)
        {
            this.form.Invoke(new deluc(form.updateChat), new object[] { nick, msg });
        }

        public void movePlayer(int playernumber, string move)
        {
            //TODO function crash
            //this.form.updateMove(playernumber, move);
            //this.form.Invoke(new deluc(form.updateChat), new object[] { nick, move });
            //this.form.Invoke(new delmove(form.updateMove), new object[] { playernumber, move });
        }

        
    }
}
