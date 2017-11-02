using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server
{
    public partial class ServerForm : Form
    {

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

        public ServerForm()
        {
            InitializeComponent();
            this.Visible = true;
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

        public void processMove(int playerNumber, string moveDirection)
        {
            PictureBox pb = retrievePicture(playerNumber);

            foreach (Control x in this.Controls)
            {
                // checking if the player hits the wall or the ghost, then game is over
                if (x is PictureBox && (x.Tag == "wall" || x.Tag == "ghost"))
                {
                    if (((PictureBox)x).Bounds.IntersectsWith(pb.Bounds))
                    {
                        pb.Left = 0;
                        pb.Top = 25;

                        //enviar mensagem a dizer que o player morreu

                    }
                }
                if (x is PictureBox && x.Tag == "coin")
                {
                    if (((PictureBox)x).Bounds.IntersectsWith(pb.Bounds))
                    {
                        this.Controls.Remove(x);
                        //enviar mensagem a dizer que a coin 'x' foi apanhada

                    }
                }
            }
        }
    }
}