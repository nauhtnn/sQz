using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sQzLib
{
    public sealed class ExamineeS1: ExamineeA
    {
        public bool bLog;

        bool bNRecd;
        public bool NRecd { get { return bNRecd; } }
        public ExamineeS1() {
            Reset();
        }

        public override void Reset()
        {
            _Reset();
            bNRecd = true;
            bLog = false;
        }

        public byte[] GetBytes_SendingToClient()
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

            return Utils.ListOfBytes_ToArray(l);
        }

        public bool ReadBytes_FromClient(byte[] buf, ref int offs)
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
            if (Enum.IsDefined(typeof(NeeStt), sz = BitConverter.ToInt32(buf, offs)))
                eStt = (NeeStt)sz;
            l -= 4;
            offs += 4;

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
                ComputerName = Encoding.UTF8.GetString(buf, offs, sz);
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

        public bool ReadBytes_FromS0(byte[] buf, ref int offs)
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
            if ((dtTim1 = DT.ReadByte(buf, ref offs)) == DT.INVALID)
                return true;
            l -= sizeof(long);

            if (l < sizeof(long))
                return true;
            if ((dtTim2 = DT.ReadByte(buf, ref offs)) == DT.INVALID)
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
                ComputerName = Encoding.UTF8.GetString(buf, offs, sz);
                l -= sz;
                offs += sz;
            }
            //
            return false;
        }

        public List<byte[]> ToByte_SendingToS0()
        {
            //suppose eStt == NeeStt.Finished
            List<byte[]> l = new List<byte[]>();
            byte[] x = Encoding.UTF8.GetBytes(ID);
            l.Add(BitConverter.GetBytes(x.Length));
            l.Add(x);
            if (0 < ComputerName.Length)
            {
                x = Encoding.UTF8.GetBytes(ComputerName);
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

        //public override void Merge(ExamineeA e)
        //{
        //    if (bFromC)
        //        MergeWithClient(e as ExamineeS1);
        //    else
        //        MergeWithS0(e as ExamineeS0);
        //}

        public void MergeWithClient(ExamineeS1 e)
        {
            if (eStt == NeeStt.Finished)
                return;
            bLog = e.bLog;
            if (eStt < NeeStt.Examing || bLog)
                ComputerName = e.ComputerName;
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

        public void MergeWithS0(ExamineeA e)
        {
            //suppose e.eStt = NeeStt.Finished
            eStt = e.eStt;
            dtTim1 = e.dtTim1;
            dtTim2 = e.dtTim2;
            Grade = e.Grade;
        }
    }
}
