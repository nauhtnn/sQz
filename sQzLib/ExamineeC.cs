using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sQzLib
{
    public sealed class ExamineeC : ExamineeA
    {
        public ExamineeC() { }
        public override List<byte[]> ToByte()
        {
            List<byte[]> l = new List<byte[]>();
            l.Add(BitConverter.GetBytes(uSlId));
            l.Add(BitConverter.GetBytes(Lv));
            l.Add(BitConverter.GetBytes(uId));
            l.Add(BitConverter.GetBytes(eStt));
            byte[] b;

            if (eStt < eSUBMITTING)//todo optmz
            {
                b = Encoding.UTF8.GetBytes(tBirdate);
                l.Add(BitConverter.GetBytes(b.Length));
                l.Add(b);

                b = Encoding.UTF8.GetBytes(tComp);
                l.Add(BitConverter.GetBytes(b.Length));
            }

            if (eStt < eEXAMING)
                return l;

            l.Add(BitConverter.GetBytes(mAnsSh.uQSId));

            if (eStt < eSUBMITTING)
                return l;

            l.Add(mAnsSh.aAns);

            return l;
        }

        public override bool ReadByte(byte[] buf, ref int offs)
        {
            int l = buf.Length - offs;

            if (l < 4)
                return true;
            eStt = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;

            if (eEXAMING < eStt)
            {
                uGrade = BitConverter.ToUInt16(buf, offs);
                l -= 2;
                offs += 2;
                return false;
            }

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
            tName = Encoding.UTF8.GetString(buf, offs, sz);
            l -= sz;
            offs += sz;

            if (l < 4)
                return true;
            sz = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            if (l < sz)
                return true;
            tBirthplace = Encoding.UTF8.GetString(buf, offs, sz);
            l -= sz;
            offs += sz;

            return false;
        }
    }
}
