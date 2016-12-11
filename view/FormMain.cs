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
            sl = new ServerLink();

            ls = new Listener(sl);
            ls.form  = this;
        }

        void writeError(Exception ex)
        {
            if (ex != null)
            {
                writeMsg(ex.Message);
            }
        }

        public void writeMsg(string msg)
        {
            textBoxMsgReceive.AppendText(msg);
            textBoxMsgReceive.AppendText(Environment.NewLine);
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            try
            {
                ;
            }
            catch (System.Exception ex)
            {
                writeError(ex);
            }
        }

        private void buttonLogIn_Click(object sender, EventArgs e)
        {
            using (BackgroundWorker bw = new BackgroundWorker())
            {
                bw.DoWork += (object o, DoWorkEventArgs ea) =>
                {
                    sl.logIn(textBoxUsername.Text, textBoxPassword.Text);
                    ls.start();
                };
                bw.RunWorkerCompleted += (object o, RunWorkerCompletedEventArgs ea) =>
                {
                    writeError(ea.Error);
                    if (ea.Error == null)
                    {
                        writeMsg("您已经成功登录，用户名为" + sl.getUserName());
                    }
                };
                bw.RunWorkerAsync();
            }
        }

        private void buttonLogOut_Click(object sender, EventArgs e)
        {
            using (BackgroundWorker bw = new BackgroundWorker())
            {
                bw.DoWork += (object o, DoWorkEventArgs ea) =>
                {
                    sl.logOut();
                    ls.stop();
                };
                bw.RunWorkerCompleted += (object o, RunWorkerCompletedEventArgs ea) =>
                {
                    writeError(ea.Error);
                    if (ea.Error == null)
                    {
                        listBoxFriend.Items.Clear();
                        writeMsg("您已经成功下线，用户名为" + sl.getUserName());
                    }
                };
                bw.RunWorkerAsync();
            }
        }

        public void refreshFriendList(IEnumerable<string> keys)
        {
            listBoxFriend.Items.Clear();
            foreach (string s in keys)
            {
                listBoxFriend.Items.Add(s);
            };
            if (listBoxFriend.Items.Count > 0 && listBoxFriend.SelectedIndex == -1)
            {
                listBoxFriend.SelectedIndex = 0;
            }
        }

        private void buttonAddFriend_Click(object sender, EventArgs e)
        {
            string newuser = Interaction.InputBox("请输入好友用户名", "计网大作业", "2014011493");
            using (BackgroundWorker bw = new BackgroundWorker())
            {
                bw.DoWork += (object o, DoWorkEventArgs ea) =>
                {
                    ls.register(newuser);
                };
                bw.RunWorkerCompleted += (object o, RunWorkerCompletedEventArgs ea) =>
                {
                    writeError(ea.Error);
                };
                bw.RunWorkerAsync();
            }
        }

        private void buttonDelFriend_Click(object sender, EventArgs e)
        {
            if (listBoxFriend.SelectedIndex > -1)
            {
                ls.unregister((string)listBoxFriend.Items[listBoxFriend.SelectedIndex]);
            }
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            if (listBoxFriend.SelectedIndex > -1)
            {
                string username = (string)listBoxFriend.Items[listBoxFriend.SelectedIndex];
                string text = textBoxMsgSend.Text;
                try
                {
                    ChatLink cl = ls.getChatLink(username);
                    cl.sendMsg(common.type_str_text, text);
                    textBoxMsgSend.Text = string.Empty;
                }
                catch (System.Exception ex)
                {
                	 writeError(ex);
                }
            }
        }

        private void buttonSendFile_Click(object sender, EventArgs e)
        {
            if (listBoxFriend.SelectedIndex > -1)
            {
                string username = (string)listBoxFriend.Items[listBoxFriend.SelectedIndex];
                string text = textBoxMsgSend.Text;
                try
                {
                    ChatLink cl = ls.getChatLink(username);
                    cl.sendMsg(common.type_str_text, text);
                    textBoxMsgSend.Text = string.Empty;
                }
                catch (System.Exception ex)
                {
                    writeError(ex);
                }
            }
        }
    }
}
