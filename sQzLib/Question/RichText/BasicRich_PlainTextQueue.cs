using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Packaging;
using System.Windows.Controls;

namespace sQzLib
{
    public class BasicRich_PlainTextQueue
    {
		public static Queue<BasicRich_PlainText> GetTextQueue(string filePath)
		{
			Queue<BasicRich_PlainText> lines = ReadTrimLines(filePath);
            Queue<BasicRich_PlainText> tokens = new Queue<BasicRich_PlainText>();
			while(lines.Count > 0)
			{
                if (lines.Peek().ElementAt(0) != '{')
                {
                    if(lines.Peek().ElementAt(0) == '\\' &&
                        lines.Peek().Length > 1)
                    {
                        tokens.Enqueue(lines.Dequeue().Substring(1));
                    }
                    else
                        tokens.Enqueue(lines.Dequeue());
                }
                else if(lines.Peek().ElementAt(lines.Peek().Length - 1) == '}')
                {
                    if(lines.Peek().Length > 2)
                    {
                        BasicRich_PlainText token = lines.Dequeue();
                        tokens.Enqueue(token.Substring(1, token.Length - 2));
                    }
                }
                else
					tokens.Enqueue(JoinLinesTo1Token(lines));
			}
			return tokens;
		}
		
		static BasicRich_PlainText JoinLinesTo1Token(Queue<BasicRich_PlainText> lines)
		{
			BasicRich_PlainText token = lines.Dequeue().Substring(1);
			while(lines.Count > 0)
			{
				if(lines.Peek().ElementAt(lines.Peek().Length - 1) == '}')
				{
                    BasicRich_PlainText s = lines.Dequeue();
                    if (s.Length > 1)
                        token.AppendNewParagraphs(s.Substring(0, s.Length - 1));
					break;
				}
				else if (lines.Peek().ElementAt(lines.Peek().Length - 1) == '\\' &&
                    lines.Peek().Length > 1)
                {
                    BasicRich_PlainText s = lines.Dequeue();
                    token.AppendNewParagraphs(s.Substring(0, s.Length - 1));
                }
                else
                    token.AppendNewParagraphs(lines.Dequeue());
			}
			return token;
		}
		
		static Queue<BasicRich_PlainText> ReadTrimLines(string filePath)
		{
			if(System.IO.Path.GetExtension(filePath) == ".docx")
				return ReadTrimDocx(filePath);
			else
				return ReadTrimTxt(filePath);
		}
		
		static Queue<BasicRich_PlainText> ReadTrimTxt(string filePath)
		{
            Queue<BasicRich_PlainText> lines = new Queue<BasicRich_PlainText>();
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
					lines.Enqueue(new BasicRich_PlainText(line));
			}
			return lines;
		}

        static Queue<BasicRich_PlainText> ReadTrimDocx(string fpath)
        {
            Queue<BasicRich_PlainText> lines = new Queue<BasicRich_PlainText>();
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
                        RichTextBox richText = new RichTextBox();
                        System.Windows.Documents.Paragraph para = new System.Windows.Documents.Paragraph();
                        foreach(Run run in p.ChildElements.OfType<Run>())
                        {
                            System.Windows.Documents.Run richText_run = new System.Windows.Documents.Run(run.InnerText);
                            if (IsBoldItalicUnderline(run))
                                richText_run.TextDecorations.Add(System.Windows.TextDecorations.Underline);
                            para.Inlines.Add(richText_run);
                        }
                        richText.Document.Blocks.Add(para);
                        lines.Enqueue(new BasicRich_PlainText(richText));
                    }
                    else if(0 < Utils.CleanSpace(p.InnerText).Length)
                        lines.Enqueue(new BasicRich_PlainText(p.InnerText));
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
