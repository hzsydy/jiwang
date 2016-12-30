using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace jiwang.model
{
    class common
    {
        //连接端口号
        public const int p2p_port = 23333;
        
        //报文结构
        //前4字节是报文特征码，中间16字节是随机生成的聊天标识符，然后10字节是报文内容长度
        //30字节之后为报文内容
        public const int name_header_length = 16;
        public const int type_header_length = 4;
        public const int msglen_length = 10;
        public const int msglen_position = 20;
        public const int msg_position = 30;

        //单次缓冲区大小
        public const int buffersize = 1024;

        //ping的间隔时间和最大延时
        public const int ping_interval = 5000;
        public const int ping_timeout = 2000;


        public const int waitfilename_timeout = 1000;

        //报文特征码
        
        //发送文字
        public const string type_str_text = "text";
        //ping&echo
        public const string type_str_ping = "ping";
        public const string type_str_echo = "echo";
        //file相关
        public const string type_str_file = "file";
        public const string type_str_filename = "flnm";
        public const string type_str_fileowner = "flow";
        //group相关。注意，点对点事实上是两人群。
        public const string type_str_invite_group = "ivgp";
        public const string type_str_set_groupname = "stgn";
        public const string type_str_quit_group = "qtgp";
        //nickname相关
        public const string type_str_request_nickname = "qnkn";
        public const string type_str_answer_nickname = "ankn";


        public const string default_nickname = "正在加载昵称...";


        //生成随机字符串
        static readonly char[] AvailableCharacters = {
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 
            'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', 
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 
            'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', 
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '-', '_'
        };

        public static string generateIdentifier(int length)
        {
            char[] identifier = new char[length];
            byte[] randomData = new byte[length];

            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(randomData);
            }

            for (int idx = 0; idx < identifier.Length-1; idx++)
            {
                int pos = randomData[idx] % AvailableCharacters.Length;
                identifier[idx] = AvailableCharacters[pos];
            }
            //为了方便人类观赏报文制作的dirty fuck
            identifier[length - 1] = '_';

            return new string(identifier);
        }

        //一些愚蠢的转换函数

        public static string unicode2Str(byte[] buffer)
        {
            return (Encoding.Unicode.GetString(buffer)); 
        }

        public static string unicode2Str(List<byte> buffer)
        {
            return unicode2Str(buffer.ToArray());
        }

        public static string ascii2Str(byte[] buffer)
        {
            int inx = Array.FindIndex(buffer, 0, (x) => x == 0);//search for 0
            if (inx >= 0)
                return (Encoding.ASCII.GetString(buffer, 0, inx));
            else
                return (Encoding.ASCII.GetString(buffer));
        }

        public static string ascii2Str(List<byte> buffer)
        {
            return ascii2Str(buffer.ToArray());
        }

        public static byte[] str2ascii(string s, int len)
        {
            byte[] b = Encoding.ASCII.GetBytes(s);
            Array.Resize(ref b, len);
            return b;
        }

        public static byte[] str2unicode(string s, int len)
        {
            byte[] b = Encoding.Unicode.GetBytes(s);
            Array.Resize(ref b, len);
            return b;
        }
    }
}
