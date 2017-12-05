using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Server {

    public partial class ServerForm : Form
    {

        public Dictionary<int, string> listMove = new Dictionary<int, string>();
        public List<int> deadPlayer;
        

        public bool started = false;

        string atecoin=" ";

        int roundID = 0;

        int boardRight = 320;
        int boardBottom = 320;
        int boardLeft = 0;
        int boardTop = 40;
        //player speed
        int speed = 5;
        string players_arg = "";
        string dead_arg = "";
        string monsters_arg = "";
        string coins_arg = "";
        

        //TO define when the game is over
        int total_coins = 61;

        //ghost speed for the one direction ghosts
        int ghost1 = 5;
        int ghost2 = 5;
        
        //x and y directions for the bi-direccional pink ghost
        int ghost3x = 5;
        int ghost3y = 5;

        private RemoteServer server;

        /*public Timer getTimer()
        {
            return this.timer1;
        }*/

        public ServerForm(RemoteServer remoteServer) {

            InitializeComponent();
            listMove = new Dictionary<int, string>();
            deadPlayer = new List<int>();
            

            this.server = remoteServer;
            this.timer1 = new System.Timers.Timer();
            if (Program.MSSEC != 0) { 
                this.timer1.Interval = Program.MSSEC; }
            else
            {
                this.timer1.Interval = 2000;
            }

            
            

            tbOutput.Text += "ServerForm criado." + timer1.ToString(); ;

        }
        
        public void processMove(int playerNumber, string move)
        {
            PictureBox pb = getPictureBoxByName("pictureBoxPlayer" + playerNumber);
            try
            {
                updatePlayerPosition(playerNumber,pb, move);
                
                foreach (Control x in this.Controls)
                {
                    // checking if the player hits the wall or the ghost, then game is over
                    if (x is PictureBox && x.Tag == "wall" || x.Tag == "ghost")
                    {
                        if (((PictureBox)x).Bounds.IntersectsWith(pb.Bounds))
                        {
                            dead_arg += "-" + playerNumber;
                            
                        }
                    }
                    if (x is PictureBox && x.Tag == "coin")
                    {
                        if (((PictureBox)x).Bounds.IntersectsWith(pb.Bounds))
                        {
                            
                            Controls.Remove(x);
                            coins_arg +="-" + x.Name;
                            atecoin+="-"+x.Left+":"+x.Top;
                            sendCoinEaten(playerNumber, x.Name);
                        }
                    }
                }
                foreach (Client c in server.clientList)
                {
                    if (c.connected)
                    {
                        try
                        {
                            //c.clientProxy.movePlayer(playerNumber, move);

                        }
                        catch (SocketException exception)
                        {
                            c.connected = false;
                            Console.WriteLine("Debug: " + exception.ToString());
                        }
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
            deadPlayer.Add(playerNumber);
        }

        public void sendCoinEaten(int playerNumber, string coinName)
        {
            server.sendCoinEaten(playerNumber, coinName);
        }
        
        private void updateGhostsPosition()
        {

            
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

            //TODO, writing the server localstate to a file, must be flexible with number of players
            // tbOutput.Text += PATH+ Path.DirectorySeparatorChar + "log" + Path.DirectorySeparatorChar + roundID;
            using (StreamWriter sw = File.CreateText(Server.DIRECTORY+ Path.DirectorySeparatorChar + roundID))
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
                        if (deadPlayer.Contains(1))
                        {
                            sw.WriteLine("P1, L, " + x.Location.X + ", " + x.Location.Y);
                            
                        }
                        else
                        {
                            sw.WriteLine("P1, P, " + x.Location.X + ", " + x.Location.Y);
                          
                        }
                    }
                    else if (x is PictureBox && x.Tag == "player2")
                    {
                        if (deadPlayer.Contains(2))
                        {
                            sw.WriteLine("P2, L, " + x.Location.X + ", " + x.Location.Y);
                           
                        }
                        else
                        {
                            sw.WriteLine("P2, P, " + x.Location.X + ", " + x.Location.Y);
                           
                        }
                    }
                    else if (x is PictureBox && x.Tag == "player3")
                    {
                        if (deadPlayer.Contains(2))
                        {
                            sw.WriteLine("P3, L, " + x.Location.X + ", " + x.Location.Y);
                           
                        }
                        else
                        {
                            sw.WriteLine("P3, P, " + x.Location.X + ", " + x.Location.Y);
                            
                        }
                    }
                    else if (x is PictureBox && x.Tag == "player4")
                    {
                        if (deadPlayer.Contains(2))
                        {
                            sw.WriteLine("P4, L, " + x.Location.X + ", " + x.Location.Y);
                          
                        }
                        else
                        {
                            sw.WriteLine("P4, P, " + x.Location.X + ", " + x.Location.Y);
                           
                        }
                    }
                    else if (x is PictureBox && x.Tag == "player5")
                    {
                        if (deadPlayer.Contains(2))
                        {
                            sw.WriteLine("P5, L, " + x.Location.X + ", " + x.Location.Y);
                           
                        }
                        else
                        {
                            sw.WriteLine("P5, P, " + x.Location.X + ", " + x.Location.Y);
                            
                        }
                    }
                    else if (x is PictureBox && x.Tag == "player6")
                    {
                        if (deadPlayer.Contains(2))
                        {
                            sw.WriteLine("P6, L, " + x.Location.X + ", " + x.Location.Y);
                           
                        }
                        else
                        {
                            sw.WriteLine("P6, P, " + x.Location.X + ", " + x.Location.Y);
                           
                        }
                    }
                }
               

            }

            monsters_arg += redGhost.Left + ":" + yellowGhost.Left + ":" + pinkGhost.Left + ":" + pinkGhost.Top;
        }

        private void updatePlayerPosition(int playerNumber, PictureBox pb, string move)
        {

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

            players_arg += "-" + playerNumber + ":" + move;

        }

        private string playerLocation()
        {
            string players=
            pictureBoxPlayer1.Top+" "+
            pictureBoxPlayer1.Left + " " +
            pictureBoxPlayer2.Top + " " +
            pictureBoxPlayer2.Left + " " +
            pictureBoxPlayer3.Top + " " +
            pictureBoxPlayer3.Left + " " +
            pictureBoxPlayer4.Top + " " +
            pictureBoxPlayer4.Left + " " +
            pictureBoxPlayer5.Top + " " +
            pictureBoxPlayer5.Left + " " +
            pictureBoxPlayer6.Top + " " +
            pictureBoxPlayer6.Left;
            return players;
        }

        private void processingTimer()
        {
            tbOutput.Text += ("Ronda " + roundID + " \r\n");
            //server.sendRoundUpdate(roundID, players_arg, dead_arg, monsters_arg, coins_arg);

            server.SendFirstRound(roundID, playerLocation(), monsters_arg, atecoin);
            server.sendRoundUpdate(roundID, players_arg, dead_arg, monsters_arg, coins_arg);
            roundID++;

            players_arg = dead_arg = monsters_arg = coins_arg = "";

            foreach (KeyValuePair<int, string> entry in listMove)
            {
                processMove(entry.Key, entry.Value);
            }
            listMove = new Dictionary<int, string>();

            updateGhostsPosition();
        }

        public void UpdateBoard(int roundID, string pl , string monst, string coin)
        {
            this.roundID = roundID;

            string[] pl_tok = pl.Split('-');
            
            string[] coin_tok = coin.Split('-');

            for (int i = 1; i < pl_tok.Length; i++)
            {
                string[] each_player_parameters = pl_tok[i].Split(' ');
                PictureBox pb = getPictureBoxByName("pictureBoxPlayer"+i);
                tbOutput.AppendText("\r\nPlayer: " + pb.Name);
                pb.Left = Int32.Parse(each_player_parameters[0]);
                pb.Top = Int32.Parse(each_player_parameters[1]);
            }

            //monsters_arg = redGhost.Left + ":" + yellowGhost.Left + ":" + pinkGhost.Left + ":" + pinkGhost.Top;
            string[] monst_tok = monst.Split(':');
            redGhost.Left = Int32.Parse(monst_tok[0]);
            yellowGhost.Left = Int32.Parse(monst_tok[1]);
            pinkGhost.Left = Int32.Parse(monst_tok[2]);
            pinkGhost.Top = Int32.Parse(monst_tok[3]);
                

            for (int i = 1; i < coin_tok.Length; i++)
            {
                //TODO, already received ate coins by left:top
                string[] each_coin = coin_tok[i].Split(':');
                tbOutput.AppendText("\r\n"+coin_tok[i]);
            }
        }

        public void timer1_Tick(object sender, EventArgs e)
        {
            Thread thread = new Thread(() => processingTimer());
            thread.Start();
        }

        public void startGame(int playerNumbers)
        {
            for (int i = 1; i <= playerNumbers; i++)
                getPictureBoxByName("pictureBoxPlayer" + i).Visible = true;

            timer1.Elapsed += timer1_Tick;
            timer1.Start();

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

    }
}
