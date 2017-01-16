using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace sQzCS
{
    class Utils
    {
        public static string readFile(string fname)
        {
            try
            {
                string buf = System.IO.File.ReadAllText(fname);
                return buf;
            }
            catch (System.Exception)
            {
                System.Console.WriteLine("Cannot read file '{0}'", fname);
                return null;
            }
        }
        public static string cleanWhSp(string buf)
        {
            char[] whSp = { ' ', '\t', '\n', '\r'};
            int i = 0, e = buf.Length;
            while (i < e && whSp.Contains(buf[i]))
                ++i;//truncate front
            string s = null;
            while (i < e)
            {
                do
                    s += buf[i++];
                while (i < e && !whSp.Contains(buf[i]));
                if (i < e)
                {
                    int h = i;
                    do ++i;//truncate middle
                    while (i < e && whSp.Contains(buf[i]));
                    if (i < e)
                    {//truncate end
                        bool nl = false;
                        while (h < i && !nl)
                            if (buf[h++] == '\n')
                                nl = true;
                        if (nl)
                            s += '\n';
                        else
                            s += ' ';
                    }
                }
            }
            return s;
        }
        public static string HTMLspecialChars(string buf)
        {
            if (!buf.Contains('&') && buf.Contains('\"') &&
                buf.Contains('\'') && buf.Contains('<') &&
                buf.Contains('>'))
                return buf;
            string s = null;
            for (int i = 0; i < buf.Length; ++i)
            {
                switch (buf[i])
                {
                    case '&':
                        s += "&amp;";
                        break;
                    case '\"':
                        s += "&quot;";
                        break;
                    case '\'':
                        s += "&apos;";
                        break;
                    case '<':
                        s += "&lt;";
                        break;
                    case '>':
                        s += "&gt;";
                        break;
                    default:
                        s += buf[i];
                        break;
                }
            }
            return s;
        }
    }
}
