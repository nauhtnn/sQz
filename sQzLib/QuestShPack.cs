using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sQzLib
{
    public class QuestShPack
    {
        public Dictionary<uint, QuestSheet> vSheet;
        public QuestShPack()
        {
            vSheet = new Dictionary<uint, QuestSheet>();
        }

        //only Operation0 uses this.
        //optimization: return byte[] instead of List<byte[]>.
        public byte[] ToByte(bool woKey)
        {
            woKey = false;
            List<byte[]> l = new List<byte[]>();
            //List<bool> lk = new List<bool>();
            //if(woKey)
            //    lk.Add(false);
            l.Add(BitConverter.GetBytes(vSheet.Values.Count));//opt?
            foreach (QuestSheet qs in vSheet.Values)
            {
                foreach (byte[] i in qs.ToByte(woKey))
                    l.Add(i);
            }
            //join
            int sz = 0;
            //int szk = 0;
            foreach (byte[] i in l)
            {
                sz += i.Length;
                //if (woKey && lk[j])
                //    szk += l[j].Length;
            }
            //if (woKey)
            //    sbArr = new byte[sz - szk];
            byte[] r = new byte[sz];
            int offs = 0;
            foreach (byte[] i in l)
            {
                Buffer.BlockCopy(i, 0, r, offs, i.Length);
                //if (woKey && !lk[j])
                //    Buffer.BlockCopy(l[j], 0, sbArr, offs, l[j].Length);
                offs += i.Length;
            }
            return r;
            //sRdywKey = true;
            //if (woKey)
            //    sRdy = true;
        }

        //only Operation1 uses this.
        public void ReadByte(byte[] buf, ref int offs, bool wKey)
        {
            wKey = true;
            vSheet.Clear();
            if (buf == null)
                return;
            int offs0 = offs;
            int l = buf.Length - offs;
            if (l < 4)
                return;
            int nSh = BitConverter.ToInt32(buf, offs);
            offs += 4;
            l -= 4;
            if (nSh < 1)
                return;
            while(0 < nSh)
            {
                QuestSheet qs = new QuestSheet();
                bool err = qs.ReadByte(buf, ref offs, wKey);
                if (err)
                    break;
                //if (!vSheet.TryGetValue(qs.mId, out qs))//todo safer
                    vSheet.Add(qs.mId, qs);
                --nSh;
            }
        }
    }
}
