using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Packaging;
using System.IO;
namespace sQzLib
{
    public class UICbMsg
    {
        private StringBuilder sb;
        private bool bUp;
        public UICbMsg() { sb = new StringBuilder(); bUp = false; }
        public string txt
        {
            get {
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

    public class Utils
    {
        static readonly char[] WhiteChars = { ' ', '\t', '\n', '\r' };

        public static string[] ReadAllLines(string path)
        {
            try
            {
                if (!File.Exists(path))
                    return new string[0];
                if (Path.GetExtension(path) == "txt")
                    return File.ReadAllLines(path);
                else if (Path.GetExtension(path) == "docx")
                    return ReadAllLinesDocx(path);
                return new string[0];
            }
            catch (IOException)
            {
                return new string[0];
            }
        }

        static string[] ReadAllLinesDocx(string path)
        {
            List<string> l = new List<string>();
            WordprocessingDocument doc = null;
            try
            {
                doc = WordprocessingDocument.Open(path, false);
            }
            catch(OpenXmlPackageException)
            {
                return l.ToArray();
            }
            catch (System.IO.IOException)
            {
                return l.ToArray();
            }
            string s;
            Body body = doc.MainDocumentPart.Document.Body;
            //int idx = -1;
            foreach (Paragraph p in body.ChildElements.OfType<Paragraph>())
            {
                DocumentFormat.OpenXml.Drawing.Blip bl =
                    p.Descendants<DocumentFormat.OpenXml.Drawing.Blip>().FirstOrDefault();
                if (bl == null)
                {
                    if (0 < (s = CleanSpace(p.InnerText)).Length)
                        l.Add(s);
                }
                else
                {
                    throw new NotImplementedException();
                    //string id = bl.Embed.Value;
                    //ImagePart ip = doc.MainDocumentPart.GetPartById(id) as ImagePart;
                    //System.IO.Stream st = ip.GetStream();
                    //byte[] img = new byte[st.Length];
                    //st.Read(img, 0, (int)st.Length);
                    //System.IO.FileStream fs = new System.IO.FileStream("img" + ++idx, System.IO.FileMode.OpenOrCreate);
                    //fs.Write(img, 0, (int)st.Length);
                    //fs.Close();
                }
            }
            doc.Close();
            return l.ToArray();
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
    }
}
