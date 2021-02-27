using System;
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
            Utils.AppendBytesOfString(ID, l);
            l.Add(BitConverter.GetBytes((int)eStt));

            Utils.AppendBytesOfString(Birthdate, l);
            Utils.AppendBytesOfString(Name, l);
            Utils.AppendBytesOfString(Birthplace, l);

            if (eStt < NeeStt.Finished)
                return l;

            l.Add(DT.GetBytes(dtTim1));

            l.Add(DT.GetBytes(dtTim2));
            l.Add(BitConverter.GetBytes(Grade));
            if(0 < tComp.Length)
                Utils.AppendBytesOfString(tComp, l);
            else
                l.Add(BitConverter.GetBytes(0));

            return l;
        }

        public override bool ReadByte(byte[] buf, ref int offs)
        {
            //suppose eStt == NeeStt.Finished
            int l = buf.Length - offs;
            //
            if (l < 4)
                return true;
            int x = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            //
            if (l < x || x < 1)
                return true;
            ID = Encoding.UTF8.GetString(buf, offs, x);
            l -= x;
            offs += x;

            if (l < 4)
                return true;

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
                dtTim1 = DT.INVALID;
                return true;
            }
            mAnsSh.questSheetID = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            mAnsSh.aAns = new byte[AnsSheet.LEN];
            Array.Copy(buf, offs, mAnsSh.aAns, 0, AnsSheet.LEN);
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
                dtTim2 = DT.INVALID;
                return true;
            }
            Grade = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            //
            return false;
        }

        public override void Merge(ExamineeA e)
        {
            if (eStt == NeeStt.Finished)
                return;
            //suppose eStt = eINFO and e.eStt = NeeStt.Finished
            eStt = NeeStt.Finished;
            bToVw = bToDB = true;
            tComp = e.tComp;
            mAnsSh = e.mAnsSh;
            dtTim1 = e.dtTim1;
            Grade = e.Grade;
            dtTim2 = e.dtTim2;
        }
    }
}
