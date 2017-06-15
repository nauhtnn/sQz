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
            l.Add(BitConverter.GetBytes((int)eLv));
            l.Add(BitConverter.GetBytes(uId));
            l.Add(BitConverter.GetBytes((int)eStt));
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

            if (eStt < ExamStt.Finished)
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
            //suppose eStt == ExamStt.Finished
            int l = buf.Length - offs;

            if (l < 12)
                return true;

            int lv;
            if (Enum.IsDefined(typeof(ExamLv), lv = BitConverter.ToInt32(buf, offs)))
                eLv = (ExamLv)lv;
            else
                return true;
            l -= 4;
            offs += 4;

            uId = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;

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
                dtTim1 = DT.INV_;
                return true;
            }

            if (l < AnsSheet.LEN + 4)
                return true;
            mAnsSh.uQSId = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            mAnsSh.aAns = new byte[AnsSheet.LEN];
            Array.Copy(buf, offs, mAnsSh.aAns, 0, AnsSheet.LEN);
            l -= AnsSheet.LEN;
            offs += AnsSheet.LEN;

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
                dtTim2 = DT.INV_;
                return true;
            }

            if (l < 4)
                return true;
            uGrade = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;

            return false;
        }

        public override void Merge(ExamineeA e)
        {
            if (eStt == ExamStt.Finished)
                return;
            //suppose eStt = eINFO and e.eStt = ExamStt.Finished
            tComp = e.tComp;
            mAnsSh = e.mAnsSh;
            dtTim1 = e.dtTim1;
            uGrade = e.uGrade;
            dtTim2 = e.dtTim2;
        }
    }
}
