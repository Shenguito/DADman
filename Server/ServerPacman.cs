using ComLibrary;
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
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Server {

    public partial class ServerForm : Form
    {
        public delegate void ThrWork();

        public Dictionary<int, string> listMove = new Dictionary<int, string>();
        public List<int> deadPlayer;

        public Dictionary<int, BoardInfo> boardByRound;
        

        public bool started = false;

        public int roundID = 0;

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

        public string lastMonster = "";
        public string lastDeadPlayer = "";

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
            boardByRound = new Dictionary<int, BoardInfo>();
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
                updatePlayerPosition(playerNumber, pb, move);

                foreach (Control x in this.Controls)
                {
                    // checking if the player hits the wall or the ghost, then game is over
                    if (x is PictureBox && x.Tag == "wall" || x.Tag == "ghost")
                    {
                        if (((PictureBox)x).Bounds.IntersectsWith(pb.Bounds))
                        {
                            sendPlayerDead(playerNumber);
                        }
                    }
                    if (x is PictureBox && x.Tag == "coin")
                    {
                        if (((PictureBox)x).Bounds.IntersectsWith(pb.Bounds))
                        {
                            getPictureBoxByName(x.Name).Visible = false;
                            Controls.Remove(x);
                            server.clientList.FirstOrDefault(t => t.playernumber == playerNumber).score++;
                            coins_arg += "-" + x.Name;
                            sendCoinEaten(playerNumber, x.Name);
                        }
                    }
                }
            }
            catch (Exception e)
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
            
            monsters_arg = redGhost.Left + ":"+ redGhost.Top+":" + yellowGhost.Left + ":" +
                yellowGhost.Top + ":" + pinkGhost.Left + ":" + pinkGhost.Top;
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
            string players = "";
            for (int i=1; i <= server.clientList.Count; i++)
            {
                players += "-"+
                    getPictureBoxByName("pictureBoxPlayer"+i).Top + ":" +
                    getPictureBoxByName("pictureBoxPlayer" + i).Left;

                if (deadPlayer.Contains(i))
                {
                    players += ":D";
                }
                else if (server.clientList[i - 1].dead)
                {
                    deadPlayer.Add(server.clientList[i - 1].playernumber);
                    players += ":D";
                }
                else
                {
                    players += ":A";
                }
            }
            return players;
        }

        private void processingTimer()
        {
            //tbOutput.Text += ("Ronda " + roundID + " \r\n");
            roundID++;
            players_arg = "";
            
            foreach (KeyValuePair<int, string> entry in listMove)
            {
                processMove(entry.Key, entry.Value);
            }
            listMove = new Dictionary<int, string>();
            updateGhostsPosition();
            //to remember players_arg=LEFT:D  && playerLocation=-x:y
            BoardInfo thisround = new BoardInfo(roundID, playerLocation(), players_arg, monsters_arg, coins_arg);
            try
            {
                boardByRound.Add(roundID, thisround);
            }
            catch
            {
                tbOutput.AppendText("\r\nAdding same round error");
            }
            server.sendRoundUpdate(thisround);
        }

        public void UpdateBoard(BoardInfo board)
        {
            tbOutput.AppendText("updating");
            this.roundID = board.RoundID;
            boardByRound.Add(roundID, board);
            string[] pl_tok = board.Players.Split('-');
            
            string[] coin_tok = board.Coins.Split('-');
            
            for (int i = 1; i < pl_tok.Length; i++)
            {
                PictureBox pb = getPictureBoxByName("pictureBoxPlayer"+i);
                pb.Top = Int32.Parse(pl_tok[i].Split(':')[0]);
                pb.Left = Int32.Parse(pl_tok[i].Split(':')[1]);
                pb.Visible = true;
            }

            //monsters_arg = redGhost.Left + ":" + yellowGhost.Left + ":" + pinkGhost.Left + ":" + pinkGhost.Top;
            string[] monst_tok = board.Monsters.Split(':');
            redGhost.Left = Int32.Parse(monst_tok[0]);
            yellowGhost.Left = Int32.Parse(monst_tok[2]);
            pinkGhost.Left = Int32.Parse(monst_tok[4]);
            pinkGhost.Top = Int32.Parse(monst_tok[5]);

            for (int i = 1; i < coin_tok.Length; i++)
            {
                PictureBox pb = getPictureBoxByName(coin_tok[i]);
                pb.Visible = false;
                Controls.Remove(pb);
            }
            //TODO, TIMER SYNCHRONIZATION
            tbOutput.AppendText("set timer");
            //timer1.Elapsed += timer1_Tick;
            //timer1.Start();
            tbOutput.AppendText("timer setted");
        }

        public void startGame(int playerNumbers)
        {
            for (int i = 1; i <= playerNumbers; i++)
                getPictureBoxByName("pictureBoxPlayer" + i).Visible = true;

            timer1.Elapsed += timer1_Tick;
            timer1.Start();
        }

        public BoardInfo getLocalState(int roundID)
        {
            return boardByRound[roundID];
        }

        public void ReceivingMove(Movement move)
        {
            //TODO IMPORTANTE roundIDsheng, aqui está o problema do 6 players a por mesma roundid
            if (!listMove.ContainsKey(move.roundID))
                try
                {
                    listMove.Add(move.playernumber, move.move);
                }
                catch
                {
                    //TODO problem
                    tbOutput.AppendText("\r\n"+move.nick + ":" + move.roundID+" already exists");
                }
        }
        public void timer1_Tick(object sender, EventArgs e)
        {
            //try delegate
            Thread t = new Thread(new ThreadStart(processingTimer));
            t.Start();
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
