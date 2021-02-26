using System;
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
            l.Add(BitConverter.GetBytes((int)eStt));
            if (eStt == NeeStt.Finished)
                l.Add(BitConverter.GetBytes(Grade));

            if (eStt < NeeStt.Finished || bLog)
            {
                byte[] b = Encoding.UTF8.GetBytes(Birthdate);
                l.Add(BitConverter.GetBytes(b.Length));
                l.Add(b);

                b = Encoding.UTF8.GetBytes(Name);
                l.Add(BitConverter.GetBytes(b.Length));
                l.Add(b);

                b = Encoding.UTF8.GetBytes(Birthplace);
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
            if (l < 4)
                return true;
            int sz = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;

            if (l < sz)
                return true;
            ID = Encoding.UTF8.GetString(buf, offs, sz);
            l -= sz;
            offs += sz;

            if (l < 4)
                return true;
            bLog = BitConverter.ToBoolean(buf, offs);
            l -= 1;
            offs += 1;
            //
            if (eStt < NeeStt.Examing || bLog)
            {
                if (l < 4)
                    return true;
                sz = BitConverter.ToInt32(buf, offs);
                l -= 4;
                offs += 4;
                if (l < sz + 4)
                    return true;
                Birthdate = Encoding.UTF8.GetString(buf, offs, sz);
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

            if (eStt < NeeStt.Examing)
                return false;

            if (l < 4)
                return true;
            mAnsSh.questSheetID = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;

            if (eStt < NeeStt.Submitting)
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
            if (l < 4)
                return true;

            int sz = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            //
            if (l < sz)
                return true;
            if (0 < sz)
            {
                ID = Encoding.UTF8.GetString(buf, offs, sz);
                l -= sz;
                offs += sz;
            }

            if (l < 4)
                return true;

            int x;
            if (Enum.IsDefined(typeof(NeeStt), x = BitConverter.ToInt32(buf, offs)))
                eStt = (NeeStt)x;
            l -= 4;
            offs += 4;

            sz = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            //
            if (l < sz + 4)
                return true;
            if (0 < sz)
            {
                Birthdate = Encoding.UTF8.GetString(buf, offs, sz);
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
                Name = Encoding.UTF8.GetString(buf, offs, sz);
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
                Birthplace = Encoding.UTF8.GetString(buf, offs, sz);
                l -= sz;
                offs += sz;
            }
            if (eStt < NeeStt.Finished)
                return false;
            bNRecd = false;
            //
            if (l < sizeof(long))
                return true;
            if (DT.ReadByte(buf, ref offs, out dtTim1))
                return true;
            l -= sizeof(long);

            if (l < 4)
                return true;
            Grade = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;

            if (l < 4)
                return true;
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
            byte[] x = Encoding.UTF8.GetBytes(ID);
            l.Add(BitConverter.GetBytes(x.Length));
            l.Add(x);
            if (0 < tComp.Length)
            {
                x = Encoding.UTF8.GetBytes(tComp);
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
            l.Add(BitConverter.GetBytes(Grade));
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
            if (eStt == NeeStt.Finished)
                return;
            bLog = e.bLog;
            if (eStt < NeeStt.Examing || bLog)
                tComp = e.tComp;
            if (e.eStt < NeeStt.Examing)
                eStt = NeeStt.Examing;
            else
                eStt = e.eStt;
            if (eStt < NeeStt.Examing)
                return;
            mAnsSh = new AnsSheet();
            mAnsSh.questSheetID = e.mAnsSh.questSheetID;

            if (eStt < NeeStt.Submitting)
                return;
            mAnsSh.aAns = e.mAnsSh.aAns;
        }

        public void MergeS(ExamineeA e)
        {
            //suppose e.eStt = NeeStt.Finished
            eStt = e.eStt;
            dtTim1 = e.dtTim1;
            dtTim2 = e.dtTim2;
            Grade = e.Grade;
        }
    }
}
