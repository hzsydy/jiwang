using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using jiwang.model;

namespace jiwang.model
{
    class ServerLink
    {
        IPAddress addr;
        IPEndPoint endpoint;
        Socket tcpSocket;

        byte[] login_resp, logout_resp, not_on_line;

        string username;

        bool linked;

        byte[] buffer = new byte[1024];

        public ServerLink(string ip = "166.111.140.14", int port=8000)
        {
            addr = IPAddress.Parse(ip);
            endpoint = new IPEndPoint(addr, port);
            tcpSocket = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            // Set the receive buffer size to 8k
            tcpSocket.ReceiveBufferSize = 8192;

            // Set the timeout for synchronous receive methods to 
            // 1 second (1000 milliseconds.)
            tcpSocket.ReceiveTimeout = 1000;

            // Set the send buffer size to 8k.
            tcpSocket.SendBufferSize = 8192;

            // Set the timeout for synchronous send methods
            // to 1 second (1000 milliseconds.)			
            tcpSocket.SendTimeout = 1000;
            linked = false;
            username = null;

            login_resp = Encoding.ASCII.GetBytes("lol");
            logout_resp = Encoding.ASCII.GetBytes("loo");
            not_on_line = Encoding.ASCII.GetBytes("\n");
        }

        bool beginWith(byte[] src, byte[] dst)
        {
            for (int i = 0; i < dst.Length; i++ )
            {
                if (src[i] != dst[i])
                {
                    return false;
                }
            }
            return true;
        }

        public bool isAlive()
        {
            return linked;
        }

        public string getUserName()
        {
            return username;
        }

        public bool start(ref Exception e)
        {
            e = null;
            try
            {
                if (linked) throw new System.Exception("已经连接服务器！");
                tcpSocket.Connect(endpoint);
            }
            catch (System.Exception ex)
            {
                e = ex;
                return false;
            }
            linked = true;
            return true;
        }

        public bool logIn(string username, string password, ref Exception e)
        {
            e = null;
            byte[] msg = Encoding.ASCII.GetBytes(username + "_" + password);
            try
            {
                buffer[0] = 0;

                if (!linked) throw new System.Exception("尚未连接服务器！");

                // Send the data through the socket.
                int bytesSent = tcpSocket.Send(msg);

                // Receive the response from the remote device.
                int bytesRec = tcpSocket.Receive(buffer);

                if (beginWith(buffer, login_resp))
                {
                    this.username = username;
                    return true;
                }
                else
                {
                    throw new System.Exception("未能收到服务器的正确登录应答！");
                }
            }
            catch (System.Exception ex)
            {
                e = ex;
                return false;
            }
        }

        public bool query4IP(string dst_username, out string ip, ref Exception e)
        {
            ip = null;
            e = null;
            byte[] msg = Encoding.ASCII.GetBytes("q" + dst_username);
            try
            {
                if (!linked) throw new System.Exception("尚未连接服务器！");
                buffer[0] = 0;

                // Send the data through the socket.
                int bytesSent = tcpSocket.Send(msg);

                // Receive the response from the remote device.
                int bytesRec = tcpSocket.Receive(buffer);

                if (beginWith(buffer, not_on_line))
                {
                    throw new System.Exception("该用户已经下线！");
                }
                else
                {
                    ip = common.ascii2Str(buffer);
                    return true;
                }
            }
            catch (System.Exception ex)
            {
                e = ex;
                return false;
            }
        }

        public bool logOut(ref Exception e)
        {
            e = null;
            byte[] msg = Encoding.ASCII.GetBytes("logout" + username);
            try
            {
                buffer[0] = 0;

                if (!linked) throw new System.Exception("尚未连接服务器！");

                // Send the data through the socket.
                int bytesSent = tcpSocket.Send(msg);

                // Receive the response from the remote device.
                int bytesRec = tcpSocket.Receive(buffer);

                if (beginWith(buffer, logout_resp))
                {
                    return true;
                }
                else
                {
                    throw new System.Exception("未能收到服务器的正确下线应答！");
                }
            }
            catch (System.Exception ex)
            {
                e = ex;
                return false;
            }
        }

        public bool stop(ref Exception e)
        {
            try
            {
                if (!linked) throw new System.Exception("尚未连接服务器！");
                // Release the socket.
                tcpSocket.Shutdown(SocketShutdown.Both);
                tcpSocket.Close();
            }
            catch (System.Exception ex)
            {
                e = ex;
                return false;
            }
            linked = false;
            return true;
        }
    }
}
