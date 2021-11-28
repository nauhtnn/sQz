using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sQzLib
{
    public class Txt
    {
        List<string> avEnum;
        List<string> avTxt;
        byte[] mBuf;
        int mSz;
        public string[] __;

        public string _(int str_idx)
        {
            if (__ != null && __.Length > str_idx && __[str_idx] != null)
                return __[str_idx];
            return "NO STRING";
        }

        static Txt _s = null;

        public Txt()
        {
            avEnum = new List<string>();
            avTxt = new List<string>();
            mBuf = null;
            __ = null;
            mSz = 0;
        }
		
		public static Txt s{
			get{
				if(_s == null) {
					_s = new Txt();
                    if (!_s.ReadByte("GUI-vi.bin"))
                        _s.ReadTextFile("GUI-vi.txt");
				}
				return _s;
			}
		}

        public void Scan(string f)
        {
            if (f == null)
                return;
            avEnum.Clear();
            avTxt.Clear();
            string[] sep = { "\r\n", "\n" };
            string[] vs = f.Split(sep, StringSplitOptions.None);
            mSz = 4;
            int i = -1;
            foreach (string s in vs)
            {
                string[] vt = s.Split('\t');
                if (vt.Length == 2)
                {
                    if (vt[0].Length == 0)
                    {
                        avTxt[i] += '\n' + vt[1];
                        mSz += 4;
                    }
                    else
                    {
                        avEnum.Add(vt[0]);
                        avTxt.Add(vt[1]);
                        mSz += 4;
                        ++i;
                    }
                    mSz += vt[1].Length * 4;
                }
            }
        }

        public void WriteEnum(string fp)
        {
            if (avEnum.Count < 1)
                return;
            StringBuilder sb = new StringBuilder();
            sb.Append("namespace sQzLib {\n");
            sb.Append("\tpublic enum TxI {\n");
            foreach (string s in avEnum)
                sb.Append("\t\t" + s + ",\n");
            sb.Append("\t}\n}\n");
            System.IO.File.WriteAllText(fp, sb.ToString());
        }

        public void WriteByte(string fp)
        {
            if (avTxt.Count < 1)
                return;
            Console.WriteLine(mSz + " " + avTxt.Count);
            mBuf = new byte[mSz];
            int offs = 0;
            Buffer.BlockCopy(BitConverter.GetBytes(avTxt.Count), 0, mBuf, offs, 4);
            offs += 4;
            foreach (string s in avTxt)
            {
                byte[] b = Encoding.UTF8.GetBytes(s);
                Buffer.BlockCopy(BitConverter.GetBytes(b.Length), 0, mBuf, offs, 4);
                offs += 4;
                Buffer.BlockCopy(b, 0, mBuf, offs, b.Length);
                offs += b.Length;
            }
            System.IO.File.WriteAllBytes(fp, mBuf);
        }

        public bool ReadTextFile(string fp)
        {
            string content;
            try
            {
                content = System.IO.File.ReadAllText(fp);
            }
            catch (System.IO.FileNotFoundException)
            {
                return false;
            }

            Scan(content);

            if (avEnum.Count == 0)
                return false;

            __ = new string[avEnum.Count];


            for(int i = 0; i < avEnum.Count; ++i)
            {
                TxI index = 0;
                if (Enum.TryParse<TxI>(avEnum[i], out index))
                    __[(int)index] = avTxt[i];
                else
                    __[i] = "NO STRING";
            }

            return true;
        }

        public bool ReadByte(string fp)
        {

            byte[] mBuf;
            try
            {
                mBuf = System.IO.File.ReadAllBytes(fp);
            }
            catch(System.IO.FileNotFoundException)
            {
                return false;
            }
            int offs = 0;
            int n = BitConverter.ToInt32(mBuf, offs);
            offs += 4;
            __ = new string[n];
            for (int i = 0; i < n; ++i)
            {
                int l = BitConverter.ToInt32(mBuf, offs);
                offs += 4;
                __[i] = Encoding.UTF8.GetString(mBuf, offs, l);
                offs += l;
            }
            //int j = -1;
            //foreach(string s in _)
            //	Console.WriteLine("" + ++j + ") " + s);
            return true;
        }
    }
}
