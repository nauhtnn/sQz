using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
<<<<<<< HEAD
using System.Windows.Controls;

=======
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Packaging;
using System.IO;
>>>>>>> master
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

<<<<<<< HEAD
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
=======
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
                    NonnullRichTextBuilder richTextRuns = new NonnullRichTextBuilder(CompactRuns(runs));
                    richTexts.Enqueue(richTextRuns);
                }
            }
            doc.Close();
            return richTexts;
        }

        static List<object> CompactRuns(List<object> runs)
        {
            List<object> compactRuns = new List<object>();
            StringBuilder joinTexts = new StringBuilder();
            foreach(object run in runs)
            {
                string s = run as string;
                if(s == null)
                {
                    if(joinTexts.Length > 0)
                    {
                        compactRuns.Add(joinTexts.ToString());
                        joinTexts.Clear();
                    }
                    compactRuns.Add(run);
                }
                else
                    joinTexts.Append(s);
            }
            if (joinTexts.Length > 0)
                compactRuns.Add(joinTexts.ToString());
            return compactRuns;
        }

        public static int GetImageGUID()
        {
            return 0;
>>>>>>> master
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
<<<<<<< HEAD
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

        public static RichTextBox CreateRichTextBox(string text)
        {
            RichTextBox richText = new RichTextBox();
            char[] chars = text.ToCharArray();
            byte[] bytes = new byte[chars.Length];
            for (int i = 0; i < text.Length; ++i)
                bytes[i] = (byte)chars[i];
            System.IO.MemoryStream stream = new System.IO.MemoryStream(bytes);
            var range = new System.Windows.Documents.TextRange(richText.Document.ContentStart, richText.Document.ContentEnd);
            range.Load(stream, System.Windows.DataFormats.Rtf);

            return richText;
        }

        public static System.Windows.Documents.FlowDocument CreateRichText(string text)
        {
            char[] chars = text.ToCharArray();
            byte[] bytes = new byte[chars.Length];
            for (int i = 0; i < text.Length; ++i)
                bytes[i] = (byte)chars[i];
            System.IO.MemoryStream stream = new System.IO.MemoryStream(bytes);
            var rtDoc = new System.Windows.Documents.FlowDocument();
            var range = new System.Windows.Documents.TextRange(rtDoc.ContentStart, rtDoc.ContentEnd);
            range.Load(stream, System.Windows.DataFormats.Rtf);

            return rtDoc;
        }

        public static Queue<DocumentFormat.OpenXml.Wordprocessing.Paragraph>
            CreateDocxUnderlineParagraphs(string prefix, string text)
        {
            var docxParagraphs = new Queue<DocumentFormat.OpenXml.Wordprocessing.Paragraph>();
            var rtDoc = CreateRichText(text);
            foreach(var para in rtDoc.Blocks.OfType<System.Windows.Documents.Paragraph>())
            {
                if (para.Inlines.Count == 0)
                    continue;
                var docxPara = new DocumentFormat.OpenXml.Wordprocessing.Paragraph();
                var prefixText = new DocumentFormat.OpenXml.Wordprocessing.Text(prefix);
                prefixText.Space = DocumentFormat.OpenXml.SpaceProcessingModeValues.Preserve;
                docxPara.Append(new DocumentFormat.OpenXml.Wordprocessing.Run(prefixText));
                foreach (var span in para.Inlines.OfType<System.Windows.Documents.Span>())
                {
                    bool isUnderLine = false;
                    foreach(var decor in span.TextDecorations)
                        if(decor.Location == System.Windows.TextDecorationLocation.Underline)
                        {
                            isUnderLine = true;
                            break;
                        }
                    StringBuilder spanText = new StringBuilder();
                    foreach (var run in span.Inlines.OfType<System.Windows.Documents.Run>())
                        spanText.Append(run.Text);
                    var docxText = new DocumentFormat.OpenXml.Wordprocessing.Text(spanText.ToString());
                    docxText.Space = DocumentFormat.OpenXml.SpaceProcessingModeValues.Preserve;
                    var docxRun = new DocumentFormat.OpenXml.Wordprocessing.Run(docxText);
                    if (isUnderLine)
                    {
                        docxRun.RunProperties =
                            new DocumentFormat.OpenXml.Wordprocessing.RunProperties();
                        var underline =
                            new DocumentFormat.OpenXml.Wordprocessing.Underline()
                            { Val = DocumentFormat.OpenXml.Wordprocessing.UnderlineValues.Single };
                        docxRun.RunProperties.Append(underline);
                    }
                    docxPara.Append(docxRun);
                }
                docxParagraphs.Enqueue(docxPara);
            }
            return docxParagraphs;
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
=======
>>>>>>> master
        }
    }
}
