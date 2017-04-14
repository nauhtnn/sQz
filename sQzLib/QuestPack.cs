using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sQzLib
{
    public class QuestPack
    {
        public Dictionary<uint, QuestSheet> vSheet;
        public QuestPack()
        {
            vSheet = new Dictionary<uint, QuestSheet>();
        }

        //only Operation0 uses this.
        //optimization: return byte[] instead of List<byte[]>.
        public byte[] ToByte()
        {
            List<byte[]> l = new List<byte[]>();
            l.Add(BitConverter.GetBytes(vSheet.Values.Count));//opt?
            foreach (QuestSheet qs in vSheet.Values)
                foreach (byte[] i in qs.ToByte())
                    l.Add(i);
            //join
            int sz = 0;
            foreach (byte[] i in l)
                sz += i.Length;
            byte[] r = new byte[sz];
            int offs = 0;
            foreach (byte[] i in l)
            {
                Buffer.BlockCopy(i, 0, r, offs, i.Length);
                offs += i.Length;
            }
            return r;
        }

        //only Operation1 uses this.
        public void ReadByte(byte[] buf, ref int offs)
        {
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
                bool err = qs.ReadByte(buf, ref offs);
                if (err)
                    break;
                QuestSheet x;
                if (!vSheet.TryGetValue(qs.mId, out x))
                    vSheet.Add(qs.mId, qs);
                --nSh;
            }
        }
    }
}
