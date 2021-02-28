using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sQzLib
{
    public class ExamRoomS1
    {
        public int uId;
        public SortedList<string, ExamineeS1> Examinees;
        public DateTime t1, t2;

        public ExamRoomS1()
        {
            uId = -1;
            Examinees = new SortedList<string, ExamineeS1>();
        }

        public ExamineeS1 Signin(ExamineeS1 e)
        {
            ExamineeS1 o;
            if (Examinees.TryGetValue(e.ID, out o) && o.Birthdate == e.Birthdate)
            {
                o.MergeWithClient(e);
                return o;
            }
            return null;
        }

        public bool ReadBytes_FromS0(byte[] buf, ref int offs)
        {
            if (buf == null)
                return true;
            if (buf.Length - offs < 0)
                return true;
            int n = BitConverter.ToInt32(buf, offs);
            offs += 4;
            if (n < 0)
                return true;
            while (0 < n)
            {
                --n;
                ExamineeS1 newNee = new ExamineeS1();
                if (newNee.ReadBytes_FromS0(buf, ref offs))
                    return true;
                ExamineeS1 o;
                if (Examinees.TryGetValue(newNee.ID, out o))
                    o.MergeWithS0(newNee);
                else
                    Examinees.Add(newNee.ID, newNee);
            }
            return false;
        }

        public List<byte[]> GetBytes_SendingToS0()
        {
            List<byte[]> l = new List<byte[]>();
            l.Add(BitConverter.GetBytes(uId));
            int n = 0;
            foreach (ExamineeS1 e in Examinees.Values)
                if (e.eStt == NeeStt.Finished && e.NRecd)
                {
                    ++n;
                    l.InsertRange(l.Count, e.ToByte_SendingToS0());
                }
            l.Insert(1, BitConverter.GetBytes(n));
            return l;
        }
    }
}
