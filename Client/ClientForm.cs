using ComLibrary;
using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic.FileIO;

namespace Client
{
    public partial class ClientForm : Form
    {

        public int myNumber = 1;

        string nickname;
        public int port;
        public bool started = false;
        bool dead = false;
        bool sent = false;
        public bool freeze = false;

        // direction player is moving in. Only one will be true
        bool goup;
        bool godown;
        bool goleft;
        bool goright;

        int boardRight = 320;
        int boardBottom = 320;
        int boardLeft = 0;
        int boardTop = 40;

        //player speed
        int speed = 5;
        int score = 0;
        //TODO to define when game is over
        int total_coins = 61;
        Dictionary<string, int> delayLog;
        ConnectedClient lider;

        private TcpChannel channel;

        public List<ConnectedClient> clients;

        public Dictionary<string, IServer> serversConnected = new Dictionary<string, IServer>();


        public ClientForm()
        {
          
            nickname = Program.PLAYERNAME;
            clients = new List<ConnectedClient>();
            delayLog = new Dictionary<string, int>();
            InitializeComponent();
            this.Text += ": " + nickname;
            label2.Visible = false;

            Init();

        }

        public void Init()
        {

            this.nickname = Program.PLAYERNAME;
            this.port = Program.PORT;
            channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, false);



            // Registro do cliente
            RemoteClient rmc = new RemoteClient(this);
            string clientServiceName = "Client";
            // ## dont know what this does
            RemotingServices.Marshal(
                rmc,
                clientServiceName,
                typeof(RemoteClient)
            );
            try
            {
                if (Program.SERVERURL != "")
                {
                    string[] serv = Program.SERVERURL.Split('-');
                    for (int i = 1; i < serv.Length; i++)
                    {
                        connectToServer(serv[i].Split('_')[0], serv[i].Split('_')[1]);
                    }
                }
            }
            catch { tbChat.AppendText("Catched"); }

        }

        public void connectToServer(string servername, string serverURL)
        {
            //try catch missed
            IServer serverProxy = null;
            while (serverProxy == null)
            {
                //TODO, CONNECT TO THE SERVER PROBLEM, WHEN SERVER STARTED BEFORE OR AFTER

                try
                {
                    serverProxy = (IServer)Activator.GetObject(
                    typeof(IServer),
                    serverURL);

                    serverProxy.connectClient(nickname, "tcp://" + Util.GetLocalIPAddress() + ":" + port + "/Client");
                    serversConnected.Add(servername, serverProxy);
                    break;
                }
                catch
                {
                    tbChat.Text += "Catch: Didn't connected to server";
                    Thread.Sleep(1000);
                }
            }
        }

        private void doMove(string move)
        {
            while (!started)
            {
                Thread.Sleep(1);
                if (started)
                    break;
            }
            while (sent || freeze)
            {
                Thread.Sleep(1);
                if (!sent && !freeze)
                    break;
            }
            if (!dead)
            {

                    foreach (KeyValuePair<string, IServer> entry in serversConnected) {
                        if (move.Equals("LEFT"))
                        {
                            entry.Value.sendMove(nickname, "LEFT");
                            sent = true;
                        }
                        else if (move.Equals("RIGHT"))
                        {
                            entry.Value.sendMove(nickname, "RIGHT");
                            sent = true;
                        }
                        else if (move.Equals("UP"))
                        {
                            entry.Value.sendMove(nickname, "UP");
                            sent = true;
                        }
                        else if (move.Equals("DOWN"))
                        {
                            entry.Value.sendMove(nickname, "DOWN");
                            sent = true;
                        }
                }
            }
        }


            //Todo, sending only if he can
            private void keyisdown(object sender, KeyEventArgs e)
            {
                if (!sent && !freeze)
                    if (started)
                    {
                        if (!dead)
                        {
                            foreach (KeyValuePair<string, IServer> entry in serversConnected)
                            {
                                if (e.KeyCode == Keys.Left)
                                {
                                    entry.Value.sendMove(nickname, "LEFT");
                                    sent = true;
                                }
                                if (e.KeyCode == Keys.Right)
                                {
                                    entry.Value.sendMove(nickname, "RIGHT");
                                    sent = true;
                                }
                                if (e.KeyCode == Keys.Up)
                                {
                                    entry.Value.sendMove(nickname, "UP");
                                    sent = true;
                                }
                                if (e.KeyCode == Keys.Down)
                                {
                                    entry.Value.sendMove(nickname, "DOWN");
                                    sent = true;
                                }
                            }
                        }
                    }
                if (e.KeyCode == Keys.Enter)
                {
                    tbMsg.Enabled = true; tbMsg.Focus();
                }
            }

            private void keyisup(object sender, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Left)
                {
                    goleft = false;
                }
                if (e.KeyCode == Keys.Right)
                {
                    goright = false;
                }
                if (e.KeyCode == Keys.Up)
                {
                    goup = false;
                }
                if (e.KeyCode == Keys.Down)
                {
                    godown = false;
                }
            }

            private void tbMsg_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Thread thread = new Thread(() =>
                {
                    processMessage();
                });
                thread.Start();
                tbMsg.Enabled = false;
                this.Focus();

            }
        }

        private void processMessage()
        {
            try
            {
                if (!tbMsg.Text.Trim().Equals(""))
                {

                    string msg = tbMsg.Text;
                    IClient myself = null;
                    int mId = 1;
                    if (lider == null)
                    {
                        Thread thread3 = new Thread(() =>
                        {
                            takeLider(lider);
                        });
                        thread3.Start();
                        thread3.Join();
                    }

                    if (!lider.nick.Equals(this.nickname))
                        mId = askId();
                    else
                    {
                        Thread thread2 = new Thread(() =>
                        {
                            mId = askId();
                        });
                        thread2.Start();
                        thread2.Join();
                    }

                    foreach (ConnectedClient connectedClient in clients)
                    {
                        try
                        {
                            if (delayLog.ContainsKey(connectedClient.nick) && connectedClient.connected)
                            {
                                Thread thread = new Thread(() => delayThread(msg, mId, connectedClient));
                                thread.Start();
                            }
                            else if (!connectedClient.nick.Equals(nickname) && connectedClient.connected)
                            {
                                connectedClient.clientProxy.send(nickname, msg, mId);
                            }
                            else if (connectedClient.nick.Equals(nickname))
                            {
                                myself = connectedClient.clientProxy;
                            }
                        }
                        catch (SocketException exception)
                        {
                            //Client Disconnected
                            connectedClient.connected = false;
                            Console.WriteLine("Debug: " + exception.ToString());

                        }
                    }
                    if (myself != null)
                    {
                        Thread thread = new Thread(() => myself.send(nickname, msg, mId));
                        thread.Start();
                    }

                }
                tbMsg.Clear();
                tbMsg.Enabled = false;
                this.Focus();
            }
            catch (Exception exception)
            {
                Console.WriteLine("Debug: " + exception.ToString());
            }
        }

            //TODO receber messageID, Player+Move, Ghost+move
            public void updateChat(string nick, string msg)
            {
                tbChat.Text += nick + ": " + msg + "\r\n";
            }

            //TODO receber messageID, Player+Move, Ghost+move
            public void updateMove(int playernumber, string move)
            {
                tbChat.AppendText("\r\nUpdateMode");
                goleft = goright = goup = godown = false;
                PictureBox pb = getPictureBoxByName("pictureBoxPlayer" + playernumber);

                if (move.Equals("LEFT"))
                {
                    if (pb.Left > (boardLeft))
                    {
                        pb.Left -= speed;
                        pb.Image = Properties.Resources.Left;
                    }
                }
                if (move.Equals("RIGHT"))
                {
                    if (pb.Left < (boardRight))
                    {
                        pb.Left += speed;
                        pb.Image = Properties.Resources.Right;
                    }
                }
                if (move.Equals("UP"))
                {
                    if (pb.Top > (boardTop))
                    {
                        pb.Top -= speed;
                        pb.Image = Properties.Resources.Up;
                    }
                }
                if (move.Equals("DOWN"))
                {
                    if (pb.Top < (boardBottom))
                    {
                        pb.Top += speed;
                        pb.Image = Properties.Resources.down;
                    }
                }
                sent = false;


            }

            public void updateGhostsMove(int g1, int g2, int g3x, int g3y)
        {
            redGhost.Left = g1;
            yellowGhost.Left = g2;
            pinkGhost.Left = g3x;
            pinkGhost.Top = g3y;
        }

        internal void updateDead(int playerNumber)
        {
            Console.WriteLine(myNumber + playerNumber);
            if (myNumber == playerNumber)
            {
                dead = true;
                label2.Text = "GAME OVER";
                label2.Visible = true;
                getPictureBoxByName("pictureBoxPlayer" + myNumber).BackColor = Color.Black;
            }
            /*  PictureBox pb = retrievePicture(playerNumber);

              pb.Left = 0;
              pb.Top = 25; */
        }

        internal void updateCoin(string pictureBoxName)
        {
            foreach (Control x in this.Controls)
            {
                if (x is PictureBox && x.Tag == "coin" && x.Name.Equals(pictureBoxName))
                {
                    Controls.Remove(x);
                }
            }
        }

            internal void startGame(int playerNumbers)
            {
                if (!started)
                {
                    for (int i = 2; i <= playerNumbers; i++)
                        getPictureBoxByName("pictureBoxPlayer" + i).Visible = true;
                    started = true;

                    getPictureBoxByName("pictureBoxPlayer" + myNumber).BackColor = Color.LightSkyBlue;
                    tbChat.Text += "My Number " + myNumber;



                    Thread thread = new Thread(() => doWork());
                    thread.Start();
                }
            }

            private void doWork()
        {
            if (Program.FILENAME != "")
            {
                using (TextFieldParser parser = new TextFieldParser(Util.PROJECT_ROOT + "PuppetMaster" +
                    Path.DirectorySeparatorChar + "file" + Path.DirectorySeparatorChar + Program.FILENAME))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");
                    while (!parser.EndOfData)
                    {
                        //Processing row
                        string[] fields = parser.ReadFields();
                        try
                        {
                            doMove(fields[1]);
                        }
                        catch
                        {
                            tbChat.AppendText("\r\nOccur a error reading file");
                        }
                    }
                }
            }
        }

        public void writeToFile(int roundID)
        {
            using (StreamWriter sw = File.CreateText(Client.DIRECTORY + Path.DirectorySeparatorChar + roundID))
            {
                foreach (Control x in this.Controls)
                {
                    if (x is PictureBox && x.Tag == "ghost")
                    {
                        sw.WriteLine("M, " + x.Location.X + ", " + x.Location.Y);
                    }
                    else if (x is PictureBox && x.Tag == "coin")
                    {
                        sw.WriteLine("C, " + x.Location.X + ", " + x.Location.Y);
                    }
                    else if (x is PictureBox && x.Tag == "player1")
                    {
                        sw.WriteLine("P1, " + x.Location.X + ", " + x.Location.Y);
                    }
                    else if (x is PictureBox && x.Tag == "player2")
                    {
                        sw.WriteLine("P2, " + x.Location.X + ", " + x.Location.Y);
                    }
                    else if (x is PictureBox && x.Tag == "player3")
                    {
                        sw.WriteLine("P3, " + x.Location.X + ", " + x.Location.Y);
                    }
                    else if (x is PictureBox && x.Tag == "player4")
                    {
                        sw.WriteLine("P4, " + x.Location.X + ", " + x.Location.Y);
                    }
                    else if (x is PictureBox && x.Tag == "player5")
                    {
                        sw.WriteLine("P5, " + x.Location.X + ", " + x.Location.Y);
                    }
                    else if (x is PictureBox && x.Tag == "player6")
                    {
                        sw.WriteLine("P6, " + x.Location.X + ", " + x.Location.Y);
                    }
                }
            }
        }

        //Get Picture by String
        private PictureBox getPictureBoxByName(string name)
        {
            foreach (object p in this.Controls)
            {
                if (p.GetType() == typeof(PictureBox))
                    if (((PictureBox)p).Name == name)
                        return (PictureBox)p;
            }
            return new PictureBox(); //OR return null;
        }

        public void updateLider(ConnectedClient conn)
        {
            lider = conn;
        }

        public void takeLider(ConnectedClient prev)
        {
            int next = 1;
            Boolean clOk = false;
            IClient myself = null;

            if (lider != null)
                next = lider.playernumber + 1;
            while (!clOk)
            {
                foreach (ConnectedClient connectedClient in clients)
                {
                    if (connectedClient.playernumber == next)
                    {
                        if (connectedClient.connected)
                            clOk = true;
                        else
                        {
                            if (next == clients.Count)
                                next = 1;
                            else
                                next++;
                        }
                    }
                }
            }

            foreach (ConnectedClient connectedClient in clients)
            {

                try
                {
                    if (!connectedClient.nick.Equals(nickname) && connectedClient.connected)
                    {
                        connectedClient.clientProxy.sendLider(next);
                    }
                    else if (connectedClient.nick.Equals(nickname))
                    {
                        myself = connectedClient.clientProxy;
                    }
                }
                catch (SocketException exception)
                {
                    //Client Disconnected
                    connectedClient.connected = false;
                    Console.WriteLine("Debug: " + exception.ToString());

                }
            }
            if (myself != null)
            {
                Thread thread = new Thread(() => myself.sendLider(next));
                thread.Start();
                thread.Join();
            }

        }


        public void injectDelay(string nick)
        {
            //delay value defined
            int delay = 5000;
            if (!(delayLog.ContainsKey(nick)))
            {
                delayLog.Add(nick, delay);
            }
            else
            {
                delayLog[nick] += delay;
            }
        }

        public void delayThread(string msg, int mId, ConnectedClient conn)
        {
            Thread.Sleep(delayLog[conn.nick]);
            conn.clientProxy.send(nickname, msg, mId);
        }

        public int askId()
        {
            int temp = 1;
            try
            {
                temp = lider.clientProxy.getId();
                return temp;
            }
            catch (Exception ex)
            {
                lider.connected = false;
                Console.WriteLine("Debug: lider died " + ex.ToString());
                Thread thread3 = new Thread(() =>
                {
                    takeLider(lider);
                });
                thread3.Start();
                thread3.Join();
                if (!lider.nick.Equals(this.nickname))
                    temp = lider.clientProxy.getId();
                else
                {
                    Thread thread2 = new Thread(() =>
                    {
                        temp = lider.clientProxy.getId();
                    });
                    thread2.Start();
                    thread2.Join();
                }
                return temp;

            }
        }
        public void debugFunction(string text)
        {
            tbChat.AppendText(text);
        }
    }
}
