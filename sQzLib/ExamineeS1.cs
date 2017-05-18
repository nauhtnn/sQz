using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sQzLib
{
    public sealed class ExamineeS1: ExamineeA
    {
        public ExamineeS1() { }

        public override List<byte[]> ToByte()
        {
            if (bFromC)
                return ToByteC();
            return ToByteS();
        }

        public override bool ReadByte(byte[] buf, ref int offs)
        {
            if (bFromC)
                return ReadByteC(buf, ref offs);
            return ReadByteS(buf, ref offs);
        }

        public List<byte[]> ToByteC()
        {
            List<byte[]> l = new List<byte[]>();
            l.Add(BitConverter.GetBytes(eStt));
            if (eStt == eFINISHED)
                l.Add(BitConverter.GetBytes(uGrade));

            if (eStt < eFINISHED || bLog)
            {
                byte[] b = Encoding.UTF8.GetBytes(tBirdate);
                l.Add(BitConverter.GetBytes(b.Length));
                l.Add(b);

                b = Encoding.UTF8.GetBytes(tName);
                l.Add(BitConverter.GetBytes(b.Length));
                l.Add(b);

                b = Encoding.UTF8.GetBytes(tBirthplace);
                l.Add(BitConverter.GetBytes(b.Length));
                l.Add(b);
            }

            bLog = false;

            return l;
        }

        public bool ReadByteC(byte[] buf, ref int offs)
        {
            int l = buf.Length - offs;

            if (l < 13)
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

            eStt = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;

            bLog = BitConverter.ToBoolean(buf, offs);
            l -= 1;
            offs += 1;

            if (eStt < eEXAMING || bLog)
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
                tComp = Encoding.UTF8.GetString(buf, offs, sz);
                l -= sz;
                offs += sz;
            }

            if (eStt < eEXAMING)
                return false;

            if (l < 2)
                return true;
            mAnsSh.uQSId = BitConverter.ToUInt16(buf, offs);
            l -= 2;
            offs += 2;

            if (eStt < eSUBMITTING)
                return false;

            if (l < 120)
                return true;
            mAnsSh.aAns = new byte[120];//hardcode
            Buffer.BlockCopy(buf, offs, mAnsSh.aAns, 0, 120);
            l -= 120;
            offs += 120;

            return false;
        }

        public bool ReadByteS(byte[] buf, ref int offs)
        {
            int l = buf.Length - offs;

            if (l < 12)
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

            eStt = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;

            if (l < 4)
                return true;
            int sz = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            if (l < sz)
                return true;
            if (0 < sz)
            {
                tBirdate = Encoding.UTF8.GetString(buf, offs, sz);
                l -= sz;
                offs += sz;
            }

            if (l < 4)
                return true;
            sz = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            if (l < sz)
                return true;
            if (0 < sz)
            {
                tName = Encoding.UTF8.GetString(buf, offs, sz);
                l -= sz;
                offs += sz;
            }

            if (l < 4)
                return true;
            sz = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            if (l < sz)
                return true;
            if (0 < sz)
            {
                tBirthplace = Encoding.UTF8.GetString(buf, offs, sz);
                l -= sz;
                offs += sz;
            }

            if (eStt < eFINISHED)
                return false;

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

        public List<byte[]> ToByteS()
        {
            //suppose eStt == eFINISHED
            List<byte[]> l = new List<byte[]>();
            l.Add(BitConverter.GetBytes(uSlId));
            l.Add(BitConverter.GetBytes(Lv));
            l.Add(BitConverter.GetBytes(uId));
            l.Add(BitConverter.GetBytes(dtTim1.Hour));
            l.Add(BitConverter.GetBytes(dtTim1.Minute));

            l.Add(BitConverter.GetBytes(mAnsSh.uQSId));

            l.Add(mAnsSh.aAns);

            l.Add(BitConverter.GetBytes(dtTim2.Hour));
            l.Add(BitConverter.GetBytes(dtTim2.Minute));
            l.Add(BitConverter.GetBytes(uGrade));
            return l;
        }

        public override void Merge(ExamineeA e)
        {
            if (bFromC)
                MergeC(e);
            else
                MergeS(e);
        }

        public void MergeC(ExamineeA e)
        {
            if (eStt == eFINISHED)
                return;
            if (e.eStt < eEXAMING)
                eStt = eEXAMING;
            else
                eStt = e.eStt;
            bLog = e.bLog;
            if (eStt < eEXAMING || bLog)
                tComp = e.tComp;
            if (eStt < eEXAMING)
                return;
            mAnsSh = new AnsSheet();
            mAnsSh.uQSId = e.mAnsSh.uQSId;

            if (eStt < eSUBMITTING)
                return;
            mAnsSh.aAns = e.mAnsSh.aAns;
        }

        public void MergeS(ExamineeA e)
        {
            //suppose e.eStt = eFINISHED
            eStt = e.eStt;
            dtTim1 = e.dtTim1;
            dtTim2 = e.dtTim2;
            uGrade = e.uGrade;
        }
    }
}
