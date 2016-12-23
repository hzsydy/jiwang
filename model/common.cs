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
        public const int p2p_port = 23333;
        public const int name_header_length = 16;
        public const int type_header_length = 4;
        public const int msglen_length = 10;
        public const int msglen_position = 20;
        public const int msg_position = 30;
        public const int buffersize = 1024;

        public const int ping_interval = 5000;
        public const int ping_timeout = 2000;


        public const string type_str_text = "text";
        public const string type_str_filename = "flnm";
        public const string type_str_file = "file";
        public const string type_str_ping = "ping";
        public const string type_str_echo = "echo";

        public const string type_str_invite_group = "ivgp";
        public const string type_str_quit_group = "qtgp";

        public const string default_nickname = "huaji";

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

            for (int idx = 0; idx < identifier.Length; idx++)
            {
                int pos = randomData[idx] % AvailableCharacters.Length;
                identifier[idx] = AvailableCharacters[pos];
            }

            return new string(identifier);
        }


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
