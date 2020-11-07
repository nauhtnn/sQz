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
		public Queue<string> GetTextQueue(string filePath)
		{
			Queue<string> lines = ReadTrimLines(filePath);
            Queue<string> tokens = new Queue<string>();
			while(lines.Count > 0)
			{
				if(lines[0].First() != '{')
					tokens.add(lines.pop(0));
				else if(lines[0].charAt(lines[0].Length - 1) == '}')
					tokens.add(lines.pop(0).Substring(1, lines[0].Length - 1));
				else
					tokens.add(CreateTokenFromLines(lines));
			}
			return tokens;
		}
		
		string CreateTokenFromLines(List<string> lines)
		{
			StringBuilder token = new StringBuilder();
			token.append(lines.pop(0).Substring(1));
			while(!lines.IsEmpty())
			{
				if(lines[0].charAt(lines[0].Length) == '}')
				{
					token.append(lines.pop(0).Substring(-1));
					break;
				}
				else
					token.append(lines.pop(0));
			}
			return token.ToString();
		}
		
		static Queue<string> ReadTrimLines(string filePath)
		{
			if(System.IO.Path.GetExtension(filePath) == "docx")
				return ReadTrimDocx(filePath);
			else
				return ReadTrimTxt(filePath);
		}
		
		static Queue<string> ReadTrimTxt(string filePath)
		{
            Queue<string> lines = new Queue<string>();
			string[] rawLines = System.IO.File.ReadAllLines(filePath);
			foreach(string line in rawLines)
			{
				string s;
				if (0 < (s = Utils.CleanSpace(line)).Length)
					lines.Add(s);
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
            catch(OpenXmlPackageException err)
            {
                //messagebox(err);
                return lines;
            }
            catch (System.IO.IOException err)
            {
				//messagebox(err);
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
