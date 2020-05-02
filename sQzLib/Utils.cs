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

        public static Queue<NonnullRichTextBuilder> ReadAllLines(string path)
        {
            if (Path.GetExtension(path) == ".docx")
                return ReadAllLinesDocx(path);
            else
                return NonnullRichTextBuilder.NewWith(File.ReadAllLines(path));
        }

        static Queue<NonnullRichTextBuilder> ReadAllLinesDocx(string path)
        {
            Queue<NonnullRichTextBuilder> richTexts = new Queue<NonnullRichTextBuilder>();
            WordprocessingDocument doc = null;
            try
            {
                doc = WordprocessingDocument.Open(path, false);
            }
            catch(OpenXmlPackageException)
            {
                return richTexts;
            }
            catch (System.IO.IOException)
            {
                return richTexts;
            }
            Body body = doc.MainDocumentPart.Document.Body;
            foreach (Paragraph paragraph in body.ChildElements.OfType<Paragraph>())
            {
                DocumentFormat.OpenXml.Drawing.Blip hasImage =
                    paragraph.Descendants<DocumentFormat.OpenXml.Drawing.Blip>().FirstOrDefault();
                if (hasImage == null)
                {
                    richTexts.Enqueue(new NonnullRichTextBuilder(paragraph.InnerText));
                }
                else
                {
                    List<object> runs = new List<object>();
                    foreach (Run docRun in paragraph.ChildElements.OfType<Run>())
                    {
                        DocumentFormat.OpenXml.Drawing.Blip imgContainer =
                            docRun.Descendants<DocumentFormat.OpenXml.Drawing.Blip>().FirstOrDefault();
                        if (imgContainer == null)
                            runs.Add(docRun.InnerText);
                        else
                        {
                            string imgId = imgContainer.Embed.Value;
                            ImagePart imgPart = doc.MainDocumentPart.GetPartById(imgId) as ImagePart;
                            System.IO.Stream imgStream = imgPart.GetStream();
                            byte[] imgInBytes = new byte[imgStream.Length];
                            imgStream.Read(imgInBytes, 0, (int)imgStream.Length);
                            runs.Add(imgInBytes);
                        }
                    }
                    NonnullRichTextBuilder richTextRuns = new NonnullRichTextBuilder(runs);
                    richTexts.Enqueue(richTextRuns);
                }
            }
            doc.Close();
            return richTexts;
        }

        public static int GetImageGUID()
        {
            return 0;
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
