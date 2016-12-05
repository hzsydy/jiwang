using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using jiwang.model;

namespace jiwang
{
    public partial class FormMain : Form
    {
        ServerLink sl;
        Listener ls;

        public FormMain()
        {
            InitializeComponent();
        }

        void writeError(Exception ex)
        {
            if (ex != null)
            {
                textBoxMsgReceive.Text += ex.Message;
                textBoxMsgReceive.Text += Environment.NewLine;
            }
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            sl = new ServerLink();
            Exception ex = null;

            sl.start(ref ex);
            if (ex != null) { writeError(ex); return; }
        }

        private void buttonLogIn_Click(object sender, EventArgs e)
        {
            Exception ex = null;
            if (sl.logIn(textBoxUsername.Text, textBoxPassword.Text, ref ex))
            {
                ls = new Listener(sl);
                ls.start(ref ex);
                if (ex != null) { writeError(ex); return; }
            }
            else
            {
                if (ex != null) { writeError(ex); return; }
            }

        }

        private void buttonLogOut_Click(object sender, EventArgs e)
        {
            Exception ex = null;
            if (sl.logOut(ref ex))
            {
                ;
            }
            else
            {
                if (ex != null) { writeError(ex); return; }
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            FormMain fm = new FormMain();
            fm.Show();
        }
    }
}
