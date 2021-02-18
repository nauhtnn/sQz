using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sQzLib
{
    public sealed class ExamineeC : ExamineeA
    {
        public ExamineeC() { }
        public override List<byte[]> ToByte()
        {
            List<byte[]> l = new List<byte[]>();
            byte[] b = Encoding.UTF8.GetBytes(ID);
            l.Add(BitConverter.GetBytes(b.Length));
            l.Add(b);
            l.Add(BitConverter.GetBytes((int)eStt));
			l.Add(BitConverter.GetBytes(bLog));

            if (eStt < NeeStt.Examing || bLog)
            {
                b = Encoding.UTF8.GetBytes(Birthdate);
                l.Add(BitConverter.GetBytes(b.Length));
                l.Add(b);

                b = Encoding.UTF8.GetBytes(tComp);
                l.Add(BitConverter.GetBytes(b.Length));
                l.Add(b);
            }

            if (eStt < NeeStt.Examing)
                return l;

            l.Add(BitConverter.GetBytes(mAnsSh.questSheetID));

            if (eStt < NeeStt.Submitting)
                return l;

            l.Add(mAnsSh.aAns);

            return l;
        }

        public override bool ReadByte(byte[] buf, ref int offs)
        {
            int l = buf.Length - offs;

            if (l < 4)
                return true;
            int x;
            if (Enum.IsDefined(typeof(NeeStt), x = BitConverter.ToInt32(buf, offs)))
                eStt = (NeeStt)x;
            l -= 4;
            offs += 4;

            if (eStt == NeeStt.Finished)
            {
                Grade = BitConverter.ToInt32(buf, offs);
                l -= 4;
                offs += 4;
            }

			if(eStt < NeeStt.Submitting || bLog)
			{
				if (l < 4)
					return true;
				int sz = BitConverter.ToInt32(buf, offs);
				l -= 4;
				offs += 4;
				if (l < sz)
					return true;
				Birthdate = Encoding.UTF8.GetString(buf, offs, sz);
				l -= sz;
				offs += sz;

				if (l < 4)
					return true;
				sz = BitConverter.ToInt32(buf, offs);
				l -= 4;
				offs += 4;
				if (l < sz)
					return true;
				Name = Encoding.UTF8.GetString(buf, offs, sz);
				l -= sz;
				offs += sz;

				if (l < 4)
					return true;
				sz = BitConverter.ToInt32(buf, offs);
				l -= 4;
				offs += 4;
				if (l < sz)
					return true;
				Birthplace = Encoding.UTF8.GetString(buf, offs, sz);
				l -= sz;
				offs += sz;
            }

            bLog = false;

            return false;
        }

        public override void Merge(ExamineeA e)
        {
            eStt = e.eStt;
            if (eStt == NeeStt.Finished)
                Grade = e.Grade;
            if (eStt < NeeStt.Finished || bLog)
            {
                Birthdate = e.Birthdate;
                Name = e.Name;
                Birthplace = e.Birthplace;
            }
            bLog = false;
        }
    }
}
