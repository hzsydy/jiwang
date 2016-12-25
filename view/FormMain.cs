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
using System.Xml.Linq;
using System.Xml;

namespace jiwang.view
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

            msgHistory = new Dictionary<string, StringBuilder>();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            try
            {
                loadHistory();
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
                        writeInstantMsg("您已经成功登录，用户名为" + sl.getUserName());
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
                        writeInstantMsg("您已经成功下线，用户名为" + sl.getUserName());
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
                cl.start();
                if (users.Count == 1)
                {
                    cl.Nickname = nickname;
                }
                else
                {
                    cl.sendMsg(common.type_str_set_groupname, nickname);
                }
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

        public void writeError(Exception ex)
        {
            if (ex != null)
            {
                writeInstantMsg(ex.Message);
            }
        }

        public void writeInstantMsg(string msg)
        {
            if(msg == "远程主机强迫关闭了一个现有的连接。")
            {
                //发RST发习惯了我都数不清到底发了多少RST了
                //姑且假装什么事情都没有发生
                return;
            }
            textBoxMsgReceive.AppendText(msg);
            textBoxMsgReceive.AppendText(Environment.NewLine);
        }

        public void writeMsg(string chatname, string msg)
        {
            StringBuilder sb = getsb(chatname);
            sb.AppendLine(msg);
            refreshTextWindow();
        }

        public void writeFile(string chatname, string filename, byte[] bytes)
        {
            try
            {
                FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
                folderBrowserDialog.Description = "选择文件保存的位置";
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    string filepath = folderBrowserDialog.SelectedPath + @"\" + filename;
                    File.WriteAllBytes(filepath, bytes);
                    writeMsg(chatname, "文件已保存到" + filepath);

                    string text = string.Format(
                           "{0} 在 {1:MM-dd H:mm:ss} 成功接收了文件{2}",
                           sl.getUserName(),
                           DateTime.Now,
                           filename
                           );
                    ChatLink cl = ls.getChatLink(chatname);
                    cl.sendMsg(common.type_str_text, text);
                }
            }
            catch (System.Exception ex)
            {
                writeError(ex);
            }
        }

        Dictionary<string, StringBuilder> msgHistory;
        StringBuilder getsb(string chatname)
        {
            if (!msgHistory.ContainsKey(chatname))
            {
                msgHistory.Add(chatname, new StringBuilder());
            }
            return msgHistory[chatname];
        }

        void refreshTextWindow()
        {
            if (listBoxFriend.SelectedIndex != -1)
            {
                string chatname = ((ChatLink)listBoxFriend.Items[listBoxFriend.SelectedIndex]).getChatname();
                textBoxMsgReceive.Text = string.Empty;
                textBoxMsgReceive.AppendText(getsb(chatname).ToString());
            }
        }

        private void listBoxFriend_SelectedIndexChanged(object sender, EventArgs e)
        {
            refreshTextWindow();
        }

        void loadHistory()
        {
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load("chat_mistory.xml");
            }
            catch (System.Exception /*ex*/)
            {
                writeInstantMsg("未能找到上次运行的聊天记录！");
                return;
            }
            XmlNodeList chatNodes = xmlDoc.SelectNodes("//chats/chat");
            foreach (XmlNode chatNode in chatNodes)
            {
                string chatname = chatNode.Attributes["chatname"].Value;
                string history = chatNode.InnerText;
                StringBuilder sb = getsb(chatname);
                sb.Append(history);
            }
        }

        void saveHistory()
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode rootNode = xmlDoc.CreateElement("chats");
            xmlDoc.AppendChild(rootNode);
            foreach (string chatname in msgHistory.Keys)
            {
                XmlNode chatNode = xmlDoc.CreateElement("chat");
                XmlAttribute attribute = xmlDoc.CreateAttribute("chatname");
                attribute.Value = chatname;
                chatNode.Attributes.Append(attribute);
                chatNode.InnerText = msgHistory[chatname].ToString();
                rootNode.AppendChild(chatNode);
            }
            xmlDoc.Save("chat_mistory.xml");
        }

        private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            ls.stop();
            saveHistory();
            Environment.Exit(0);
        }
    }
}
