using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace sQzLib
{
    public class AnswerPack
    {
        public SortedList<int, AnswerSheet> vSheet;
        public AnswerPack()
        {
            vSheet = new SortedList<int, AnswerSheet>();
        }

        public int GetByteCount()
        {
            int sz = 4;
            foreach (AnswerSheet s in vSheet.Values)
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
            foreach (AnswerSheet i in vSheet.Values)
                i.ToByte(ref buf, ref offs);
            l = buf.Length - offs;
            return false;
        }

        public List<byte[]> ToByte()
        {
            List<byte[]> l = new List<byte[]>();
            l.Add(BitConverter.GetBytes(vSheet.Values.Count));
            foreach (AnswerSheet i in vSheet.Values)
                l.Add(i.ToByte());
            return l;
        }

        //only Operation0 uses this.
        public void ExtractKey(List<QuestSheet> l)
        {
            foreach(QuestSheet qs in l)
            {
                AnswerSheet i = new AnswerSheet();
                qs.ExtractKey(i);
                if (!vSheet.ContainsKey(i.questSheetID))
                    vSheet.Add(i.questSheetID, i);
                else
                    vSheet[i.questSheetID] = i;
            }
        }

        public AnswerSheet ExtractKey(QuestSheet qs)
        {
            AnswerSheet i = new AnswerSheet();
            qs.ExtractKey(i);
            if (!vSheet.ContainsKey(i.questSheetID))
            {
                vSheet.Add(i.questSheetID, i);
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
                AnswerSheet i = new AnswerSheet();
                if (i.ReadByte(buf, ref offs) || vSheet.ContainsKey(i.questSheetID))
                    return true;
                vSheet.Add(i.questSheetID, i);
                --nSh;
            }
            return false;
        }
    }
}
