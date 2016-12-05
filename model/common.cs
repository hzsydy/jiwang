using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jiwang.model
{
    class common
    {
        public const int p2p_port = 23333;
        public const int name_header_length = 16;
        public const int type_header_length = 4;
        public const int msglen_length = 4;
        public const int msg_position = 24;

        public const string type_str_text = "text";
        public const string type_str_file = "file";


        public static string unicode2Str(byte[] buffer)
        {
            int inx = Array.FindIndex(buffer, 0, (x) => x == 0);//search for 0
            if (inx >= 0)
                return (Encoding.Unicode.GetString(buffer, 0, inx));
            else
                return (Encoding.Unicode.GetString(buffer)); 
        }

        public static string ascii2Str(byte[] buffer)
        {
            int inx = Array.FindIndex(buffer, 0, (x) => x == 0);//search for 0
            if (inx >= 0)
                return (Encoding.ASCII.GetString(buffer, 0, inx));
            else
                return (Encoding.ASCII.GetString(buffer));
        }

        public static string unicode2Str(List<byte> buffer)
        {
            return unicode2Str(buffer.ToArray());
        }

        public static byte[] str2unicode(string s, int len)
        {
            byte[] b = Encoding.Unicode.GetBytes(s);
            Array.Resize(ref b, len);
            return b;
        }
    }
}
