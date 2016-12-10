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

        byte[] buffer = new byte[1024];

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
                        Thread.Sleep(10000);
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
                Thread.Sleep(5000);
                if (!echoreceived)
                {
                    throw new Exception("对方客户端无响应，或者与我方客户端并不遵循同一套协议。");
                }
            }
        }

        bool echoreceived = false;

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
                ;
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
        }


        void SendCallback(IAsyncResult ar)
        {
            Socket handler = (Socket)ar.AsyncState;
            int bytesSent = handler.EndSend(ar);
        }

        public void sendMsg(string type_str, string message)
        {
            byte[] msg = Encoding.Unicode.GetBytes(message);
            
            byte[] msg_len = common.str2ascii(
                msg.Length.ToString(), common.msglen_length);
            
            byte[] type_header = common.str2ascii(
                type_str, common.type_header_length);
            byte[] name_header = common.str2ascii(
                sl.getUserName(), common.name_header_length);

            type_header.CopyTo(buffer, 0);
            name_header.CopyTo(buffer, common.type_header_length);
            msg_len.CopyTo(buffer, common.msglen_position);
            msg.CopyTo(buffer, common.msg_position);
            
            // Send the data through the socket.
            sendSocket.BeginSend(buffer, 0, buffer.Length, 0,
                new AsyncCallback(SendCallback), sendSocket);
            
            ls.writeMsg("you send : " + message);
        }
    }
}
