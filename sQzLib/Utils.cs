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

        public static Queue<RichTextBuilder> ReadAllLines(string path)
        {
            if (Path.GetExtension(path) == "docx")
                return ReadAllLinesDocx(path);
            else
                return RichTextBuilder.NewWith(File.ReadAllLines(path));
        }

        static Queue<RichTextBuilder> ReadAllLinesDocx(string path)
        {
            Queue<RichTextBuilder> richTexts = new Queue<RichTextBuilder>();
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
                    paragraph.Descendants<DocumentFormat.OpenXml.Drawing.Blip>().First();
                if (hasImage == null)
                {
                    string rawText = CleanSpace(paragraph.InnerText);
                    if (rawText.Length > 0)
                        richTexts.Enqueue(new RichTextBuilder(rawText));
                }
                else
                {
                    RichTextBuilder richTextRuns = new RichTextBuilder();
                    foreach (Run docRun in paragraph.ChildElements)
                    {
                        DocumentFormat.OpenXml.Drawing.Blip imgContainer =
                            docRun.Descendants<DocumentFormat.OpenXml.Drawing.Blip>().FirstOrDefault();
                        if (imgContainer == null)
                        {
                            richTextRuns.AddRun(CleanSpace(docRun.InnerText));
                        }
                        else
                        {
                            string imgId = imgContainer.Embed.Value;
                            ImagePart imgPart = doc.MainDocumentPart.GetPartById(imgId) as ImagePart;
                            System.IO.Stream imgStream = imgPart.GetStream();
                            byte[] imgInBytes = new byte[imgStream.Length];
                            imgStream.Read(imgInBytes, 0, (int)imgStream.Length);
                            richTextRuns.AddRun(imgInBytes);
                        }
                    }
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
