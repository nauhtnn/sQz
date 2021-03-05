using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Packaging;
using System.Windows.Controls;

namespace WpfApplication1
{
    public class BasicRich_PlainTextQueue
    {
		public static Queue<string> GetTextQueue(string filePath)
		{
			Queue<string> lines = ReadTrimLines(filePath);
            Queue<string> tokens = new Queue<string>();
			while(lines.Count > 0)
			{
                if (lines.Peek().ElementAt(0) != '{')
                    tokens.Enqueue(lines.Dequeue());
                else if (lines.Peek().ElementAt(lines.Peek().Length - 1) == '}')
                {
                    string s = lines.Dequeue();
                    if(s.Length > 2)
                        tokens.Enqueue(s.Substring(1, s.Length - 2));
                }
				else
					tokens.Enqueue(JoinLinesTo1Token(lines));
			}
			return tokens;
		}
		
		static string JoinLinesTo1Token(Queue<string> lines)
		{
			StringBuilder token = new StringBuilder();
			token.Append(lines.Dequeue().Substring(1));
			while(lines.Count > 0)
			{
				if(lines.Peek().ElementAt(lines.Peek().Length - 1) == '}')
				{
                    string s = lines.Dequeue();
                    if (s.Length > 1)
                        token.Append("\n" + s.Substring(0, s.Length - 1));
					break;
				}
				else
					token.Append("\n" + lines.Dequeue());
			}
			return token.ToString();
		}
		
		static Queue<string> ReadTrimLines(string filePath)
		{
			if(System.IO.Path.GetExtension(filePath) == ".docx")
				return ReadTrimDocx(filePath);
			else
				return ReadTrimTxt(filePath);
		}
		
		static Queue<string> ReadTrimTxt(string filePath)
		{
            Queue<string> lines = new Queue<string>();
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
					lines.Enqueue(line);
			}
			return lines;
		}

        static Queue<string> ReadTrimDocx(string fpath)
        {
            Queue<string> lines = new Queue<string>();
            WordprocessingDocument doc = null;
            try
            {
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
                    if(IsBoldItalicUnderline(p))
                    {
                        RichTextBox richText = new RichTextBox();
                        richText.Document = new System.Windows.Documents.FlowDocument();
                        foreach(Run run in p.ChildElements.OfType<Run>())
                        {
                            if(IsBoldItalicUnderline(run))
                            {
                                //System.Windows.Documents.Run richText_run = new System.Windows.Documents.Run(run.InnerText);
                                //richText_run.TextDecorations.Add(new System.Windows.TextDecorations())
                            }
                        }
                    }
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

        private static bool IsBoldItalicUnderline(Paragraph para)
        {
            foreach (Run run in para.ChildElements.OfType<Run>())
            {
                if (IsBoldItalicUnderline(run))
                    return true;
            }
            return false;
        }

        private static bool IsBoldItalicUnderline(Run run)
        {
            if (run.RunProperties.Underline == null || run.RunProperties.Underline.Val == UnderlineValues.None)
                return false;
            return true;
        }
    }
}
