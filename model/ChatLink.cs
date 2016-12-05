using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace jiwang.model
{
    class ChatLink
    {
        ServerLink sl;
        string dst_username;

        public string getDstUserName()
        {
            return dst_username;
        }

        string dst_ip;
        IPAddress addr;
        IPEndPoint endpoint;


        Socket sendSocket;

        byte[] buffer = new byte[1024];


        public ChatLink(ServerLink sl, string dst_username)
        {
            this.sl = sl;
            this.dst_username = dst_username;
            sendSocket = null;
        }

        public bool isOnline(ref Exception e)
        {
            e = null;
            if (!sl.isAlive())
            {
                e = new Exception("您已离线.");
                return false;
            }

            return sl.query4IP(dst_username, out dst_ip, ref e);
        }

        public bool isLinked()
        {
            return sendSocket != null;
        }

        public bool tryLink(ref Exception e)
        {
            e = null;
            sendSocket = null;
            if (!isOnline(ref e)) return false;

            addr = IPAddress.Parse(dst_ip);
            endpoint = new IPEndPoint(addr, common.p2p_port);
            sendSocket = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            sendSocket.ReceiveBufferSize = 8192;
            sendSocket.ReceiveTimeout = 1000;
            sendSocket.SendBufferSize = 8192;		
            sendSocket.SendTimeout = 1000;
            try
            {
                sendSocket.Connect(endpoint);
            }
            catch (System.Exception ex)
            {
                e = ex;
                sendSocket = null;
                return false;
            }
            return true;
        }

        public void onReceive(string type_str, byte[] msg)
        {
            if (type_str == common.type_str_text)
            {
                string str_msg = common.ascii2Str(msg);
                Console.WriteLine(dst_username+":"+str_msg);
            }
        }

        void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        void sendAsync(byte[] data)
        {
            sendSocket.BeginSend(data, 0, data.Length, 0,
                new AsyncCallback(SendCallback), sendSocket);
        }

        public bool sendMsg(string type_str, string message, ref Exception e)
        {
            e = null;
            if (!isOnline(ref e)) return false;
            if (!isLinked())
            {
                e = new Exception("您未连接到对方客户端。");
                return false;
            }
            try
            {
                byte[] msg = Encoding.ASCII.GetBytes(message);

                byte[] msg_len = common.str2ASCII(
                    msg.Length.ToString(), common.msglen_length);

                byte[] type_header = common.str2ASCII(
                    type_str, common.type_header_length);
                byte[] name_header = common.str2ASCII(
                    sl.getUserName(), common.name_header_length);

                // Send the data through the socket.
                sendAsync(type_header);
                sendAsync(name_header);
                sendAsync(msg_len);
                sendAsync(msg);
                Console.WriteLine(sl.getUserName()+ ":" + message);
            }
            catch (System.Exception ex)
            {
                e = ex;
                return false;
            }
            return true;
        }

        public bool cutLink(ref Exception e)
        {
            e = null;
            if (!isOnline(ref e)) return false;
            if (!isLinked()) return true;
            try
            {
                sendSocket.Shutdown(SocketShutdown.Both);
                sendSocket.Close();
            }
            catch (System.Exception ex)
            {
                e = ex;
                return false;
            }
            sendSocket = null;
            return true;
        }
    }
}
