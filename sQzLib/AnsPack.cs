using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace sQzLib
{
    public class AnsPack
    {
        public SortedList<int, AnsSheet> vSheet;
        public AnsPack()
        {
            vSheet = new SortedList<int, AnsSheet>();
        }

        public int GetByteCount()
        {
            int sz = 4;
            foreach (AnsSheet s in vSheet.Values)
                sz += s.GetByteCount();
            return sz;
        }

        //only Operation0 uses this.
        public bool ToByte(ref byte[] buf, ref int offs)//todo: opt-out?
        {
            int l = buf.Length - offs;
            if (l < 4)
                return true;
            Buffer.BlockCopy(BitConverter.GetBytes(vSheet.Values.Count), 0, buf, offs, 4);
            offs += 4;
            //l -= 4;
            foreach (AnsSheet i in vSheet.Values)
                i.ToByte(ref buf, ref offs);
            l = buf.Length - offs;
            return false;
        }

        public List<byte[]> ToByte()
        {
            List<byte[]> l = new List<byte[]>();
            l.Add(BitConverter.GetBytes(vSheet.Values.Count));
            foreach (AnsSheet i in vSheet.Values)
                l.Add(i.ToByte());
            return l;
        }

        //only Operation0 uses this.
        public void ExtractKey(List<QuestSheet> l)
        {
            foreach(QuestSheet qs in l)
            {
                AnsSheet i = new AnsSheet();
                i.ExtractKey(qs);
                if (!vSheet.ContainsKey(i.uQSLvId))
                    vSheet.Add(i.uQSLvId, i);
                else
                    vSheet[i.uQSLvId] = i;
            }
        }

        public AnsSheet ExtractKey(QuestSheet qs)
        {
            AnsSheet i = new AnsSheet();
            i.ExtractKey(qs);
            if (!vSheet.ContainsKey(i.uQSId))
            {
                vSheet.Add(i.uQSId, i);
                return i;
            }
            return null;
        }

        //only Operation1 uses this.
        public bool ReadByte(byte[] buf, ref int offs)
        {
            vSheet.Clear();
            if (buf == null)
                return true;
            int l = buf.Length - offs;
            if (l < 4)
                return true;
            int nSh = BitConverter.ToInt32(buf, offs);
            offs += 4;
            l -= 4;
            if (nSh < 0)
                return true;
            while (0 < nSh)
            {
                AnsSheet i = new AnsSheet();
                if (i.ReadByte(buf, ref offs) || vSheet.ContainsKey(i.uQSId))
                    return true;
                vSheet.Add(i.uQSId, i);
                --nSh;
            }
            return false;
        }

        public bool ReadByte1(byte[] buf, ref int offs)
        {
            if (buf == null)
                return true;
            AnsSheet i = new AnsSheet();
            if (i.ReadByte(buf, ref offs) || vSheet.ContainsKey(i.uQSId))
                return true;
            vSheet.Add(i.uQSId, i);
            return false;
        }
    }
}
