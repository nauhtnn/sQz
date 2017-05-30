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
            //l.Add(BitConverter.GetBytes(mLv));
            l.Add(BitConverter.GetBytes(uId));
            l.Add(BitConverter.GetBytes(mStt));
			l.Add(BitConverter.GetBytes(bLog));
            byte[] b;

            if (eStt < ExamStt.Examing || bLog)
            {
                b = Encoding.UTF8.GetBytes(tBirdate);
                l.Add(BitConverter.GetBytes(b.Length));
                l.Add(b);

                b = Encoding.UTF8.GetBytes(tComp);
                l.Add(BitConverter.GetBytes(b.Length));
                l.Add(b);
            }

            if (eStt < ExamStt.Examing)
                return l;

            l.Add(BitConverter.GetBytes(mAnsSh.uQSId));

            if (eStt < ExamStt.Submitting)
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
            if (Enum.IsDefined(typeof(ExamStt), x = BitConverter.ToInt32(buf, offs)))
                eStt = (ExamStt)x;
            l -= 4;
            offs += 4;

            if (eStt == ExamStt.Finished)
            {
                uGrade = BitConverter.ToInt32(buf, offs);
                l -= 4;
                offs += 4;
            }

			if(eStt < ExamStt.Submitting || bLog)
			{
				if (l < 4)
					return true;
				int sz = BitConverter.ToInt32(buf, offs);
				l -= 4;
				offs += 4;
				if (l < sz)
					return true;
				tBirdate = Encoding.UTF8.GetString(buf, offs, sz);
				l -= sz;
				offs += sz;

				if (l < 4)
					return true;
				sz = BitConverter.ToInt32(buf, offs);
				l -= 4;
				offs += 4;
				if (l < sz)
					return true;
				tName = Encoding.UTF8.GetString(buf, offs, sz);
				l -= sz;
				offs += sz;

				if (l < 4)
					return true;
				sz = BitConverter.ToInt32(buf, offs);
				l -= 4;
				offs += 4;
				if (l < sz)
					return true;
				tBirthplace = Encoding.UTF8.GetString(buf, offs, sz);
				l -= sz;
				offs += sz;
            }

            bLog = false;

            return false;
        }

        public override void Merge(ExamineeA e)
        {
            eStt = e.eStt;
            if (eStt == ExamStt.Finished)
                uGrade = e.uGrade;
            if (eStt < ExamStt.Finished || bLog)
            {
                tBirdate = e.tBirdate;
                tName = e.tName;
                tBirthplace = e.tBirthplace;
            }
            bLog = false;
        }
    }
}
