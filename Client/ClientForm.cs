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

namespace Client {
    public partial class ClientForm : Form {

        public int myNumber = 1;

        string nickname;
        public int port;
        bool started = false;
        bool dead = false;
        bool sent = false;
        //CREATED
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
        
        private  TcpChannel channel;
        IServer serverProxy;

        public List<ConnectedClient> clients;

        
        public ClientForm() {
            
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
            
            serverProxy = (IServer)Activator.GetObject(
                typeof(IServer),
                Program.SERVERURL
            );

            // Registro do cliente
            RemoteClient rmc = new RemoteClient(this);
            string clientServiceName = "Client";
            // ## dont know what this does
            RemotingServices.Marshal(
                rmc,
                clientServiceName,
                typeof(RemoteClient)
            );

            //try catch missed
            if (serverProxy != null)
            {
                try
                {
                    serverProxy.connect(nickname, "tcp://"+ Util.GetLocalIPAddress()+":"+ port+"/Client");
                }
                catch
                {
                    tbChat.Text += "Catch: Didn't connected to server";
                }

            }
            else
            {
                tbChat.Text += "Else: Didn't connected to server";
            }
            
        }

        //CREATED
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
                if (move.Equals("LEFT"))
                {
                    serverProxy.sendMove(nickname, "LEFT");
                    sent = true;
                }
                else if (move.Equals("RIGHT"))
                {
                    serverProxy.sendMove(nickname, "RIGHT");
                    sent = true;
                }
                else if (move.Equals("UP"))
                {
                    serverProxy.sendMove(nickname, "UP");
                    sent = true;
                }
                else if (move.Equals("DOWN"))
                {
                    serverProxy.sendMove(nickname, "DOWN");
                    sent = true;
                }
            }
        }


        //Todo, sending only if he can
        private void keyisdown(object sender, KeyEventArgs e) {
            //CREATED
            if (!sent && !freeze)
                if (started)
                {
                    if (!dead)
                    {
                        if (e.KeyCode == Keys.Left)
                        {
                            serverProxy.sendMove(nickname, "LEFT");
                            sent = true;
                        }
                        if (e.KeyCode == Keys.Right)
                        {
                            serverProxy.sendMove(nickname, "RIGHT");
                            sent = true;
                        }
                        if (e.KeyCode == Keys.Up)
                        {
                            serverProxy.sendMove(nickname, "UP");
                            sent = true;
                        }
                        if (e.KeyCode == Keys.Down)
                        {
                            serverProxy.sendMove(nickname, "DOWN");
                            sent = true;
                            tbChat.Text += "enviei " + nickname + "DOWN";
                        }
                    }
                }
            if (e.KeyCode == Keys.Enter) {
                    tbMsg.Enabled = true; tbMsg.Focus();
            }
        }

        private void keyisup(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Left) {
                goleft = false;
            }
            if (e.KeyCode == Keys.Right) {
                goright = false;
            }
            if (e.KeyCode == Keys.Up) {
                goup = false;
            }
            if (e.KeyCode == Keys.Down) {
                godown = false;
            }
        }

        private void tbMsg_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                try
                {
                    if (!tbMsg.Text.Trim().Equals(""))
                    {

                        string msg = tbMsg.Text;
                        IClient myself = null;
                        int mId = 1;
                        if(lider != null)
                        {
                            try
                            {
                                Thread thread2 = new Thread(() => {
                                    mId = lider.clientProxy.getId();
                                });
                                thread2.Start();
                                thread2.Join();
                            }
                            catch(Exception ex)
                            {
                                Console.WriteLine("Debug: lider died i am lider now, resend prev message " + ex.ToString());
                                Thread thread = new Thread(() => takeLider(lider));
                                thread.Start();
                            }
                        }
                        else {
                            Thread thread = new Thread(() => takeLider(lider));
                            thread.Start();
                            thread.Join();/*   ele demora a fazer o take lider portanto este tem de esperar
                            if (lider.nick.Equals(this.nickname))
                            {
                                Thread thread2 = new Thread(() =>  {
                                    mId = lider.clientProxy.getId();
                                    });
                                thread2.Start();
                                thread2.Join();

                            }
                            else
                                mId = lider.clientProxy.getId(); */
                        }

                        foreach (ConnectedClient connectedClient in clients)
                        {
                            try
                            {
                                /*
                                if(!connectedClient.nick.Equals(nickname) && connectedClient.connected && delay)
                                {
                                    //Thread
                                }
                                
                                else */
                                if (!connectedClient.nick.Equals(nickname) && connectedClient.connected)
                                {
                                    connectedClient.clientProxy.send(nickname, msg, mId, delayLog);
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
                            Thread thread = new Thread(() => myself.send(nickname, msg, mId, delayLog));
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
        }
                
        public void updateChat(string nick, string msg)
        {
            tbChat.Text += nick + ": " + msg + "\r\n";
        }
        
        //TODO receber messageID, Player+Move, Ghost+move
        public void updateMove(int playernumber, string move)
        {
            tbChat.AppendText("\r\nUpdateMode");
            goleft = goright = goup = godown = false;
            PictureBox pb = getPictureBoxByName("pictureBoxPlayer"+playernumber);

            if (move.Equals("LEFT"))
            {
                if (pb.Left > (boardLeft)){
                    pb.Left -= speed;
                    pb.Image = Properties.Resources.Left;
                }
            }
            if (move.Equals("RIGHT"))
            {
                if (pb.Left < (boardRight)) { 
                    pb.Left += speed;
                    pb.Image = Properties.Resources.Right;
                }
            }
            if (move.Equals("UP"))
            {
                if (pb.Top > (boardTop)) { 
                    pb.Top -= speed;
                    pb.Image = Properties.Resources.Up;
                }
            }
            if (move.Equals("DOWN")) {
                if (pb.Top < (boardBottom)){
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
            for (int i = 2; i <= playerNumbers; i++)
                getPictureBoxByName("pictureBoxPlayer" + i).Visible = true;
            started = true;

            getPictureBoxByName("pictureBoxPlayer" + myNumber).BackColor = Color.LightSkyBlue;
            tbChat.Text += "My Number " + myNumber;

            
            //CREATED
            Thread thread = new Thread(() => doWork());
            thread.Start();
            
        }

        //CREATED
        private void doWork()
        {
            if (Program.FILENAME != "")
            {
                using (TextFieldParser parser = new TextFieldParser(Util.PROJECT_ROOT + "Client" +
                    Path.DirectorySeparatorChar + "file" + Path.DirectorySeparatorChar + Program.FILENAME))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");
                    tbChat.AppendText("\r\nENTREI");
                    while (!parser.EndOfData)
                    {
                        //Processing row
                        string[] fields = parser.ReadFields();
                        try
                        {
                            for (int i = 1; i < fields.Length; i += 2)
                            {
                                doMove(fields[i]);
                            }
                        }
                        catch
                        {
                            tbChat.AppendText("\r\nOccur a error reading file");
                        }
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
            IClient myself = null;
            if (lider != null)
                next = lider.playernumber + 1;

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
            }

        }


        public void injectDelay(String nick)
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

        public void debugFunction(string text)
        {
            tbChat.AppendText("Debug: " + text);
        }
    }
}
