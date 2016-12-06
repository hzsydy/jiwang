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
        
        Socket sendSocket;

        byte[] buffer = new byte[1024];


        public ChatLink(ServerLink sl, string dst_username)
        {
            this.sl = sl;
            this.dst_username = dst_username;
            sendSocket = null;
        }

        public void checkDstOnline(out string dst_ip)
        {
            dst_ip = null;
            if (!sl.linked) throw new Exception("您已离线.");
            sl.query4IP(dst_username, out dst_ip);
        }

        public void onReceive(string type_str, byte[] msg)
        {
            if (type_str == common.type_str_text)
            {
                string str_msg = common.unicode2Str(msg);
                Console.WriteLine(dst_username+":"+str_msg);
            }
        }
        
        void sendAsync(byte[] data)
        {
            sendSocket.BeginSend(data, 0, data.Length, 0,
                new AsyncCallback(SendCallback), sendSocket);
        }
        
        void SendCallback(IAsyncResult ar)
        {
            Socket handler = (Socket)ar.AsyncState;

            int bytesSent = handler.EndSend(ar);
            Console.WriteLine("Sent {0} bytes to client.", bytesSent);

            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
        }

        public void sendMsg(string type_str, string message)
        {
            string dst_ip;
            checkDstOnline(out dst_ip);

            IPAddress addr = IPAddress.Parse(dst_ip);
            IPEndPoint endpoint = new IPEndPoint(addr, common.p2p_port);
            sendSocket = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            sendSocket.ReceiveBufferSize = 8192;
            sendSocket.ReceiveTimeout = 1000;
            sendSocket.SendBufferSize = 8192;		
            sendSocket.SendTimeout = 1000;
            
            sendSocket.Connect(endpoint);
            
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
            sendAsync(buffer);
            Console.WriteLine("you send : " + message);
        }
    }
}
