using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace sQzLib
{
    public class Utils
    {
        static char[] WhiteChars = { ' ', '\t', '\n', '\r' };

        public static string ReadBytesOfString(byte[] buf, ref int offs)
        {
            int remainLength = buf.Length - offs;
            return ReadBytesOfString(buf, ref offs, ref remainLength);
        }

        public static string ReadBytesOfString(byte[] buf, ref int offs, ref int remainLenth)
        {
            if (remainLenth < 4)
                return null;
            int sz = BitConverter.ToInt32(buf, offs);
            remainLenth -= 4;
            offs += 4;
            if (remainLenth < sz)
                return null;
            string text;
            if (sz == 0)
                text = string.Empty;
            else
                text = Encoding.UTF8.GetString(buf, offs, sz);
            offs += sz;
            return text;
        }

        public static void AppendBytesOfString(string text, List<byte[]> byteList)
        {
            if (text == null || text.Length == 0)
                byteList.Add(BitConverter.GetBytes((int)0));
            else
            {
                byte[] b = Encoding.UTF8.GetBytes(text);
                byteList.Add(BitConverter.GetBytes(b.Length));
                byteList.Add(b);
            }
        }

        public static byte[] ToArray_FromListOfBytes(List<byte[]> l)
        {
            int sz = 0;
            foreach (byte[] x in l)
                sz += x.Length;
            byte[] buf = new byte[sz];
            sz = 0;
            foreach (byte[] x in l)
            {
                Buffer.BlockCopy(x, 0, buf, sz, x.Length);
                sz += x.Length;
            }
            return buf;
        }

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

        public static int GetMinutes(TimeSpan timeSpan)
        {
            return timeSpan.Hours * 60 + timeSpan.Minutes;
        }

        public static string GetPasswordCharset()
        {
            StringBuilder sb = new StringBuilder();
            for (char i = '0'; i <= '9'; ++i)
                sb.Append(i);
            //for (char i = 'A'; i < 'I'; ++i)
            //    sb.Append(i);
            //for (char i = 'J'; i <= 'Z'; ++i)
            //    sb.Append(i);
            //for (char i = 'a'; i < 'l'; ++i)
            //    sb.Append(i);
            //for (char i = 'm'; i <= 'z'; ++i)
            //    sb.Append(i);
            //sb.Append('!');
            //sb.Append('@');
            //sb.Append('#');
            //sb.Append('$');
            //sb.Append('%');
            //sb.Append('^');
            //sb.Append('&');
            //sb.Append('*');
            //sb.Append('(');
            //sb.Append(')');
            //sb.Append('-');
            //sb.Append('_');
            return sb.ToString();
        }

        public static string GeneratePassword(string vch, Random r)
        {
            int n = vch.Length;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 8; ++i)
                sb.Append(vch[r.Next() % n]);
            return sb.ToString();
        }

        public static string GetFirstNonBlankLine(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
                return null;
            string[] lines;
            try
            {
                lines = System.IO.File.ReadAllLines(filePath);
            }
            catch(System.IO.IOException e)
            {
                System.Windows.MessageBox.Show("GetFirstNonBlankLine " + filePath + "\n" + e.ToString());
                return null;
            }
            if (lines != null)
                foreach (string line in lines)
                    if (line.Trim().Length > 0)
                        return line;
            return null;
        }

        public static bool IsRichText(string text)
        {
            if (text.Length < 5 || !text.StartsWith("{\\rtf"))
                return false;
            else
                return true;
        }

        public static RichTextBox GetRichText(string text)
        {
            RichTextBox richText = new RichTextBox();
            byte[] bytes = new byte[text.Length];
            char[] chars = text.ToCharArray();
            for (int i = 0; i < text.Length; ++i)
                bytes[i] = (byte)chars[i];
            System.IO.MemoryStream stream = new System.IO.MemoryStream(bytes);
            var range = new System.Windows.Documents.TextRange(richText.Document.ContentStart, richText.Document.ContentEnd);
            range.Load(stream, System.Windows.DataFormats.Rtf);

            return richText;
        }

        public static int CountEnumerator(IEnumerator<object> itor)
        {
            int count = 1;
            while (itor.MoveNext())
                ++count;
            return count;
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
