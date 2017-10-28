using ComLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;



namespace pacman {
    public partial class ClientForm : Form {

        string nickname;
        public int port;

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

        int score = 0; int total_coins = 61;

        //ghost speed for the one direction ghosts
        int ghost1 = 5;
        int ghost2 = 5;
        
        //x and y directions for the bi-direccional pink ghost
        int ghost3x = 5;
        int ghost3y = 5;

        LoginForm formLogin;
        public TcpChannel channel;
        IServer serverProxy;

        public Dictionary<ClientChat, IClient> clients; 
        //public Dictionary<string, IClient> clients { get => clients; set => clients = value; }

        public ClientForm() {


            clients = new Dictionary<ClientChat, IClient>();
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            formLogin = new LoginForm(this);
            formLogin.Show();
            //InitializeComponent();
            //label2.Visible = false;

        }

        public void Init(string nickname, int port)
        {
            


            this.nickname = nickname;
            this.port = port;
            // Iniciar canal
            channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, false);

            // Registro do Servidor
            serverProxy = (IServer)Activator.GetObject(
                typeof(IServer),
                "tcp://localhost:8000/ChatServer" //lacking null verification
            );

            // Registro do cliente
            RemoteClient rmc = new RemoteClient(nickname, this);
            String clientServiceName = "ChatClient";

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
                    serverProxy.connect(nickname, "tcp://localhost:" + port + "/" + clientServiceName);

                    formLogin.Close();

                    //TODO loading for players

                    this.ShowInTaskbar = true;
                    this.WindowState = FormWindowState.Normal;
                    InitializeComponent();
                    label2.Visible = false;
                }
                catch (Exception e)
                {
                    formLogin.LoginError();
                }
            }
            else
            {
                //connection error problem
            }
        }

        //TODO move pacman
        private void keyisdown(object sender, KeyEventArgs e) {

            
            if (e.KeyCode == Keys.Left) {
                Thread thread = new Thread(() => serverProxy.sendMove(nickname,"left"));
                thread.Start();
               
            }
            if (e.KeyCode == Keys.Right) {
                Thread thread = new Thread(() => serverProxy.sendMove(nickname, "right"));
                thread.Start();
               
            }
            if (e.KeyCode == Keys.Up) {
                Thread thread = new Thread(() => serverProxy.sendMove(nickname, "up"));
                thread.Start();
               
            }
            if (e.KeyCode == Keys.Down) {
                Thread thread = new Thread(() => serverProxy.sendMove(nickname, "down"));
                thread.Start();
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

        private void timer1_Tick(object sender, EventArgs e) {
            label1.Text = "Score: " + score;

            
            //move ghosts
            redGhost.Left += ghost1;
            yellowGhost.Left += ghost2;

            // if the red ghost hits the picture box 4 then wereverse the speed
            if (redGhost.Bounds.IntersectsWith(pictureBox1.Bounds))
                ghost1 = -ghost1;
            // if the red ghost hits the picture box 3 we reverse the speed
            else if (redGhost.Bounds.IntersectsWith(pictureBox2.Bounds))
                ghost1 = -ghost1;
            // if the yellow ghost hits the picture box 1 then wereverse the speed
            if (yellowGhost.Bounds.IntersectsWith(pictureBox3.Bounds))
                ghost2 = -ghost2;
            // if the yellow chost hits the picture box 2 then wereverse the speed
            else if (yellowGhost.Bounds.IntersectsWith(pictureBox4.Bounds))
                ghost2 = -ghost2;
            //moving ghosts and bumping with the walls end
            //for loop to check walls, ghosts and points
            foreach (Control x in this.Controls) {
                // checking if the player hits the wall or the ghost, then game is over
                if (x is PictureBox && (x.Tag == "wall" || x.Tag == "ghost")) {
                    if (((PictureBox)x).Bounds.IntersectsWith(pictureBoxPlayer1.Bounds)) {
                        pictureBoxPlayer1.Left = 0;
                        pictureBoxPlayer1.Top = 25;
                        label2.Text = "GAME OVER";
                        label2.Visible = true;
                        timer1.Stop();
                    }
                }
                if (x is PictureBox && x.Tag == "coin") {
                    if (((PictureBox)x).Bounds.IntersectsWith(pictureBoxPlayer1.Bounds)) {
                        this.Controls.Remove(x);
                        score++;
                        //TODO check if all coins where "eaten"
                        if (score == total_coins) {
                            //pacman.Left = 0;
                            //pacman.Top = 25;
                            label2.Text = "GAME WON!";
                            label2.Visible = true;
                            timer1.Stop();
                            }
                    }
                }
            }
                pinkGhost.Left += ghost3x;
                pinkGhost.Top += ghost3y;

                if (pinkGhost.Left < boardLeft ||
                   pinkGhost.Left > boardRight ||
                    (pinkGhost.Bounds.IntersectsWith(pictureBox1.Bounds)) ||
                    (pinkGhost.Bounds.IntersectsWith(pictureBox2.Bounds)) ||
                    (pinkGhost.Bounds.IntersectsWith(pictureBox3.Bounds)) ||
                    (pinkGhost.Bounds.IntersectsWith(pictureBox4.Bounds))) {
                    ghost3x = -ghost3x;
                }
                if (pinkGhost.Top < boardTop || pinkGhost.Top + pinkGhost.Height > boardBottom - 2) {
                    ghost3y = -ghost3y;
                }
        }

        private void tbMsg_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter)
            {
                /*//TODO TEST dicrtionary only contains himself
                foreach(KeyValuePair<ClientChat, IClient> entry in clients)
                {
                    tbChat.Text += "PlayerName: " + entry.Key.nick + ":" + entry.Key.url+ "\r\n";
                }
                */
                if (!tbMsg.Text.Trim().Equals("")) {
                    foreach (KeyValuePair<ClientChat, IClient> entry in clients)
                    {
                        tbChat.AppendText ("exist: " + entry.Key.nick+" == "+nickname+ "\r\n");
                        if (!entry.Key.nick.Equals(nickname)) {
                            tbChat.AppendText(nickname + ": "+ tbMsg.Text + " to: "+ entry.Key.nick+ "\r\n");
                            Thread thread = new Thread(() => entry.Value.send(nickname, tbMsg.Text.ToString()));
                            thread.Start();
                        }
                    }
                    tbMsg.Clear();
                    tbMsg.Enabled = false;
                    this.Focus();
                }
            }
        }
        public void updateChat(string nick, string msg)
        {
            tbChat.Text += nick + ": " + msg + "\r\n";
        }

        public PictureBox retrievePicture(int playerNumber)
        {
            if (playerNumber == 1)
            {
                return pictureBoxPlayer1;
            }
            if (playerNumber == 2)
            {
                return pictureBoxPlayer2;
            }
            if (playerNumber == 3)
            {
                return pictureBoxPlayer3;
            }
            else
            {
                return pictureBoxPlayer4;
            }

        }

        public void updateMove(int playernumber, string move)
        {
            goleft = goright = goup = godown = false;
            PictureBox pb = retrievePicture(playernumber);

            if (move.Equals("left"))
            {
                if (pb.Left > (boardLeft)){
                    pb.Left -= speed;
                    pb.Image = Properties.Resources.Left;
                }
            }
            if (move.Equals("right"))
            {
                if (pb.Left < (boardRight)) { 
                    pb.Left += speed;
                    pb.Image = Properties.Resources.Right;
                }
            }
            if (move.Equals("up"))
            {
                if (pb.Top > (boardTop)) { 
                    pb.Top -= speed;
                    pb.Image = Properties.Resources.Up;
                }
            }
            if (move.Equals("down")) {
                if (pb.Top < (boardBottom)){
                    pb.Top += speed;
                    pb.Image = Properties.Resources.down;
                }
            }
        }
    }
}
