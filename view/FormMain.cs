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
using Microsoft.VisualBasic;

namespace jiwang
{
    public partial class FormMain : Form
    {
        ServerLink sl = null;
        Listener ls = null;

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
                ls.onRegDictChange += refreshFriendList;
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
                ls = null;
                listBoxFriend.Items.Clear();
            }
            else
            {
                if (ex != null) { writeError(ex); return; }
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //FormMain fm = new FormMain();
            //fm.Show();
        }

        void refreshFriendList(Dictionary<string, ChatLink> dict)
        {
            listBoxFriend.Items.Clear();
            foreach (string s in dict.Keys)
            {
                listBoxFriend.Items.Add(s);
            }
        }

        private void buttonAddFriend_Click(object sender, EventArgs e)
        {
            if (ls != null)
            {
                ls.register(Interaction.InputBox("请输入好友用户名", "计网大作业", "2014011493"));
            }
        }

        private void buttonDelFriend_Click(object sender, EventArgs e)
        {
            if (listBoxFriend.SelectedIndex > -1)
            {
                if (ls != null)
                {
                    ls.unregister((string)listBoxFriend.Items[listBoxFriend.SelectedIndex]);
                }
            }
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            if (listBoxFriend.SelectedIndex > -1)
            {
                string username = (string)listBoxFriend.Items[listBoxFriend.SelectedIndex];
                if (ls != null)
                {
                    ChatLink cl = ls.getChatLink(username);

                    Exception ex = null;
                    if (cl.tryLink(ref ex))
                    {
                        string text = textBoxMsgSend.Text;
                        if (cl.sendMsg(common.type_str_text, text, ref ex))
                        {
                            textBoxMsgSend.Text = string.Empty;
                        } 
                        else
                        {
                            if (ex != null) { writeError(ex); return; }
                        }
                    }
                    else
                    {
                        if (ex != null) { writeError(ex); return; }
                    }
                }
            }
        }
    }
}
