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
        string dst_username;

        public string getDstUserName()
        {
            return dst_username;
        }
        
        Socket sendSocket;

        //byte[] buffer = new byte[common.buffersize];

        public bool linked { get { return sendSocket.Connected; } }

        public ChatLink(ServerLink sl, Listener ls, string dst_username)
        {
            this.sl = sl;
            this.ls = ls;
            this.dst_username = dst_username;
            sendSocket = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            sendSocket.ReceiveBufferSize = 8192;
            sendSocket.ReceiveTimeout = 1000;
            sendSocket.SendBufferSize = 8192;
            sendSocket.SendTimeout = 1000;

        }

        public void start()
        {
            checkDstOnline();
            using (BackgroundWorker bw = new BackgroundWorker())
            {
                bw.DoWork += (object o, DoWorkEventArgs ea) =>
                {
                    if (linked)
                    {
                        ping();
                        Thread.Sleep(common.ping_interval);
                    }
                };
                bw.RunWorkerCompleted += (object o, RunWorkerCompletedEventArgs ea) =>
                {
                    if (ea.Error != null)
                    {
                        ls.writeMsg(ea.Error.Message);
                        ls.unregister(dst_username);
                    }
                    else
                    {
                        bw.RunWorkerAsync();
                    }
                };
                bw.RunWorkerAsync();
            }
        }

        public void stop()
        {
            sendSocket.Close();
        }

        public void checkDstOnline()
        {
            string dst_ip = null;
            if (!sl.linked) throw new Exception("您已离线.");
            sl.query4IP(dst_username, out dst_ip);
            IPAddress addr = IPAddress.Parse(dst_ip);
            IPEndPoint endpoint = new IPEndPoint(addr, common.p2p_port);

            if (!linked)
            {
                bool done = false;
                sendSocket.BeginConnect(endpoint.Address, endpoint.Port,
                    new AsyncCallback((IAsyncResult ar) =>
                    {
                        done = true;
                        Socket s = (Socket)ar.AsyncState;
                        s.EndConnect(ar);
                    }), sendSocket);
                while (!done)
                {
                    ;
                }
            }
            if (!linked) throw new Exception("无法连接服务器！");
        }

        public void ping()
        {
            if (linked)
            {
                echoreceived = false;
                sendMsg(common.type_str_ping, "");
                Thread.Sleep(common.ping_timeout);
                if (!echoreceived)
                {
                    throw new Exception("对方客户端无响应，或者与我方客户端并不遵循同一套协议。");
                }
            }
        }

        bool echoreceived = false;
        string nextFileName = string.Empty;

        public void onReceive(string type_str, byte[] msg)
        {
            if (type_str == common.type_str_text)
            {
                Console.WriteLine("receive text");
                string str_msg = common.unicode2Str(msg);
                ls.writeMsg(dst_username + ":" + str_msg);
            }
            else if (type_str == common.type_str_file)
            {
                Console.WriteLine("receive file");
                ls.writeFile(nextFileName, msg);
            } 
            else if (type_str == common.type_str_ping)
            {
                Console.WriteLine("receive ping");
                sendMsg(common.type_str_echo, "");
            }
            else if (type_str == common.type_str_echo)
            {
                Console.WriteLine("receive echo");
                echoreceived = true;
            }
            else if (type_str == common.type_str_filename)
            {
                Console.WriteLine("receive file name");
                nextFileName = common.unicode2Str(msg);
                ls.writeMsg(dst_username + "向您发送了文件" + nextFileName);
            }
        }

        public class StateObject
        {
            public Socket workSocket = null;
            public byte[] data;
            public int sendPos = 0;
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
                            sendSocket.BeginSend(state.data, state.sendPos, common.buffersize, 0,
                                new AsyncCallback(SendCallback), state);
                        }
                        else
                        {
                            sendSocket.BeginSend(state.data, state.sendPos, state.data.Length - state.sendPos, 0,
                                new AsyncCallback(SendCallback), state);
                        }
                    }
                    else
                    {
                        ;
                    }
                }
            }
            catch (System.Exception /*ex*/)
            {
                ;
            }
        }

        public void sendMsg(string type_str, byte[] msg)
        {
            StateObject state = new StateObject();

            byte[] msg_len = common.str2ascii(
                msg.Length.ToString(), common.msglen_length);

            byte[] type_header = common.str2ascii(
                type_str, common.type_header_length);
            byte[] name_header = common.str2ascii(
                sl.getUserName(), common.name_header_length);

            state.workSocket = sendSocket;
            List<Byte> data = new List<Byte>();
            data.AddRange(type_header);
            data.AddRange(name_header);
            data.AddRange(msg_len);
            data.AddRange(msg);
            state.data = data.ToArray();

            // Send the data through the socket.
            if (state.data.Length - state.sendPos >= common.buffersize)
            {
                sendSocket.BeginSend(state.data, state.sendPos, common.buffersize, 0,
                    new AsyncCallback(SendCallback), state);
            }
            else
            {
                sendSocket.BeginSend(state.data, state.sendPos, state.data.Length - state.sendPos, 0,
                    new AsyncCallback(SendCallback), state);
            }

            if (type_str == common.type_str_text)
            {
                Console.WriteLine("sending text");
            }
            else if (type_str == common.type_str_file)
            {
                Console.WriteLine("sending file");
            }
            else if (type_str == common.type_str_ping)
            {
                Console.WriteLine("sending ping");
            }
            else if (type_str == common.type_str_echo)
            {
                Console.WriteLine("sending echo");
            }
        }

        public void sendMsg(string type_str, string message)
        {
            byte[] msg = Encoding.Unicode.GetBytes(message);

            sendMsg(type_str, msg);

            if (type_str == common.type_str_text)
            {
                ls.writeMsg("you send : " + message);
            }
        }
    }
}
