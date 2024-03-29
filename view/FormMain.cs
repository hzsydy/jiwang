﻿using System;
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
using Microsoft.VisualBasic;

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

            //listBoxFriend.DisplayMember = "Nickname";
            listBoxFriend.DrawMode = DrawMode.OwnerDrawFixed;

            msgHistorys = new Dictionary<string, MsgHistory>();
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
                    if (!ls.isRunning())
                    {
                        ls.start();
                    }
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

        public string getThisNickname()
        {
            return textBoxNickname.Text;
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
                    if (nickname != "")
                    {
                        cl.Nickname = nickname;
                    }
                }
                else
                {
                    cl.sendMsg(common.type_str_set_groupname, nickname);
                }
                writeInstantMsg("已添加聊天：" + cl.Nickname);
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
                ChatLink cl = (ChatLink)listBoxFriend.Items[listBoxFriend.SelectedIndex];
                ls.unregister(cl.getChatname());
                writeInstantMsg("已删除聊天：" + cl.Nickname);
            }
        }

        private void buttonChangeName_Click(object sender, EventArgs e)
        {
            if (listBoxFriend.SelectedIndex > -1)
            {
                ChatLink cl = (ChatLink)listBoxFriend.Items[listBoxFriend.SelectedIndex];
                string newname = Interaction.InputBox("请输入新的备注", "计网大作业", "滑稽");
                if (cl.groupNumber == 2)
                {
                    cl.Nickname = newname;
                    listBoxFriend.Refresh();
                }
                else
                {
                    cl.sendMsg(common.type_str_set_groupname, newname);
                }
            }
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            if (listBoxFriend.SelectedIndex > -1)
            {
                string text = string.Format(
                    "{0} {1:MM-dd H:mm:ss}{2}",
                    getMyFullname(),
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

        string getMyFullname()
        {
            return string.Format("{0}({1})", textBoxNickname.Text, sl.getUserName());
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
                            getMyFullname(),
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

        public void writeCriticalError(Exception ex)
        {
            if (ex != null)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void popMsg(string msg)
        {
            MessageBox.Show(msg, "计网大作业");
        }

        public void writeInstantMsg(string msg)
        {
            //if(msg == "远程主机强迫关闭了一个现有的连接。")
            //{
            //    //发RST发习惯了我都数不清到底发了多少RST了
            //    //姑且假装什么事情都没有发生
            //    return;
            //}
            textBoxMsgReceive.AppendText(msg);
            textBoxMsgReceive.AppendText(Environment.NewLine);
        }

        public void writeMsg(string chatname, string msg)
        {
            MsgHistory m = getMsgHistory(chatname);
            m.sb.AppendLine(msg);
            m.unread++;
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
                           getMyFullname(),
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

        class MsgHistory
        {
            public StringBuilder sb;
            public int unread;
        }
        Dictionary<string, MsgHistory> msgHistorys;
        MsgHistory getMsgHistory(string chatname)
        {
            if (!msgHistorys.ContainsKey(chatname))
            {
                MsgHistory m = new MsgHistory();
                m.sb = new StringBuilder();
                m.unread = 0;
                msgHistorys.Add(chatname, m);
            }
            return msgHistorys[chatname];
        }

        void refreshTextWindow()
        {
            if (listBoxFriend.SelectedIndex != -1)
            {
                string chatname = ((ChatLink)listBoxFriend.Items[listBoxFriend.SelectedIndex]).getChatname();
                textBoxMsgReceive.Text = string.Empty;
                MsgHistory m = getMsgHistory(chatname);
                textBoxMsgReceive.AppendText(m.sb.ToString());
                m.unread = 0;
            }
            listBoxFriend.Refresh();
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
                StringBuilder sb = getMsgHistory(chatname).sb;
                sb.Append(history);
            }
        }

        void saveHistory()
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode rootNode = xmlDoc.CreateElement("chats");
            xmlDoc.AppendChild(rootNode);
            foreach (string chatname in msgHistorys.Keys)
            {
                XmlNode chatNode = xmlDoc.CreateElement("chat");
                XmlAttribute attribute = xmlDoc.CreateAttribute("chatname");
                attribute.Value = chatname;
                chatNode.Attributes.Append(attribute);
                chatNode.InnerText = (msgHistorys[chatname]).sb.ToString();
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

        private void listBoxFriend_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Index >= 0)
            {
                ChatLink cl = (ChatLink)listBoxFriend.Items[e.Index];
                string chatname = cl.getChatname();
                var m = getMsgHistory(chatname);
                if (m.unread > 0)
                {
                    string showText = string.Format("【{0}未读】{1}", m.unread, cl.Nickname);
                    e.Graphics.DrawString(showText, new Font("Arial", 8, FontStyle.Bold), Brushes.Black, e.Bounds);
                }
                else
                {
                    e.Graphics.DrawString(cl.Nickname, new Font("Arial", 8, FontStyle.Regular), Brushes.Black, e.Bounds);
                }
            }
            e.DrawFocusRectangle();
        }
    }
}
