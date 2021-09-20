using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sQzLib
{
    public interface IText
    {
        bool StartsWith(string text);
        string GetInnerText();

        char Last();
        void Replace(string item1, string v);

        void AppendBytesOfString(List<byte[]> byteList);
    }

    public class ITextFactory
    {
        public static ITextFactory Instance;

        public static IText CreateFromBytes(byte[] buf, ref int offs)
        {
            return Instance.VirtualCreateFromBytes(buf, ref offs);
        }

        public static IText CreateFromString(string s)
        {
            return Instance.VirtualCreateFromString(s);
        }

        protected virtual IText VirtualCreateFromBytes(byte[] buf, ref int offs)
        {
            return null;
        }

        protected virtual IText VirtualCreateFromString(string s)
        {
            return null;
        }
    }
}
