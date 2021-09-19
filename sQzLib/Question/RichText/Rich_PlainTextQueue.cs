using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Packaging;
using System.Windows.Controls;

namespace sQzLib
{
    public class Rich_PlainTextQueue
    {
		public static IEnumerable<IText> GetTextQueue(string filePath)
		{
			Queue<Rich_PlainText> lines = ReadTrimLines(filePath);
            Queue<Rich_PlainText> tokens = new Queue<Rich_PlainText>();
            while (lines.Count > 0)
            {
                if (lines.Peek().ElementAt(0) != '{')
                {
                    if (lines.Peek().ElementAt(0) == '\\' &&
                        lines.Peek().Length > 1)
                    {
                        tokens.Enqueue(lines.Dequeue().Substring(1));
                    }
                    else
                        tokens.Enqueue(lines.Dequeue());
                }
                else if (lines.Peek().ElementAt(lines.Peek().Length - 1) == '}')
                {
                    if (lines.Peek().Length > 2)
                    {
                        Rich_PlainText token = lines.Dequeue();
                        tokens.Enqueue(token.Substring(1, token.Length - 2));
                    }
                }
                else
                    tokens.Enqueue(JoinLinesTo1Token(lines));
            }
            return tokens;
		}
		
		static Rich_PlainText JoinLinesTo1Token(Queue<Rich_PlainText> lines)
		{
			Rich_PlainText token = lines.Dequeue().Substring(1);
			while(lines.Count > 0)
			{
				if(lines.Peek().ElementAt(lines.Peek().Length - 1) == '}')
				{
                    Rich_PlainText s = lines.Dequeue();
                    if (s.Length > 1)
                        token.AppendNewParagraphs(s.Substring(0, s.Length - 1));
					break;
				}
				else if (lines.Peek().ElementAt(lines.Peek().Length - 1) == '\\' &&
                    lines.Peek().Length > 1)
                {
                    Rich_PlainText s = lines.Dequeue();
                    token.AppendNewParagraphs(s.Substring(0, s.Length - 1));
                }
                else
                    token.AppendNewParagraphs(lines.Dequeue());
			}
			return token;
		}
		
		static Queue<Rich_PlainText> ReadTrimLines(string filePath)
		{
			if(System.IO.Path.GetExtension(filePath) == ".docx")
				return ReadTrimDocx(filePath);
			else
				return ReadTrimTxt(filePath);
		}
		
		static Queue<Rich_PlainText> ReadTrimTxt(string filePath)
		{
            Queue<Rich_PlainText> lines = new Queue<Rich_PlainText>();
            string[] rawLines;
            try
            {
                rawLines = System.IO.File.ReadAllLines(filePath);
            }
            catch(System.IO.IOException e)
            {
                System.Windows.MessageBox.Show(e.ToString());
                return lines;
            }
			foreach(string line in rawLines)
			{
				if (0 < line.Trim().Length)
					lines.Enqueue(new Rich_PlainText(line));
			}
			return lines;
		}

        static Queue<Rich_PlainText> ReadTrimDocx(string fpath)
        {
            Queue<Rich_PlainText> lines = new Queue<Rich_PlainText>();
            WordprocessingDocument doc = null;
            try
            {
                bool exists = System.IO.File.Exists(fpath);
                doc = WordprocessingDocument.Open(fpath, false);
            }
            catch(OpenXmlPackageException e)
            {
                System.Windows.MessageBox.Show(e.ToString());
                return lines;
            }
            catch (System.IO.IOException e)
            {
                System.Windows.MessageBox.Show(e.ToString());
                return lines;
            }
            Body body = doc.MainDocumentPart.Document.Body;
            foreach (Paragraph p in body.ChildElements.OfType<Paragraph>())
            {
                DocumentFormat.OpenXml.Drawing.Blip bl =
                    p.Descendants<DocumentFormat.OpenXml.Drawing.Blip>().FirstOrDefault();
                if (bl == null)
                {
                    if(IsUnderlined(p))
                    {
                        Queue<ParagraphData> paragraphs = new Queue<ParagraphData>();
                        ParagraphData para = new ParagraphData();
                        foreach(Run run in p.ChildElements.OfType<Run>())
                        {
                            RunData richText_run = new RunData(run.InnerText);
                            if (IsBoldItalicUnderline(run))
                                richText_run.Format = TEXT_FORMAT.Underline;
                            para.Runs.Enqueue(richText_run);
                        }
                        paragraphs.Enqueue(para);
                        lines.Enqueue(new Rich_PlainText(paragraphs));
                    }
                    else if(0 < Utils.CleanSpace(p.InnerText).Length)
                        lines.Enqueue(new Rich_PlainText(p.InnerText));
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
            return lines;
        }

        public static bool IsUnderlined(Paragraph para)
        {
            foreach (Run run in para.ChildElements.OfType<Run>())
            {
                if (IsBoldItalicUnderline(run))
                    return true;
            }
            return false;
        }

        public static bool IsBoldItalicUnderline(Run run)
        {
            if (run.RunProperties == null ||
                run.RunProperties.Underline == null || run.RunProperties.Underline.Val == UnderlineValues.None)
                return false;
            return true;
        }
    }
}
