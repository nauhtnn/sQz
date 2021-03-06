﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sQzLib
{
    public sealed class ExamineeS0: ExamineeA
    {
        public bool bToDB;
        public bool bToVw;
        public ExamineeS0()
        {
            bToVw = bToDB = false;
        }

        public override List<byte[]> ToByte()
        {
            List<byte[]> l = new List<byte[]>();
            l.Add(BitConverter.GetBytes((int)Lv));
            l.Add(BitConverter.GetBytes(uId));
            l.Add(BitConverter.GetBytes((int)mPhase));
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

            if (mPhase < ExamineePhase.Finished)
                return l;

            l.Add(BitConverter.GetBytes(dtTim1.Hour));
            l.Add(BitConverter.GetBytes(dtTim1.Minute));

            l.Add(BitConverter.GetBytes(dtTim2.Hour));
            l.Add(BitConverter.GetBytes(dtTim2.Minute));
            l.Add(BitConverter.GetBytes(uGrade));
            if(0 < tComp.Length)
            {
                byte[] x = Encoding.UTF8.GetBytes(tComp);
                l.Add(BitConverter.GetBytes(x.Length));
                l.Add(x);
            }
            else
                l.Add(BitConverter.GetBytes(0));

            return l;
        }

        public override bool ReadByte(byte[] buf, ref int offs)
        {
            //suppose eStt == NeeStt.Finished
            int l = buf.Length - offs;
            //
            if (l < 12)
                return true;
            int x;
            if (Enum.IsDefined(typeof(Level), x = BitConverter.ToInt32(buf, offs)))
                Lv = (Level)x;
            else
                return true;
            l -= 4;
            offs += 4;
            uId = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            x = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            //
            if (l < x)
                return true;
            if (0 < x)
            {
                tComp = Encoding.UTF8.GetString(buf, offs, x);
                l -= x;
                offs += x;
            }
            //
            if (l < AnsSheet.LEN + 24)
                return true;
            int h = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            int m = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            if (!DateTime.TryParse(h.ToString() + ':' + m, out dtTim1))
            {
                dtTim1 = DT.INV_;
                return true;
            }
            mAnsSheet.uQSLvId = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            mAnsSheet.aAns = new byte[AnsSheet.LEN];
            Array.Copy(buf, offs, mAnsSheet.aAns, 0, AnsSheet.LEN);
            l -= AnsSheet.LEN;
            offs += AnsSheet.LEN;

            h = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            m = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            if (!DateTime.TryParse(h.ToString() + ':' + m, out dtTim2))
            {
                dtTim2 = DT.INV_;
                return true;
            }
            uGrade = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            //
            return false;
        }

        public override void Merge(ExamineeA e)
        {
            if (mPhase == ExamineePhase.Finished)
                return;
            //suppose eStt = eINFO and e.eStt = NeeStt.Finished
            mPhase = ExamineePhase.Finished;
            bToVw = bToDB = true;
            tComp = e.tComp;
            mAnsSheet = e.mAnsSheet;
            dtTim1 = e.dtTim1;
            uGrade = e.uGrade;
            dtTim2 = e.dtTim2;
        }
    }
}
