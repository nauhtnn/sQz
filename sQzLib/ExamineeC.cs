﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sQzLib
{
    public sealed class ExamineeC : ExamineeA
    {
        public ExamineeC():base() { }
        public ExamineeC(string id)
        {
            ParseLvID(id);
        }
        public override List<byte[]> ToByte()
        {
            List<byte[]> l = new List<byte[]>();
            l.Add(BitConverter.GetBytes(LvId));
            l.Add(BitConverter.GetBytes((int)mPhase));
			l.Add(BitConverter.GetBytes(bLog));
            byte[] b;

            if (mPhase < ExamineePhase.Examing || bLog)
            {
                b = Encoding.UTF8.GetBytes(tBirdate);
                l.Add(BitConverter.GetBytes(b.Length));
                l.Add(b);

                b = Encoding.UTF8.GetBytes(tComp);
                l.Add(BitConverter.GetBytes(b.Length));
                l.Add(b);
            }

            if (mPhase < ExamineePhase.Examing)
                return l;

            l.Add(BitConverter.GetBytes(mAnsSheet.uQSLvId));

            if (mPhase < ExamineePhase.Submitting)
                return l;

            l.Add(mAnsSheet.aAns);

            return l;
        }

        public override bool ReadByte(byte[] buf, ref int offs)
        {
            int l = buf.Length - offs;

            if (l < 4)
                return true;
            int x;
            if (Enum.IsDefined(typeof(ExamineePhase), x = BitConverter.ToInt32(buf, offs)))
                mPhase = (ExamineePhase)x;
            l -= 4;
            offs += 4;

            if (mPhase == ExamineePhase.Finished)
            {
                uGrade = BitConverter.ToInt32(buf, offs);
                l -= 4;
                offs += 4;
            }

			if(mPhase < ExamineePhase.Submitting || bLog)
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
            mPhase = e.mPhase;
            if (mPhase == ExamineePhase.Finished)
                uGrade = e.uGrade;
            if (mPhase < ExamineePhase.Finished || bLog)
            {
                tBirdate = e.tBirdate;
                tName = e.tName;
                tBirthplace = e.tBirthplace;
            }
            bLog = false;
        }
    }
}
