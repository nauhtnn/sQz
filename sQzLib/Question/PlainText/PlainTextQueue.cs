using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Packaging;
namespace sQzLib
{
    public class PlainTextQueue
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
                        token.Append(s.Substring(0, s.Length - 2));
					break;
				}
				else
					token.Append(lines.Dequeue());
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
				string s;
				if (0 < (s = Utils.CleanSpace(line)).Length)
					lines.Enqueue(s);
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
					string s;
                    if (0 < (s = Utils.CleanSpace(p.InnerText)).Length)
                        lines.Enqueue(s);
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
    }
}
