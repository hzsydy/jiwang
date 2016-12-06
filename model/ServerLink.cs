using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using jiwang.model;
using System.ComponentModel;

namespace jiwang.model
{
    class ServerLink
    {
        IPEndPoint endpoint;
        Socket tcpSocket;

        //private static Mutex mutex = new Mutex();
        Object lockObj = new Object();

        byte[] login_resp, logout_resp, not_on_line;

        string username;

        bool logged;

        byte[] buffer = new byte[1024];

        void genNewTcpSocket()
        {
            tcpSocket = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            tcpSocket.ReceiveBufferSize = 1024;
            tcpSocket.ReceiveTimeout = 1000;
            tcpSocket.SendBufferSize = 1024;
            tcpSocket.SendTimeout = 1000;
        }

        public ServerLink(string ip = "166.111.140.14", int port = 8000)
        {
            IPAddress addr = IPAddress.Parse(ip);
            endpoint = new IPEndPoint(addr, port);
            
            logged = false;
            username = null;

            login_resp = Encoding.ASCII.GetBytes("lol");
            logout_resp = Encoding.ASCII.GetBytes("loo");
            not_on_line = Encoding.ASCII.GetBytes("\n");

            genNewTcpSocket();
        }

        bool beginWith(byte[] src, byte[] dst)
        {
            for (int i = 0; i < dst.Length; i++)
            {
                if (src[i] != dst[i])
                {
                    return false;
                }
            }
            return true;
        }

        public bool linked { get { return tcpSocket.Connected; } }

        public string getUserName()
        {
            return username;
        }

        public void logIn(string username, string password)
        {
            if (Monitor.TryEnter(lockObj))
            {
                try
                {
                    byte[] msg = Encoding.ASCII.GetBytes(username + "_" + password);

                    buffer[0] = 0;

                    if (!linked)
                    {
                        bool done = false;
                        tcpSocket.BeginConnect(endpoint.Address, endpoint.Port,
                            new AsyncCallback((IAsyncResult ar) =>
                            {
                                done = true;
                                Socket s = (Socket)ar.AsyncState;
                                s.EndConnect(ar);
                            }), tcpSocket);
                        while (!done)
                        {
                            ;
                        }
                    }
                    if (!linked) throw new Exception("无法连接服务器！");
                    if (logged) throw new Exception("您已上线！");

                    int bytesSent = tcpSocket.Send(msg);
                    int bytesRec = tcpSocket.Receive(buffer);

                    if (beginWith(buffer, login_resp))
                    {
                        this.username = username;
                        logged = true;
                    }
                    else
                    {
                        throw new Exception("未能收到服务器的正确登录应答！");
                    }
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    Monitor.Exit(lockObj);
                }
            }
            else
            {
                throw new Exception("请等待上一项操作完成。");
            }
        }

        public void query4IP(string dst_username, out string ip)
        {
            if (Monitor.TryEnter(lockObj))
            {
                try
                {
                    ip = null;
                    byte[] msg = Encoding.ASCII.GetBytes("q" + dst_username);


                    if (!linked) throw new System.Exception("尚未连接服务器！");
                    if (!logged) throw new Exception("您未上线！");
                    buffer[0] = 0;

                    int bytesSent = tcpSocket.Send(msg);
                    int bytesRec = tcpSocket.Receive(buffer);

                    if (beginWith(buffer, not_on_line))
                    {
                        throw new System.Exception("该用户已经下线！");
                    }
                    else
                    {
                        ip = common.ascii2Str(buffer);
                    }
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    Monitor.Exit(lockObj);
                }
            }
            else
            {
                throw new Exception("请等待上一项操作完成。");
            }

        }

        public void logOut()
        {
            if (Monitor.TryEnter(lockObj))
            {
                try
                {
                    byte[] msg = Encoding.ASCII.GetBytes("logout" + username);

                    buffer[0] = 0;

                    if (!linked) throw new System.Exception("尚未连接服务器！");
                    if (!logged) throw new Exception("您未上线！");

                    int bytesSent = tcpSocket.Send(msg);
                    int bytesRec = tcpSocket.Receive(buffer);

                    if (beginWith(buffer, logout_resp))
                    {
                        logged = false;
                        
                        genNewTcpSocket();
                    }
                    else
                    {
                        throw new System.Exception("未能收到服务器的正确下线应答！");
                    }
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    Monitor.Exit(lockObj);
                }
            }
            else
            {
                throw new Exception("请等待上一项操作完成。");
            }
        }
    }
}
