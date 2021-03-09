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

        public void Clear()
        {
            vSheet.Clear();
        }

        public List<byte[]> GetBytes_S0SendingToS1()
        {
            List<byte[]> l = new List<byte[]>();
            l.Add(BitConverter.GetBytes(vSheet.Values.Count));
            foreach (AnswerSheet i in vSheet.Values)
                l.Add(i.GetBytes_S0SendingToS1());
            return l;
        }

        //only Operation0 uses this.
        public void ExtractKey(List<QuestSheet> l)
        {
            foreach(QuestSheet qs in l)
            {
                AnswerSheet i = new AnswerSheet();
                qs.ExtractKey(i);
                if (!vSheet.ContainsKey(i.QuestSheetID))
                    vSheet.Add(i.QuestSheetID, i);
                else
                    vSheet[i.QuestSheetID] = i;
            }
        }

        public AnswerSheet ExtractKey(QuestSheet qs)
        {
            AnswerSheet i = new AnswerSheet();
            qs.ExtractKey(i);
            if (!vSheet.ContainsKey(i.QuestSheetID))
            {
                vSheet.Add(i.QuestSheetID, i);
                return i;
            }
            return null;
        }

        //only Operation1 uses this.
        public bool ReadBytes_S1ReceivingFromS0(byte[] buf, ref int offs)
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
                if (i.ReadBytes_S1ReceivingFromS0(buf, ref offs) || vSheet.ContainsKey(i.QuestSheetID))
                    return true;
                vSheet.Add(i.QuestSheetID, i);
                --nSh;
            }
            return false;
        }
    }
}
