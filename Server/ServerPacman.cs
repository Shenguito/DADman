using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;



namespace Server {
    public partial class ServerForm : Form {

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

        private RemoteServer server;

        public ServerForm(RemoteServer remoteServer) {
            InitializeComponent();
            label2.Visible = false;
            this.server = remoteServer;
        }

        public PictureBox retrievePicture(int playerNumber)
        {
            if (playerNumber == 1)
            {
                return pictureBox1;
            }
            if (playerNumber == 2)
            {
                return pictureBox2;
            }
            if (playerNumber == 3)
            {
                return pictureBox3;
            }
            else
            {
                return pictureBox4;
            }

        }

        public void processMove(int playerNumber, string move)
        {
            PictureBox pb = retrievePicture(playerNumber);
            try
            {
                updatePlayerPosition(pb, move);

                foreach (Control x in this.Controls)
                {
                    // checking if the player hits the wall or the ghost, then game is over
                    if (x is PictureBox && x.Tag == "wall" || x.Tag == "ghost")
                    {
                        if (((PictureBox)x).Bounds.IntersectsWith(pb.Bounds))
                        {
                            pb.Left = 0;
                            pb.Top = 25;
                            sendPlayerDead(playerNumber);

                        }
                    }
                    if (x is PictureBox && x.Tag == "coin")
                    {
                        if (((PictureBox)x).Bounds.IntersectsWith(pb.Bounds))
                        {
                            Controls.Remove(x);
                            sendCoinEaten(playerNumber, x.Name);
                        }
                    }
                }
            }catch(Exception e)
            {
                Console.WriteLine("Start: \r\n" + e.ToString() + "\r\nEnd");
            }
            
        }



        public void sendPlayerDead(int playerNumber)
        {
            server.sendPlayerDead(playerNumber);
        }

        public void sendCoinEaten(int playerNumber, string coinName)
        {
            server.sendCoinEaten(playerNumber, coinName);
        }
        private void updatePlayerPosition(PictureBox pb, string move)
        {
            if (move.Equals("left"))
            {
                if (pb.Left > (boardLeft))
                {
                    pb.Left -= speed;
                    pb.Image = Properties.Resources.Left;
                }
            }
            if (move.Equals("right"))
            {
                if (pb.Left < (boardRight))
                {
                    pb.Left += speed;
                    pb.Image = Properties.Resources.Right;
                }
            }
            if (move.Equals("up"))
            {
                if (pb.Top > (boardTop))
                {
                    pb.Top -= speed;
                    pb.Image = Properties.Resources.Up;
                }
            }
            if (move.Equals("down"))
            {
                if (pb.Top < (boardBottom))
                {
                    pb.Top += speed;
                    pb.Image = Properties.Resources.down;
                }
            }
        }



    }
}
