using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;
using ComLibrary;
using System.Runtime.Remoting.Channels;

namespace pacman
{

    public delegate void deluc(String nick, String msg);

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
    }
}
