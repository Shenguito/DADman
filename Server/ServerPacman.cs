using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;



namespace Server {
    public partial class ServerForm : Form
    {
        public Dictionary<int, string> listMove;

        int boardRight = 320;
        int boardBottom = 320;
        int boardLeft = 0;
        int boardTop = 40;
        //player speed
        int speed = 5;
        
        int total_coins = 61;

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
            listMove=new Dictionary<int, string>();
            
            this.server = remoteServer;
            this.timer1.Interval = 2000;
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
                foreach (Client c in server.clientList)
                {
                    try
                    {
                        c.clientProxy.movePlayer(playerNumber, move);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception on server sendMove");
                    }

                }
            }
            catch(Exception e)
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

        private void timer1_Tick(object sender, EventArgs e)
        {
            foreach(KeyValuePair<int, string> entry in listMove)
            {
                processMove(entry.Key, entry.Value);
            }
            listMove = new Dictionary<int, string>();

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

            pinkGhost.Left += ghost3x;
            pinkGhost.Top += ghost3y;

            if (pinkGhost.Left < boardLeft ||
               pinkGhost.Left > boardRight ||
                (pinkGhost.Bounds.IntersectsWith(pictureBox1.Bounds)) ||
                (pinkGhost.Bounds.IntersectsWith(pictureBox2.Bounds)) ||
                (pinkGhost.Bounds.IntersectsWith(pictureBox3.Bounds)) ||
                (pinkGhost.Bounds.IntersectsWith(pictureBox4.Bounds)))
            {
                ghost3x = -ghost3x;
            }
            if (pinkGhost.Top < boardTop || pinkGhost.Top + pinkGhost.Height > boardBottom - 2)
            {
                ghost3y = -ghost3y;
            }
        }

    }
}
