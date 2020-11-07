using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sQzLib
{
    class Utils
    {
        static char[] WhiteChars = { ' ', '\t', '\n', '\r' };

        public static string CleanSpace(string buf)
        {
            int i = 0, e = buf.Length;
            while (i < e && WhiteChars.Contains(buf[i]))
                ++i;//truncate front
            StringBuilder s = new StringBuilder();
            while (i < e)
            {
                do
                    s.Append(buf[i++]);
                while (i < e && !WhiteChars.Contains(buf[i]));
                if (i < e)
                {
                    int h = i;
                    do ++i;//truncate middle
                    while (i < e && WhiteChars.Contains(buf[i]));
                    if (i < e)
                    {//truncate end
                        bool nl = false;
                        while (h < i && !nl)
                            if (buf[h++] == '\n')
                                nl = true;
                        if (nl)
                            s.Append('\n');
                        else
                            s.Append(' ');
                    }
                }
            }
            return s.ToString();
        }

        public static string CleanFront(string s)
        {
            int i = 0;
            while (i < s.Length && WhiteChars.Contains(s[i]))
                ++i;
            if (i == s.Length)
                return string.Empty;
            return s.Substring(i);
        }

        public static string CleantBack(string s)
        {
            int j = s.Length - 1;
            while (0 < j && WhiteChars.Contains(s[j]))
                --j;
            if (j < 0)
                return string.Empty;
            return s.Substring(0, j + 1);
        }
    }

    public class UICbMsg
    {
        private StringBuilder sb;
        private bool bUp;
        public UICbMsg() { sb = new StringBuilder(); bUp = false; }
        public string txt
        {
            get
            {
                if (bUp)
                {
                    string x = sb.ToString(); sb.Clear(); bUp = false; return x;
                }
                return string.Empty;
            }
        }
        public bool ToUp() { return bUp; }
        public static UICbMsg operator +(UICbMsg a, string s)
        {
            a.sb.Append(s);
            a.bUp = true;
            return a;
        }
        public override string ToString()
        {
            return sb.ToString();
        }
    }
}
