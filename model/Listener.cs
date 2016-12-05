using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace jiwang.model
{
    class Listener
    {
        Socket listenSocket;
        const int backlog = 10;
        ServerLink sl;
        Dictionary<string, ChatLink> reg_chatlinks;

        bool working = false;
        bool isStart() { return working; }
        ManualResetEvent allDone;

        const int buffersize = 1024;
        byte[] buffer = new byte[buffersize];

        public Listener(ServerLink sl)
        {
            this.sl = sl;
            working = false;

            listenSocket = null;
            reg_chatlinks = new Dictionary<string, ChatLink>();

            allDone = new ManualResetEvent(false);
        }

        // State object for reading client data asynchronously
        public class StateObject
        {
            // Client  socket.
            public Socket workSocket = null;
            // Received data.
            public List<Byte> data = new List<Byte>();
        }

        void acceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.
            allDone.Set();

            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);
            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(buffer, 0, buffersize, 0,
                new AsyncCallback(readCallback), state); 
        }

        void readCallback(IAsyncResult ar)
        {
            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket. 
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                state.data.AddRange(new List<Byte>(buffer).GetRange(0, bytesRead));

                int msg_len = Convert.ToInt32(common.ascii2Str(state.data.GetRange(
                    common.type_header_length + common.name_header_length,
                    common.msglen_length))
                    );

                int expect_len = common.name_header_length + common.type_header_length +
                    common.msglen_length + msg_len;

                if (state.data.Count >= expect_len)
                {
                    //all data received. parse it

                    List<byte> type_header = state.data.GetRange(0, common.type_header_length);
                    List<byte> name_header = state.data.GetRange(
                        common.type_header_length, common.name_header_length);
                    List<byte> msg = state.data.GetRange(common.msg_position, msg_len);

                    string type_str = common.ascii2Str(type_header);
                    string src_username = common.ascii2Str(name_header);

                    if (!reg_chatlinks.ContainsKey(src_username))
                    {
                        genNewChatLink(src_username);
                    }

                    ChatLink cl = reg_chatlinks[src_username];
                    cl.onReceive(type_str, msg.ToArray());
                }
                else
                {
                    // Not all data received. Get more.
                    handler.BeginReceive(buffer, 0, buffersize, 0,
                        new AsyncCallback(readCallback), state);
                }
            }
        }

        void register(ChatLink cl)
        {
            reg_chatlinks.Add(cl.getDstUserName(), cl);
        }

        void genNewChatLink(string username)
        {
            ChatLink cl = new ChatLink(sl, username);
            this.register(cl);
        }

        public bool start(ref Exception e)
        {
            e = null;                
            //IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            //IPAddress ipAddress = ipHostInfo.AddressList[0];
            string ip;
            if (!sl.query4IP(sl.getUserName(), out ip, ref e)) return false;
            IPAddress ipAddress = IPAddress.Parse(ip);
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, common.p2p_port);
            try
            {
                listenSocket = new Socket(AddressFamily.InterNetwork,
                   SocketType.Stream, ProtocolType.Tcp);
                listenSocket.Bind(localEndPoint);
                working = true;
                listenSocket.Listen(backlog);
                Task taskA = Task.Run(
                    () =>
                    {
                        while (working)
                        {
                            allDone.Reset();

                            Console.WriteLine("Waiting for a connection...");
                            listenSocket.BeginAccept(
                                new AsyncCallback(this.acceptCallback),
                                listenSocket);

                            allDone.WaitOne();
                        }
                        listenSocket.Shutdown(SocketShutdown.Both);
                        listenSocket.Close();
                        listenSocket = null;
                    }
                );
            }
            catch (System.Exception ex)
            {
                e = ex;
                working = false;
                return false;
            }
            return true;
        }

        public bool stop(ref Exception e)
        {
            e = null;
            try
            {
                working = false;
            }
            catch (System.Exception ex)
            {
                e = ex;
                return false;
            }
            return true;
        }



    }
}
