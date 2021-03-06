﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sQzLib
{
    public sealed class ExamineeS1: ExamineeA
    {
        bool bNRecd;
        public bool NRecd { get { return bNRecd; } }
        public ExamineeS1() {
            bNRecd = true;
        }

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
            l.Add(BitConverter.GetBytes((int)mPhase));
            if (mPhase == ExamineePhase.Finished)
                l.Add(BitConverter.GetBytes(uGrade));

            if (mPhase < ExamineePhase.Finished || bLog)
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
            return false;
            //int l = buf.Length - offs;
            ////
            //if (l < 12)
            //    return true;
            //int x = BitConverter.ToInt32(buf, offs);
            //l -= 4;
            //offs += 4;
            //if (ParseLvId(x))
            //    return true;
            //if (Enum.IsDefined(typeof(NeeStt), x = BitConverter.ToInt32(buf, offs)))
            //    eStt = (NeeStt)x;
            //l -= 4;
            //offs += 4;
            //bLog = BitConverter.ToBoolean(buf, offs);
            //l -= 1;
            //offs += 1;
            ////
            //if (eStt < NeeStt.Examing || bLog)
            //{
            //    if (l < 4)
            //        return true;
            //    int sz = BitConverter.ToInt32(buf, offs);
            //    l -= 4;
            //    offs += 4;
            //    if (l < sz + 4)
            //        return true;
            //    tBirdate = Encoding.UTF8.GetString(buf, offs, sz);
            //    l -= sz;
            //    offs += sz;
            //    sz = BitConverter.ToInt32(buf, offs);
            //    l -= 4;
            //    offs += 4;
            //    if (l < sz)
            //        return true;
            //    tComp = Encoding.UTF8.GetString(buf, offs, sz);
            //    l -= sz;
            //    offs += sz;
            //}

            //if (eStt < NeeStt.Examing)
            //    return false;

            //if (l < 4)
            //    return true;
            //mAnsSh.uQSLvId = BitConverter.ToInt32(buf, offs);
            //l -= 4;
            //offs += 4;

            //if (eStt < NeeStt.Submitting)
            //    return false;

            //if (l < AnsSheet.LEN)
            //    return true;
            //mAnsSh.aAns = new byte[AnsSheet.LEN];
            //Buffer.BlockCopy(buf, offs, mAnsSh.aAns, 0, AnsSheet.LEN);
            //l -= AnsSheet.LEN;
            //offs += AnsSheet.LEN;

            //return false;
        }

        public bool ReadByteS(byte[] buf, ref int offs)
        {
            int l = buf.Length - offs;
            //
            if (l < 20)
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

            if (Enum.IsDefined(typeof(ExamineePhase), x = BitConverter.ToInt32(buf, offs)))
                mPhase = (ExamineePhase)x;
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
            if (mPhase < ExamineePhase.Finished)
                return false;
            bNRecd = false;
            //
            if (l < 24)
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
            sz = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            if(0 < sz)
            {
                tComp = Encoding.UTF8.GetString(buf, offs, sz);
                l -= sz;
                offs += sz;
            }
            //
            return false;
        }

        public List<byte[]> ToByteS()
        {
            //suppose eStt == NeeStt.Finished
            List<byte[]> l = new List<byte[]>();
            l.Add(BitConverter.GetBytes((int)Lv));
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
            l.Add(BitConverter.GetBytes(mAnsSheet.uQSId));
            l.Add(mAnsSheet.aAns);
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
            if (mPhase == ExamineePhase.Finished)
                return;
            bLog = e.bLog;
            if (mPhase < ExamineePhase.Examing || bLog)
                tComp = e.tComp;
            if (e.mPhase < ExamineePhase.Examing)
                mPhase = ExamineePhase.Examing;
            else
                mPhase = e.mPhase;
            if (mPhase < ExamineePhase.Examing)
                return;
            mAnsSheet = new AnsSheet();
            mAnsSheet.uQSLvId = e.mAnsSheet.uQSLvId;

            if (mPhase < ExamineePhase.Submitting)
                return;
            mAnsSheet.aAns = e.mAnsSheet.aAns;
        }

        public void MergeS(ExamineeA e)
        {
            //suppose e.eStt = NeeStt.Finished
            mPhase = e.mPhase;
            dtTim1 = e.dtTim1;
            dtTim2 = e.dtTim2;
            uGrade = e.uGrade;
        }
    }
}
