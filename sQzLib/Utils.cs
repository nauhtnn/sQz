using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Packaging;
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
        static char[] sWhSp = { ' ', '\t', '\n', '\r' };
		
		public static List<string> ReadQuestionTokens(string fileName)
		{
			List<string> lines = ReadTrimLines(fileName);
			List<string> tokens = new List<string>();
			while(!lines.IsEmpty())
			{
				if(lines[0].charAt(0) != '{')
					tokens.add(lines.pop(0));
				else if(lines[0].charAt(lines[0].Length - 1) == '}')
					tokens.add(lines.pop(0).Substring(1, lines[0].Length - 1));
				else
					tokens.add(CreateTokenFromLines(lines));
			}
			return tokens;
		}
		
		static string CreateTokenFromLines(List<string> lines)
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
		
		static List<string> ReadTrimLines(string fileName)
		{
			if(path.getFileExtension(fileName) == "docx")
				return ReadTrimDocx(fileName);
			else
				return ReadTrimTxt(fileName);
		}
		
		static List<string> ReadTrimTxt(string fileName)
		{
			List<string> lines = new List<string>();
			string[] rawLines = System.IO.File.ReadAllLines(fileName);
			foreach(string line in rawLines)
			{
				string s;
				if (0 < (s = CleanSpace(line)).Length)
					lines.Add(s);
			}
			return lines;
		}

        static List<string> ReadTrimDocx(string fpath)
        {
            List<string> lines = new List<string>();
            WordprocessingDocument doc = null;
            try
            {
                doc = WordprocessingDocument.Open(fpath, false);
            }
            catch(OpenXmlPackageException err)
            {
				messagebox(err);
                return lines;
            }
            catch (System.IO.IOException err)
            {
				messagebox(err);
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
                    if (0 < (s = CleanSpace(p.InnerText)).Length)
                        lines.Add(s);
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
