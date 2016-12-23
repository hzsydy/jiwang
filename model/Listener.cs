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
    public class Listener
    {

        Socket listenSocket;
        const int backlog = 10;

        ServerLink sl;

        Dictionary<string, ChatLink> reg_chatlinks;

        bool working = false;
        bool isRunning() { return working; }

        ManualResetEvent allDone;

        private Object thisLock = new Object();  

        public Listener(ServerLink sl)
        {
            this.sl = sl;
            working = false;

            listenSocket = null;
            form = null;
            reg_chatlinks = new Dictionary<string, ChatLink>();

            allDone = new ManualResetEvent(false);
        }

        public FormMain form { get; set; }

        public ChatLink register(string chatname)
        {
            lock (thisLock)
            {
                if (!reg_chatlinks.ContainsKey(chatname))
                {
                    ChatLink new_cl = new ChatLink(sl, this, chatname);
                    reg_chatlinks.Add(chatname, new_cl);
                    if (form != null)
                    {
                        form.BeginInvoke((Action)delegate { form.refreshFriendList(reg_chatlinks); });
                    }
                }
                ChatLink cl = reg_chatlinks[chatname];
                return cl;
            }
        }

        public void unregister(string chatname)
        {
            lock (thisLock)
            {
                if (reg_chatlinks.ContainsKey(chatname))
                {
                    ChatLink cl = reg_chatlinks[chatname];
                    cl.stop();
                    reg_chatlinks.Remove(chatname);
                    if (form != null)
                    {
                        form.BeginInvoke((Action)delegate { form.refreshFriendList(reg_chatlinks); });
                    }
                }
            }
        }

        public void changeName(string oldname, string newname)
        {
            lock (thisLock)
            {
                if (reg_chatlinks.ContainsKey(oldname))
                {
                    ChatLink cl = reg_chatlinks[oldname];
                    reg_chatlinks.Remove(oldname);
                    reg_chatlinks.Add(newname, cl);
                    if (form != null)
                    {
                        form.BeginInvoke((Action)delegate { form.refreshFriendList(reg_chatlinks); });
                    }
                }
            }
        }

        public void writeMsg(string msg)
        {
            if (form != null)
            {
                form.BeginInvoke((Action)delegate { form.writeMsg(msg); });
            }
        }

        public void writeError(Exception ex)
        {
            if (form != null)
            {
                form.BeginInvoke((Action)delegate { form.writeError(ex); });
            }
        }

        public void writeFile(string filename, byte[] bytes)
        {
            if (form != null)
            {
                form.BeginInvoke((Action)delegate { form.writeFile(filename, bytes); });
            }
        }

        public ChatLink getChatLink(string username)
        {
            return reg_chatlinks[username];
        }

        public void start()
        {
            if (working) throw new Exception("您已经登录了。");
            string ip;
            sl.query4IP(sl.getUserName(), out ip);
            IPAddress ipAddress = IPAddress.Parse(ip);
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, common.p2p_port);
            try
            {
                listenSocket = new Socket(AddressFamily.InterNetwork,
                   SocketType.Stream, ProtocolType.Tcp);
                listenSocket.Bind(localEndPoint);
                listenSocket.Listen(backlog);
                working = true;
                using (BackgroundWorker bw = new BackgroundWorker())
                {
                    bw.DoWork += (object o, DoWorkEventArgs ea) =>
                    {
                        allDone.Reset();
                        listenSocket.BeginAccept(
                            new AsyncCallback(this.acceptCallback),
                            listenSocket); 
                        Console.WriteLine("Waiting for a connection...");
                        allDone.WaitOne();
                    };
                    bw.RunWorkerCompleted += (object o, RunWorkerCompletedEventArgs ea) =>
                    {
                        if (ea.Error == null && working)
                        {
                            bw.RunWorkerAsync();
                        }
                    };
                    bw.RunWorkerAsync();
                }
            }
            catch (System.Exception ex)
            {
                working = false;
                listenSocket.Close();
                throw ex;
            }
        }

        public void stop()
        {
            working = false;
            listenSocket.Close();
            string[] chats = reg_chatlinks.Keys.ToArray();
            foreach (string c in chats)
            {
                unregister(c);
            }
        }


        class StateObject
        {
            // Client  socket.
            public Socket workSocket = null;
            // Received data.
            public List<Byte> data = new List<Byte>();
            // buffer
            public byte[] buffer = new byte[common.buffersize];
  
        }

        bool parseStateData(StateObject state)
        {
            Console.WriteLine("receive:" + common.ascii2Str(state.data));
            if (state.data.Count >= common.msg_position)
            {
                List<byte> msg_len_header = state.data.GetRange(common.msglen_position, common.msglen_length);
                int msg_len = Convert.ToInt32(common.ascii2Str(msg_len_header));

                int expect_len = common.msg_position + msg_len;

                if (state.data.Count >= expect_len)
                {
                    //all data received. parse it

                    List<byte> type_header = state.data.GetRange(0, common.type_header_length);
                    List<byte> name_header = state.data.GetRange(
                        common.type_header_length, common.name_header_length);
                    List<byte> msg = state.data.GetRange(common.msg_position, msg_len);

                    string type_str = common.ascii2Str(type_header);
                    string chatname = common.ascii2Str(name_header);

                    lock (thisLock)
                    {
                        ChatLink cl = register(chatname);

                        cl.onReceive(type_str, msg.ToArray());
                    }


                    List<byte> left_msg = state.data.GetRange(expect_len,
                        state.data.Count - expect_len);
                    if (left_msg.Count == 0)
                        state.data = new List<byte>();
                    else
                    {
                        int notzero = left_msg.FindIndex((x) => x > 0);
                        if (notzero >= 0)
                        {
                            state.data = left_msg.GetRange(notzero, left_msg.Count - notzero);
                        }
                        else
                        {
                            state.data = new List<byte>();
                        }
                    }
                    state.buffer = new byte[common.buffersize];
                    return true;
                }
            }
            return false;
        }

        void acceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.
            allDone.Set();
            try
            {
                // Get the socket that handles the client request.
                Socket listener = (Socket)ar.AsyncState;
                Socket handler = listener.EndAccept(ar);
                StateObject state = new StateObject();
                state.workSocket = handler;
                state.buffer = new byte[common.buffersize];
                handler.BeginReceive(state.buffer, 0, state.buffer.Length, 0,
                    new AsyncCallback(readCallback), state);
            }
            catch (System.Exception ex)
            {
                writeError(ex);
            }
        }

        void readCallback(IAsyncResult ar)
        {
            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            try
            {
                // Read data from the client socket. 
                int bytesRead = handler.EndReceive(ar);
                Console.WriteLine(string.Format("receive {0} bytes", bytesRead));
                if (bytesRead > 0)
                {
                    state.data.AddRange(new List<Byte>(state.buffer).GetRange(0, bytesRead));
                    //parse data
                    while (parseStateData(state)) ;
                }
                // Not all data received. Get more.
                handler.BeginReceive(state.buffer, 0, state.buffer.Length, 0,
                    new AsyncCallback(readCallback), state);
            }
            catch (System.Exception ex)
            {
                writeError(ex);
            }
        }



    }
}