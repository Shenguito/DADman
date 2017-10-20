using ComLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;

namespace pacman
{

    public delegate void deluc(String nick, String msg);
    class Client : MarshalByRefObject, IClient
    {

        TcpChannel channel;
        IServer server;
        String nick;
        int port;

        ClientForm form;

        public Client(string nick, ClientForm form)
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
