using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Packaging;
using Ap = DocumentFormat.OpenXml.ExtendedProperties;

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
        public static char[] sWhSp = { ' ', '\t', '\n', '\r' };

        public static string ReadFile(string fileName)
        {
            try
            {
                if (!System.IO.File.Exists(fileName))
                    return null;
                return System.IO.File.ReadAllText(fileName);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string[] ReadDocx(string fpath)
        {
            List<string> l = new List<string>();
            WordprocessingDocument doc = null;
            try
            {
                doc = WordprocessingDocument.Open(fpath, false);
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

        public static void WriteQuestionSheetToDocx(string name, IEnumerable<string> data, int nAns)
        {
            WordprocessingDocument doc = null;
            try
            {
                doc = WordprocessingDocument.Create(name, DocumentFormat.OpenXml.WordprocessingDocumentType.Document);
            }
            catch (OpenXmlPackageException)
            {
                return;
            }
            catch (System.IO.IOException)
            {
                return;
            }
            MainDocumentPart mainPart = doc.AddMainDocumentPart();
            NumberingDefinitionsPart numberingDefinitionsPart = mainPart.AddNewPart<NumberingDefinitionsPart>("rId1");
            GenerateNumberingDefinitionsPartContent(numberingDefinitionsPart);

            mainPart.Document = new Document();
            Body body = doc.MainDocumentPart.Document.AppendChild(new Body());
            int i = 3;
            foreach (string s in data)
            {
                ++i;
                if(i < 4)
                    body.AppendChild(CreateParagraph(s, 1));
                else
                {
                    body.AppendChild(CreateParagraph(s, 0));
                    i = -1;
                }
            }
            doc.Close();
        }

        public static Paragraph CreateParagraph(string s, int indent)
        {
            Paragraph paragraph1 = new Paragraph();

            paragraph1.Append(CreateParagraphProperties(indent));
            paragraph1.Append(new Run(new Text(s)));

            return paragraph1;
        }

        public static ParagraphProperties CreateParagraphProperties(int lvReference)
        {
            ParagraphProperties paragraphProperties1 = new ParagraphProperties();
            ParagraphStyleId paragraphStyleId1 = new ParagraphStyleId() { Val = "ListParagraph" };

            NumberingProperties numberingProperties1 = new NumberingProperties();
            NumberingLevelReference numberingLevelReference1 = new NumberingLevelReference() { Val = lvReference };
            NumberingId numberingId1 = new NumberingId() { Val = 1 };//hardcode

            numberingProperties1.Append(numberingLevelReference1);
            numberingProperties1.Append(numberingId1);

            paragraphProperties1.Append(paragraphStyleId1);
            paragraphProperties1.Append(numberingProperties1);
            paragraphProperties1.SpacingBetweenLines = new SpacingBetweenLines();
            paragraphProperties1.SpacingBetweenLines.Before = "1";
            paragraphProperties1.SpacingBetweenLines.After = "1";

            return paragraphProperties1;
        }

        public static string[] Split(string buf, char c)
        {
            char[] sWhSp = { ' ', '\t', '\n', '\r' };
            int s = 0, e = buf.Length;
            List<string> v = new List<string>();
            while (s < e && sWhSp.Contains(buf[s]))
                ++s;//clean front
            if (s == e)
                return null;
            char[] pack = { '{', '}' };//simple KeyValuePair
            int packing = 0;
            int i = s;
            StringBuilder str = new StringBuilder();
            while (i < e)
            {
                if (buf[i] == pack[0])
                    ++packing;
                else if (buf[i] == pack[1])
                {
                    --packing;
                    if (packing < 0)
                        packing = 0;
                }
                if (buf[i] == c)//no 'else if' here
                {
                    int j = i++ - 1;
                    while (s < j && sWhSp.Contains(buf[j]))
                        --j;//clean end
                    if (s <= j) //sure sWhSp doesnt contain buf[s]
                        str.Append(buf.Substring(s, j - s + 1));
                    if (str[str.Length - 1] == '\\') //concatenate
                    {
                        str.Remove(str.Length - 1, 1);
                        str.Append('\n');
                    }
                    else if (str[str.Length - 1] == '+') //concatenate
                        str.Remove(str.Length - 1, 1);
                    else if (0 < packing)
                        str.Append('\n');
                    else
                    {
                        v.Add(str.ToString());
                        str.Clear();
                    }
                    while (i < e && sWhSp.Contains(buf[i]))
                        ++i;
                    s = i;
                } else
                    ++i;
            }
            if (s < e)
                v.Add(buf.Substring(s, e - s));
            return v.ToArray();
        }

        public static string CleanSpace(string buf)
        {
            int i = 0, e = buf.Length;
            while (i < e && sWhSp.Contains(buf[i]))
                ++i;//truncate front
            StringBuilder s = new StringBuilder();
            while (i < e)
            {
                do
                    s.Append(buf[i++]);
                while (i < e && !sWhSp.Contains(buf[i]));
                if (i < e)
                {
                    int h = i;
                    do ++i;//truncate middle
                    while (i < e && sWhSp.Contains(buf[i]));
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

        public static string CleanFront(string buf, int s)
        {
            int i = s, e = buf.Length;
            while (i < e && sWhSp.Contains(buf[i]))
                ++i;
            if (i == e)
                return null;
            return buf.Substring(i);
        }

        public static string CleanFrontBack(string buf, int s, int e)
        {
            int i = s;
            while (i < e && sWhSp.Contains(buf[i]))
                ++i;
            if (i == e)
                return null;
            int j = e;
            while (i < j && sWhSp.Contains(buf[j]))
                --j;
            return buf.Substring(i, j - i + 1);
        }
        //public static void DetectContent(ref string buf)
        //{
        //    //content is a block, but doesn't contain '{', '}' inside,
        //    //  so distingushing from normal block
        //    System.Text.RegularExpressions.Match m =
        //        System.Text.RegularExpressions.Regex.Match(buf, "\\{[a-zA-Z0-9\\. \\\\/\\-_:]+\\}");
        //    if (m.Success)
        //    {
        //        t = ContentType.Image;
        //        string ipath = buf.Substring(m.Index + 1, m.Length - 2);
        //        buf = buf.Substring(0, m.Index) + "<img src='" + ipath +
        //            "'>" + buf.Substring(m.Index + m.Length);
        //    }
        //}
        static bool detectContent(char c)
        {
            //optimization, not use regex
            //System.Text.RegularExpressions.Match m =
            //    System.Text.RegularExpressions.Regex.Match(buf, "\\{[a-zA-Z0-9\\. \\\\/\\-_:]+\\}");
            if ('a' <= c && c <= 'z')
                return true;
            if ('A' <= c && c <= 'Z')
                return true;
            if ('0' <= c && c <= '9')
                return true;
            char[] spCh = {'.', ' ', '\\', '/', '-', '_', ':'};
            if (spCh.Contains(c))
                return true;
            return false;
        }
        public static string HTML(string buf, ref ContentType t)
        {
            //suppose buf is already cleaned by Split / CleanSpace
            if (buf[0] == '{' && buf[buf.Length - 1] == '}')
                buf = CleanFrontBack(buf, 1, buf.Length - 2);

            if (buf.Contains('&') || buf.Contains('\"') ||
                buf.Contains('\'') || !buf.Contains('<') ||
                buf.Contains('>') || !buf.Contains('\n'))
            {
                string s = null;
                for (int i = 0; i < buf.Length; ++i) {
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
                        case '\n':
                            s += "<br>";
                            break;
                        case '{':
                            //detect media content
                            int j = i + 1;
                            while (j < buf.Length && detectContent(buf[j]))
                                ++j;
                            if (j < buf.Length && buf[j] == '}')
                            {
                                t = ContentType.Image;
                                string ipath = buf.Substring(i + 1, j - i - 1);
                                s += "<img src='" + ipath + "'>";
                                i = j;
                            }
                            else {
                                s += buf.Substring(i, j - i);
                                i = j - 1;
                            }
                            break;
                        default:
                            s += buf[i];
                            break;
                    }
                }
                buf = s;
            }
            return buf;
        }

        // Generates content of numberingDefinitionsPart.
        private static void GenerateNumberingDefinitionsPartContent(NumberingDefinitionsPart numberingDefinitionsPart)
        {
            Numbering numbering = new Numbering();

            AbstractNum abstractNum = new AbstractNum() { AbstractNumberId = 0 };
            MultiLevelType multiLevelType1 = new MultiLevelType() { Val = MultiLevelValues.HybridMultilevel };

            Level level1 = new Level() { LevelIndex = 0 };
            StartNumberingValue startNumberingValue1 = new StartNumberingValue() { Val = 1 };
            NumberingFormat numberingFormat1 = new NumberingFormat() { Val = NumberFormatValues.Decimal };
            LevelText levelText1 = new LevelText() { Val = "%1." };
            LevelJustification levelJustification1 = new LevelJustification() { Val = LevelJustificationValues.Left };

            PreviousParagraphProperties previousParagraphProperties1 = new PreviousParagraphProperties();
            Indentation indentation1 = new Indentation() { Left = "0", Hanging = "360" };

            previousParagraphProperties1.Append(indentation1);

            level1.Append(startNumberingValue1);
            level1.Append(numberingFormat1);
            level1.Append(levelText1);
            level1.Append(levelJustification1);
            level1.Append(previousParagraphProperties1);

            Level level2 = new Level() { LevelIndex = 1 };
            StartNumberingValue startNumberingValue2 = new StartNumberingValue() { Val = 1 };
            NumberingFormat numberingFormat2 = new NumberingFormat() { Val = NumberFormatValues.LowerLetter };
            LevelText levelText2 = new LevelText() { Val = "(%2)" };
            LevelJustification levelJustification2 = new LevelJustification() { Val = LevelJustificationValues.Left };

            PreviousParagraphProperties previousParagraphProperties2 = new PreviousParagraphProperties();
            Indentation indentation2 = new Indentation() { Left = "360", Hanging = "360" };

            previousParagraphProperties2.Append(indentation2);

            level2.Append(startNumberingValue2);
            level2.Append(numberingFormat2);
            level2.Append(levelText2);
            level2.Append(levelJustification2);
            level2.Append(previousParagraphProperties2);

            abstractNum.Append(multiLevelType1);
            abstractNum.Append(level1);
            abstractNum.Append(level2);

            NumberingInstance numberingInstance1 = new NumberingInstance() { NumberID = 1 };
            AbstractNumId abstractNumId1 = new AbstractNumId() { Val = 0 };

            numberingInstance1.Append(abstractNumId1);

            numbering.Append(abstractNum);
            numbering.Append(numberingInstance1);

            numberingDefinitionsPart.Numbering = numbering;
        }
    }
}
