using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
	public class Txt {
		List<string> avEnum;
		List<string> avTxt;
		byte[] mBuf;
		int mSz;
		string[] _;
		
		public Txt(){
			avEnum = new List<string>();
			avTxt = new List<string>();
			mBuf = null;
			_ = null;
			mSz = 0;
		}
		
		public void Scan(string f)
		{
			if(f == null)
				return;
			avEnum.Clear();
			avTxt.Clear();
			string[] vs = f.Split('\n');
			mSz = 4;
			int i = -1;
			foreach(string s in vs)
			{
				string[] vt = s.Split('\t');
				if(vt.Length == 2)
				{
					if(vt[0].Length == 0)
					{
						avTxt[i] += '\n' + vt[1];
						mSz += 4;
					}
					else
					{
						avTxt.Add(vt[1]);
						mSz += 4;
						++i;
					}
					mSz += vt[1].Length * 4;
				}
			}
		}
		
		public void WriteEnum(){
			if(avEnum.Count < 1)
				return;
			Console.WriteLine("namespace sQzLib {");
			Console.WriteLine("\tpublic enum TxI {");
			foreach(string s in avEnum)
				Console.WriteLine("\t\t" + s + ',');
			Console.WriteLine("\t}\n}");
		}
		
		public void WriteByte()
		{
			if(avTxt.Count < 1)
				return;
			Console.WriteLine(mSz + " " + avTxt.Count);
			mBuf = new byte[mSz];
			int offs = 0;
			Buffer.BlockCopy(BitConverter.GetBytes(avTxt.Count), 0, mBuf, offs, 4);
			offs += 4;
			foreach(string s in avTxt)
			{
				byte[] b = Encoding.UTF32.GetBytes(s);
				Buffer.BlockCopy(BitConverter.GetBytes(b.Length), 0, mBuf, offs, 4);
				offs += 4;
				Buffer.BlockCopy(b, 0, mBuf, offs, b.Length);
				offs += b.Length;
			}
		}

		public void ReadByte()
		{
			int offs = 0;
			int n = BitConverter.ToInt32(mBuf, offs);
			offs += 4;
			_ = new string[n];
			for(int i = 0; i < n; ++i) {
				int l = BitConverter.ToInt32(mBuf, offs);
				offs += 4;
				_[i] = Encoding.UTF32.GetString(mBuf, offs, l);
				offs += l;
			}
			int j = -1;
			foreach(string s in _)
				Console.WriteLine("" + ++j + ") " + s);
		}
	}
	
	class Program
	{
		public static string ReadFile(string fileName)
		{
			try
			{
				if (!System.IO.File.Exists(fileName))
					return null;
				return System.IO.File.ReadAllText(fileName);
			}
			catch (System.Exception)
			{
				System.Console.WriteLine("Cannot read file '{0}'", fileName);
				return null;
			}
		}
		
		public static void Main(string[] args) {
			Txt p = new Txt();
			p.Scan(ReadFile("GUI-vi.txt"));
			p.WriteEnum();
			p.WriteByte();
			p.ReadByte();
		}
	}
}
