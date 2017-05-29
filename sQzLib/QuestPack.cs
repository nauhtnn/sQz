using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace sQzLib
{
    public class QuestPack
    {
        public uint uSlId;
        public ExamLv eLv;
        public Dictionary<int, QuestSheet> vSheet;
        int mNextQSIdx;
        int mMaxQSIdx;
        public QuestPack()
        {
            mNextQSIdx = 0;
            mMaxQSIdx = -1;
            vSheet = new Dictionary<int, QuestSheet>();
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
        public bool ReadByte(byte[] buf, ref int offs)
        {
            vSheet.Clear();
            if (buf == null)
                return true;
            int offs0 = offs;
            int l = buf.Length - offs;
            if (l < 4)
                return true;
            int nSh = BitConverter.ToInt32(buf, offs);
            offs += 4;
            l -= 4;
            if (nSh < 0)
                return true;
            while(0 < nSh)
            {
                QuestSheet qs = new QuestSheet();
                if(qs.ReadByte(buf, ref offs))
                    return true;
                if (!vSheet.ContainsKey(qs.uId))
                {
                    qs.eLv = eLv;//optmz
                    vSheet.Add(qs.uId, qs);
                }
                --nSh;
            }
            mNextQSIdx = 0;
            mMaxQSIdx = vSheet.Keys.Count - 1;
            return false;
        }

        public bool ReadByte1(byte[] buf, ref int offs)
        {
            if (buf == null)
                return true;
            int offs0 = offs;
            QuestSheet qs = new QuestSheet();
            if(qs.ReadByte(buf, ref offs))
                return true;
            if (vSheet.ContainsKey(qs.uId))
                return true;
            vSheet.Add(qs.uId, qs);
            ++mMaxQSIdx;
            return false;
        }

        public List<int> DBSelectId(uint slId)
        {
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return null;
            string qry = DBConnect.mkQrySelect("questsh", "lv,id", "slId=" + slId, null);
            MySqlDataReader reader = null;//todo DBConnect.exeQrySelect(conn, qry);
            List<int> r = new List<int>();
            if (reader != null)
            {
                while (reader.Read())
                    r.Add(reader.GetInt32(0) * reader.GetUInt16(1));
                reader.Close();
            }
            DBConnect.Close(ref conn);
            return r;
        }

        public List<QuestSheet> GenQPack(int n, int[] vn)
        {
            List<QuestSheet> l = new List<QuestSheet>();
            while (0 < n)
            {
                QuestSheet qs = new QuestSheet();
                int j = -1;
                foreach (IUx i in QuestSheet.GetIUs(eLv))
                    qs.DBSelect(i, vn[++j]);
                if (0 < qs.vQuest.Count)
                {
                    qs.eLv = eLv;
                    if(!qs.UpdateCurQSId())//todo: better error handle
                    {
                        vSheet.Add(qs.uId, qs);
                        l.Add(qs);
                    }
                }
                --n;
            }
            DBIns(uSlId, l);
            return l;
        }

        public static void DBIns(uint slId, List<QuestSheet> l)
        {
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return;
            StringBuilder vals = new StringBuilder();
            foreach (QuestSheet qs in l)
                qs.DBAppendInsQry(slId, ref vals);
            vals.Remove(vals.Length - 1, 1);//remove the last comma
            //DBConnect.Ins(conn, "questsh", "slId,lv,id,vQuest", vals.ToString());//todo: catch exception
            DBConnect.Close(ref conn);
        }

        public byte[] ToByteNextQS()
        {
            if (mMaxQSIdx < 0)
                return null;
            if (mMaxQSIdx < mNextQSIdx)
                mNextQSIdx = 0;
            if (mNextQSIdx < vSheet.Count)
                return vSheet.ElementAt(mNextQSIdx++).Value.aQuest;
            else
                return null;
        }
    }
}
