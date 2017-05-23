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
        public Dictionary<int, AnsSheet> vSheet;
        public AnsPack()
        {
            vSheet = new Dictionary<int, AnsSheet>();
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

        public byte[] ToByte()
        {
            List<byte[]> l = new List<byte[]>();
            l.Add(BitConverter.GetBytes(vSheet.Values.Count));
            foreach (AnsSheet i in vSheet.Values)
                l.Add(i.ToByte());
            int sz = 0;
            foreach (byte[] x in l)
                sz += x.Length;
            if (sz == 0)
                return null;
            byte[] buf = new byte[sz];
            int offs = 0;
            foreach(byte[] x in l)
            {
                Array.Copy(x, 0, buf, offs, x.Length);
                offs += x.Length;
            }
            return buf;
        }

        //only Operation0 uses this.
        public void ExtractKey(List<QuestSheet> l)
        {
            foreach(QuestSheet qs in l)
            {
                AnsSheet i = new AnsSheet();
                i.ExtractKey(qs);
                if(!vSheet.ContainsKey(i.uQSId))
                    vSheet.Add(i.uQSId, i);
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
            if (nSh < 1)
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

        public void ReadByte1(byte[] buf, ref int offs)
        {
            if (buf == null)
                return;
            AnsSheet i = new AnsSheet();
            if(!i.ReadByte(buf, ref offs) &&
                !vSheet.ContainsKey(i.uQSId))
                vSheet.Add(i.uQSId, i);
        }
    }
}
