using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.ComponentModel;

namespace jiwang.model
{
    public class ChatLink
    {
        ServerLink sl;
        Listener ls;

        string chatname;

        private Object thisLock = new Object();  

        public string getChatname()
        {
            return chatname;
        }

        public int groupNumber
        {
            get { return links.Count; }
        }

        string nickname;
        public string Nickname
        {
            get
            {
                return nickname;
            }
            set
            {
                nickname = value;
            }
        }

        class link
        {
            public string dst_username { get; set; }
            public Socket sendSocket { get; set; }
            public ManualResetEvent canSend { get; set; }
            public bool linked { get { return sendSocket.Connected; } }
        }

        List<link> links;

        public ChatLink(ServerLink sl, Listener ls, string chatname)
        {
            this.sl = sl;
            this.ls = ls;
            this.chatname = chatname;
            this.nickname = common.default_nickname;
            links = new List<link>();
        }

        public void addUser(string dst_username)
        {
            lock (thisLock)
            {
                Socket sendSocket;
                sendSocket = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);

                sendSocket.ReceiveBufferSize = 8192;
                sendSocket.ReceiveTimeout = 1000;
                sendSocket.SendBufferSize = 8192;
                sendSocket.SendTimeout = 1000;
                link l = new link();
                l.canSend = new ManualResetEvent(false);
                l.canSend.Set();
                l.dst_username = dst_username;
                l.sendSocket = sendSocket;
                links.Add(l);
            }
        }

        public void delUser(string dst_username)
        {
            lock (thisLock)
            {
                link l = null;
                foreach (link ll in links)
                {
                    if (ll.dst_username == dst_username)
                    {
                        l = ll;
                        break;
                    }
                }
                if (l != null)
                {
                    links.Remove(l);
                    if (l.linked)
                    {
                        l.sendSocket.Shutdown(SocketShutdown.Both);
                        l.sendSocket.Close();
                    }
                }
            }
        }

        public void start()
        {
            //data to Broadcast
            List<Byte> data = new List<Byte>();
            lock (thisLock)
            {
                foreach (link l in links)
                {
                    try
                    {
                        checkDstOnline(l);
                    }
                    catch (System.Exception ex)
                    {
                        ls.writeError(ex);
                    }
                }
                foreach (link l in links)
                {
                    byte[] name_header = common.str2ascii(
                        l.dst_username, common.name_header_length);
                    data.AddRange(name_header);
                }
            }
            sendMsg(common.type_str_invite_group, data.ToArray());
        }

        public void stop()
        {
            byte[] msg = common.str2ascii(sl.getUserName(), common.name_header_length);
            sendMsg(common.type_str_quit_group, msg);
        }

        void checkDstOnline(link l)
        {
            string dst_ip = null;
            if (!sl.linked) throw new Exception("您已离线.");
            sl.query4IP(l.dst_username, out dst_ip);
            IPAddress addr = IPAddress.Parse(dst_ip);
            IPEndPoint endpoint = new IPEndPoint(addr, common.p2p_port);

            if (!l.linked)
            {
                bool done = false;
                l.sendSocket.BeginConnect(endpoint.Address, endpoint.Port,
                    new AsyncCallback((IAsyncResult ar) =>
                    {
                        done = true;
                        Socket s = (Socket)ar.AsyncState;
                        s.EndConnect(ar);
                    }), l.sendSocket);
                while (!done)
                {
                    ;
                }
                Console.WriteLine("successfully connect " + l.sendSocket.RemoteEndPoint);
            }
            if (!l.linked) throw new Exception("无法连接"+l.dst_username+"的IP地址。");
        }

        //public void ping()
        //{
        //    echoreceived = false;
        //    sendMsg(common.type_str_ping, "");
        //    Thread.Sleep(common.ping_timeout);
        //    if (!echoreceived)
        //    {
        //        throw new Exception("对方客户端无响应，或者与我方客户端并不遵循同一套协议。");
        //    }
        //}

        bool echoreceived = false;
        string nicknameRequested = null;
        string nextFileName = string.Empty;
        string nextFileOwner = string.Empty;

        public void onReceive(string type_str, byte[] msg)
        {
            //Console.WriteLine("receive " + type_str);
            if (type_str == common.type_str_text)
            {
                string str_msg = common.unicode2Str(msg);
                ls.writeMsg(chatname, str_msg);
            }
            else if (type_str == common.type_str_file)
            {
                using (BackgroundWorker bw = new BackgroundWorker())
                {
                    bw.DoWork += (object o, DoWorkEventArgs ea) =>
                    {
                        if (nextFileOwner == string.Empty || nextFileName == string.Empty)
                        {
                            Thread.Sleep(common.waitfilename_timeout);
                        }
                        if (nextFileOwner == string.Empty)
                        {
                            return;
                        }
                        //防止在群聊中重复发给自己
                        if (nextFileOwner != sl.getUserName())
                        {
                            if (nextFileName == string.Empty)
                            {
                                return;
                            }
                            ls.writeFile(chatname, nextFileName, msg);
                            nextFileName = string.Empty;
                            nextFileOwner = string.Empty;
                        }
                    };
                    bw.RunWorkerCompleted += (object o, RunWorkerCompletedEventArgs ea) =>
                    {
                        ;
                    };
                    bw.RunWorkerAsync();
                }
            } 
            else if (type_str == common.type_str_ping)
            {
                //sendMsg(common.type_str_echo, "");
            }
            else if (type_str == common.type_str_echo)
            {
                //echoreceived = true;
            }
            else if (type_str == common.type_str_filename)
            {
                nextFileName = common.unicode2Str(msg);
            }
            else if (type_str == common.type_str_fileowner)
            {
                nextFileOwner = common.unicode2Str(msg);
            }
            else if (type_str == common.type_str_invite_group)
            {
                List<byte> lb_msg = new List<byte>(msg);
                int len = common.name_header_length;
                int usernum = msg.Length / len;
                lock (thisLock)
                {
                    for (int i = 0; i < usernum; i++)
                    {
                        string username = common.ascii2Str(lb_msg.GetRange(i * len, len));
                        bool exist = false;
                        foreach (link l in links)
                        {
                            if (username == l.dst_username)
                            {
                                exist = true;
                            }
                        }
                        if (!exist)
                        {
                            addUser(username);
                        }
                    }
                }
                foreach (link l in links)
                {
                    try
                    {
                        checkDstOnline(l);
                    }
                    catch (System.Exception ex)
                    {
                        ls.writeError(ex);
                    }
                }
                if (usernum == 2 && nickname == common.default_nickname)
                {
                    //还没有给群聊名称赋值，需要硬点一个
                    //这是个两人群，也就是单独聊天
                    string username;
                    string username1 = common.ascii2Str(lb_msg.GetRange(0, len));
                    string username2 = common.ascii2Str(lb_msg.GetRange(len, len));
                    if (username1 != sl.getUserName())
                    {
                        username = username1;
                    }
                    else
                    {
                        username = username2;
                    }
                    getDstNickname(username);
                }
            }
            else if (type_str == common.type_str_quit_group)
            {
                string username = common.ascii2Str(msg);
                delUser(username);
                if (links.Count == 1)
                {
                    ls.unregister(chatname);
                }
            }
            else if (type_str == common.type_str_set_groupname)
            {
                nickname = common.unicode2Str(msg);
                ls.refreshFriendList();
            }
            else if (type_str == common.type_str_request_nickname)
            {
                string username = common.ascii2Str(msg);
                if (username == sl.getUserName())
                {
                    sendMsg(common.type_str_answer_nickname, ls.getThisNickname());
                }
            }
            else if (type_str == common.type_str_answer_nickname)
            {
                nicknameRequested = common.unicode2Str(msg);
            }
        }

        //获得目标的nickname
        void getDstNickname(string username)
        {
            using (BackgroundWorker bw = new BackgroundWorker())
            {
                bw.DoWork += (object o, DoWorkEventArgs ea) =>
                {
                    nicknameRequested = null;
                    sendMsg(common.type_str_request_nickname, common.str2ascii(username, common.name_header_length));
                    Thread.Sleep(common.ping_timeout);
                    if (nicknameRequested == null)
                    {
                        nickname = username;
                    }
                    else
                    {
                        nickname = nicknameRequested;
                    }
                    ls.refreshFriendList();
                };
                bw.RunWorkerCompleted += (object o, RunWorkerCompletedEventArgs ea) =>
                {
                    ;
                };
                bw.RunWorkerAsync();
            }
        }

        class StateObject
        {
            public Socket workSocket = null;
            public byte[] data;
            public int sendPos = 0;
            public link l;
        }

        void SendCallback(IAsyncResult ar)
        {
            try
            {
                StateObject state = (StateObject)ar.AsyncState;
                Socket handler = state.workSocket;

                int bytesSent = handler.EndSend(ar);
                if (bytesSent > 0)
                {
                    state.sendPos += bytesSent;
                    if (state.sendPos < state.data.Length)
                    {
                        if (state.data.Length - state.sendPos >= common.buffersize)
                        {
                            handler.BeginSend(state.data, state.sendPos, common.buffersize, 0,
                                new AsyncCallback(SendCallback), state);
                        }
                        else
                        {
                            handler.BeginSend(state.data, state.sendPos, state.data.Length - state.sendPos, 0,
                                new AsyncCallback(SendCallback), state);
                        }
                    }
                    else
                    {
                        if (state.l.dst_username != sl.getUserName())
                            Console.WriteLine("endsend:" + state.l.dst_username + " " + common.ascii2Str(state.data));
                        state.l.canSend.Set();
                    }
                }
            }
            catch (System.Exception ex)
            {
                ls.writeError(ex);
            }
        }

        public void sendMsg(string type_str, byte[] msg)
        {
            //Console.WriteLine("send " + type_str);

            byte[] msg_len = common.str2ascii(
                msg.Length.ToString(), common.msglen_length);

            byte[] type_header = common.str2ascii(
                type_str, common.type_header_length);
            byte[] name_header = common.str2ascii(
                chatname, common.name_header_length);

            lock (thisLock)
            {
                List<Byte> data = new List<Byte>();
                data.AddRange(type_header);
                data.AddRange(name_header);
                data.AddRange(msg_len);
                data.AddRange(msg);
                foreach (link l in links)
                {
                    l.canSend.WaitOne();
                    StateObject state = new StateObject();
                    state.workSocket = l.sendSocket;
                    state.data = data.ToArray();
                    state.l = l;

                    if (l.dst_username != sl.getUserName())
                        Console.WriteLine("beginsend:" + l.dst_username + " " + common.ascii2Str(data));

                    try
                    {
                        // Send the data through the socket.
                        if (state.data.Length - state.sendPos >= common.buffersize)
                        {

                            l.sendSocket.BeginSend(state.data, state.sendPos, common.buffersize, 0,
                                new AsyncCallback(SendCallback), state);
                        }
                        else
                        {
                            l.sendSocket.BeginSend(state.data, state.sendPos, state.data.Length - state.sendPos, 0,
                                new AsyncCallback(SendCallback), state);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        ls.writeError(ex);
                    }
                }
            }

        }

        //发string默认用unicode发
        public void sendMsg(string type_str, string message)
        {
            byte[] msg = Encoding.Unicode.GetBytes(message);

            sendMsg(type_str, msg);
        }
    }
}
