using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using jiwang.model;
using System.IO;
using jiwang.view;

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

            listBoxFriend.DisplayMember = "Nickname";
        }

        public void writeError(Exception ex)
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

        public void refreshFriendList(Dictionary<string, ChatLink> regdict)
        {
            //TODO
            listBoxFriend.Items.Clear();
            foreach (string s in regdict.Keys)
            {
                listBoxFriend.Items.Add(regdict[s]);
            };
            if (listBoxFriend.Items.Count > 0 && listBoxFriend.SelectedIndex == -1)
            {
                listBoxFriend.SelectedIndex = 0;
            }
        }

        private void buttonAddFriend_Click(object sender, EventArgs e)
        {
            if (sl.linked)
            {
                FormAddFriend f = new FormAddFriend(this);
                f.Show();
            }
        }


        private void buttonAddGroup_Click(object sender, EventArgs e)
        {
            if (sl.linked)
            {
                FormAddGroup f = new FormAddGroup(this);
                f.Show();
            }
        }

        public void addFriend(string nickname, List<string> users)
        {
            string chatname = common.generateIdentifier(common.name_header_length);
            try
            {
                ChatLink cl = ls.register(chatname);
                cl.addUser(sl.getUserName());
                foreach (string user in users)
                {
                    cl.addUser(user);
                }
                cl.Nickname = nickname;
                cl.start();
            }
            catch (System.Exception ex)
            {
                writeError(ex);
            }
        }

        private void buttonDelFriend_Click(object sender, EventArgs e)
        {
            if (listBoxFriend.SelectedIndex > -1)
            {
                ls.unregister(((ChatLink)listBoxFriend.Items[listBoxFriend.SelectedIndex]).getChatname());
            }
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            if (listBoxFriend.SelectedIndex > -1)
            {
                string text = string.Format(
                    "{0} {1:MM-dd H:mm:ss}{2}", 
                    sl.getUserName(),
                    DateTime.Now,
                    Environment.NewLine
                    );
                text += textBoxMsgSend.Text;
                try
                {
                    ChatLink cl = (ChatLink)listBoxFriend.Items[listBoxFriend.SelectedIndex];
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
                string localFilePath = String.Empty;
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "所有文件(*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.Title = "请选择要传的文件";
                openFileDialog.RestoreDirectory = true;
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    localFilePath = openFileDialog.FileName.ToString();

                    string filename = Path.GetFileName(localFilePath);
                    try
                    {
                        byte[] bytes = File.ReadAllBytes(localFilePath);
                        
                        ChatLink cl = (ChatLink)listBoxFriend.Items[listBoxFriend.SelectedIndex];

                        string text = string.Format(
                            "{0} 在 {1:MM-dd H:mm:ss} 分享了文件{2}", 
                            sl.getUserName(),
                            DateTime.Now,
                            filename
                            );

                        using (BackgroundWorker bw = new BackgroundWorker())
                        {
                            bw.DoWork += (object o, DoWorkEventArgs ea) =>
                            {
                                cl.sendMsg(common.type_str_text, text);
                                cl.sendMsg(common.type_str_fileowner, sl.getUserName());
                                cl.sendMsg(common.type_str_filename, filename);
                                cl.sendMsg(common.type_str_file, bytes);
                            };
                            bw.RunWorkerCompleted += (object o, RunWorkerCompletedEventArgs ea) =>
                            {
                                ;
                            };
                            bw.RunWorkerAsync();
                        }

                    }
                    catch (System.Exception ex)
                    {
                        writeError(ex);
                    }
                }
            }
        }

        public void writeFile(string filename, byte[] bytes)
        {
            try
            {
                FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
                folderBrowserDialog.Description = "选择文件保存的位置";
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    string filepath = folderBrowserDialog.SelectedPath + @"\" + filename;
                    File.WriteAllBytes(filepath, bytes);
                    writeMsg("文件已保存到" + filepath);
                }
            }
            catch (System.Exception ex)
            {
                writeError(ex);
            }
        }

    }
}
