using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sQzLib
{
    public abstract class ExamRoomA
    {
        public int uId;
        public SortedList<string, ExamineeA> Examinees;
        public DateTime t1, t2;
        public ExamRoomA()
        {
            uId = -1;
            Examinees = new SortedList<string, ExamineeA>();
        }

        public abstract bool ReadBytes((byte[] buf, ref int offs);

        protected bool _ReadBytes(byte[] buf, ref int offs, ExamineeA newNee, bool addIfNExist)
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
                //newNee.bFromC = false;
                if (newNee.ReadByte(buf, ref offs))
                    return true;
                var o = newNee;
                if (Examinees.TryGetValue(newNee.ID, out o))
                {
                    o.bFromC = false;
                    o.Merge(newNee);
                }
                else if(addIfNExist)
                    Examinees.Add(newNee.ID, newNee);
            }
            return false;
        }
    }
}
