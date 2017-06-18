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
            l.Add(BitConverter.GetBytes((int)eStt));
            if (eStt == ExamStt.Finished)
                l.Add(BitConverter.GetBytes(uGrade));

            if (eStt < ExamStt.Finished || bLog)
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
            //
            if (l < 12)
                return true;
            int x = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            if (ParseLvId(x))
                return true;
            if (Enum.IsDefined(typeof(ExamStt), x = BitConverter.ToInt32(buf, offs)))
                eStt = (ExamStt)x;
            l -= 4;
            offs += 4;
            bLog = BitConverter.ToBoolean(buf, offs);
            l -= 1;
            offs += 1;
            //
            if (eStt < ExamStt.Examing || bLog)
            {
                if (l < 4)
                    return true;
                int sz = BitConverter.ToInt32(buf, offs);
                l -= 4;
                offs += 4;
                if (l < sz + 4)
                    return true;
                tBirdate = Encoding.UTF8.GetString(buf, offs, sz);
                l -= sz;
                offs += sz;
                sz = BitConverter.ToInt32(buf, offs);
                l -= 4;
                offs += 4;
                if (l < sz)
                    return true;
                tComp = Encoding.UTF8.GetString(buf, offs, sz);
                l -= sz;
                offs += sz;
            }

            if (eStt < ExamStt.Examing)
                return false;

            if (l < 4)
                return true;
            mAnsSh.uQSId = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;

            if (eStt < ExamStt.Submitting)
                return false;

            if (l < AnsSheet.LEN)
                return true;
            mAnsSh.aAns = new byte[AnsSheet.LEN];
            Buffer.BlockCopy(buf, offs, mAnsSh.aAns, 0, AnsSheet.LEN);
            l -= AnsSheet.LEN;
            offs += AnsSheet.LEN;

            return false;
        }

        public bool ReadByteS(byte[] buf, ref int offs)
        {
            int l = buf.Length - offs;
            //
            if (l < 20)
                return true;

            int x;
            if (Enum.IsDefined(typeof(ExamLv), x = BitConverter.ToInt32(buf, offs)))
                eLv = (ExamLv)x;
            else
                return true;
            l -= 4;
            offs += 4;

            uId = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;

            if (Enum.IsDefined(typeof(ExamStt), x = BitConverter.ToInt32(buf, offs)))
                eStt = (ExamStt)x;
            l -= 4;
            offs += 4;

            int sz = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            //
            if (l < sz + 4)
                return true;
            if (0 < sz)
            {
                tBirdate = Encoding.UTF8.GetString(buf, offs, sz);
                l -= sz;
                offs += sz;
            }
            sz = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            //
            if (l < sz + 4)
                return true;
            if (0 < sz)
            {
                tName = Encoding.UTF8.GetString(buf, offs, sz);
                l -= sz;
                offs += sz;
            }
            sz = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            //
            if (l < sz)
                return true;
            if (0 < sz)
            {
                tBirthplace = Encoding.UTF8.GetString(buf, offs, sz);
                l -= sz;
                offs += sz;
            }
            if (eStt < ExamStt.Finished)
                return false;
            //
            if (l < 20)
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

        public List<byte[]> ToByteS()
        {
            //suppose eStt == ExamStt.Finished
            List<byte[]> l = new List<byte[]>();
            l.Add(BitConverter.GetBytes((int)eLv));
            l.Add(BitConverter.GetBytes(uId));
            if (0 < tComp.Length)
            {
                byte[] x = Encoding.UTF8.GetBytes(tComp);
                l.Add(BitConverter.GetBytes(x.Length));
                l.Add(x);
            }
            else
                l.Add(BitConverter.GetBytes(0));
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
            if (eStt == ExamStt.Finished)
                return;
            bLog = e.bLog;
            if (eStt < ExamStt.Examing || bLog)
                tComp = e.tComp;
            if (e.eStt < ExamStt.Examing)
                eStt = ExamStt.Examing;
            else
                eStt = e.eStt;
            if (eStt < ExamStt.Examing)
                return;
            mAnsSh = new AnsSheet();
            mAnsSh.uQSId = e.mAnsSh.uQSId;

            if (eStt < ExamStt.Submitting)
                return;
            mAnsSh.aAns = e.mAnsSh.aAns;
        }

        public void MergeS(ExamineeA e)
        {
            //suppose e.eStt = ExamStt.Finished
            eStt = e.eStt;
            dtTim1 = e.dtTim1;
            dtTim2 = e.dtTim2;
            uGrade = e.uGrade;
        }
    }
}
