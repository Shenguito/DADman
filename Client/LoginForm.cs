using ComLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class LoginForm : Form
    {
        ClientForm form;


        public LoginForm(ClientForm form)
        {
            this.form = form;
            InitializeComponent();
            label4.Visible = false;
        }

        private void ButtonRegister_Click(object sender, EventArgs e)
        {


            if (!textNick.Text.Trim().Equals("") 
                && !textPort.Text.Trim().Equals("") 
                && textPort.Text.Trim().All(char.IsDigit))
            {
                Console.WriteLine("Teste init");
                form.Init(textNick.Text.Trim(), Int32.Parse(textPort.Text.Trim()));
            }
            else
            {
                label4.Text = "Insert fields...";
                label4.Visible = true;
            }
        }

        internal void LoginError()
        {
            label4.Text = "Register error, try again...";
            label4.Visible = true;
        }
    }
}
