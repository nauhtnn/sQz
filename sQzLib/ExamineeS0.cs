using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sQzLib
{
    public sealed class ExamineeS0: ExamineeA
    {
        public ExamineeS0() { }

        public override List<byte[]> ToByte()
        {
            List<byte[]> l = new List<byte[]>();
            l.Add(BitConverter.GetBytes(uSlId));
            l.Add(BitConverter.GetBytes(Lv));
            l.Add(BitConverter.GetBytes(uId));
            l.Add(BitConverter.GetBytes(eStt));
            byte[] b;

            b = Encoding.UTF8.GetBytes(tBirdate);
            l.Add(BitConverter.GetBytes(b.Length));
            l.Add(b);

            b = Encoding.UTF8.GetBytes(tName);
            l.Add(BitConverter.GetBytes(b.Length));
            l.Add(b);
            b = Encoding.UTF8.GetBytes(tBirthplace);
            l.Add(BitConverter.GetBytes(b.Length));
            l.Add(b);

            if (eStt < eFINISHED)
                return l;

            l.Add(BitConverter.GetBytes(dtTim1.Hour));
            l.Add(BitConverter.GetBytes(dtTim1.Minute));

            l.Add(BitConverter.GetBytes(dtTim2.Hour));
            l.Add(BitConverter.GetBytes(dtTim2.Minute));
            l.Add(BitConverter.GetBytes(uGrade));

            return l;
        }

        public override bool ReadByte(byte[] buf, ref int offs)
        {
            //suppose eStt == eFINISHED
            int l = buf.Length - offs;

            if (l < 8)
                return true;
            uSlId = BitConverter.ToUInt32(buf, offs);
            l -= 4;
            offs += 4;

            Lv = BitConverter.ToInt16(buf, offs);
            l -= 2;
            offs += 2;

            uId = BitConverter.ToUInt16(buf, offs);
            l -= 2;
            offs += 2;

            int h = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            if (l < 4)
                return true;
            int m = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            if (!DateTime.TryParse(h.ToString() + ':' + m, out dtTim1))
            {
                dtTim1 = ExamSlot.INVALID_DT;
                return true;
            }

            if (l < 122)
                return true;
            mAnsSh.uQSId = BitConverter.ToUInt16(buf, offs);
            l -= 2;
            offs += 2;
            mAnsSh.aAns = new byte[120];
            Array.Copy(buf, offs, mAnsSh.aAns, 0, 120);
            l -= 120;
            offs += 120;

            h = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            if (l < 4)
                return true;
            m = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            if (!DateTime.TryParse(h.ToString() + ':' + m, out dtTim2))
            {
                dtTim2 = ExamSlot.INVALID_DT;
                return true;
            }

            if (l < 2)
                return true;
            uGrade = BitConverter.ToUInt16(buf, offs);
            l -= 2;
            offs += 2;

            return false;
        }

        public override void Merge(ExamineeA e)
        {
            if (eStt == eFINISHED)
                return;
            //suppose eStt = eINFO and e.eStt = eFINISHED
            tComp = e.tComp;
            mAnsSh = e.mAnsSh;
            dtTim1 = e.dtTim1;
            uGrade = e.uGrade;
            dtTim2 = e.dtTim2;
        }
    }
}
