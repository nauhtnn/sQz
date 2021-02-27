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

        public bool ReadBytes(byte[] buf, ref int offs)
        {
            return ReadBytes(buf, ref offs, new ExamineeS1(), true);
        }

        public List<byte[]> GetBytes()
        {
            List<byte[]> l = new List<byte[]>();
            l.Add(BitConverter.GetBytes(uId));
            int n = 0;
            foreach (ExamineeS1 e in Examinees.Values)
                if (e.eStt == NeeStt.Finished && e.NRecd)
                {
                    ++n;
                    l.InsertRange(l.Count, e.ToByte_S1SendingToS0());
                }
            l.Insert(1, BitConverter.GetBytes(n));
            return l;
        }
    }
}
