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
        public Dictionary<int, QuestSheet> vSheet;
        public QuestPack()
        {
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
                if(qs.ReadByte(buf, ref offs))
                    break;
                if (!vSheet.ContainsKey(qs.uId))
                    vSheet.Add(qs.uId, qs);
                --nSh;
            }
        }

        public bool ReadByte1(byte[] buf, ref int offs)
        {
            if (buf == null)
                return false;
            int offs0 = offs;
            QuestSheet qs = new QuestSheet();
            if(qs.ReadByte(buf, ref offs))
                return false;
            if (!vSheet.ContainsKey(qs.uId))
            {
                vSheet.Add(qs.uId, qs);
                return true;
            }
            return false;
        }

        public List<int> DBSelectId(uint slId)
        {
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return null;
            string qry = DBConnect.mkQrySelect("questsh", "lv,id", "slId=" + slId, null);
            MySqlDataReader reader = DBConnect.exeQrySelect(conn, qry);
            List<int> r = new List<int>();
            if (reader != null)
            {
                while (reader.Read())
                    r.Add(reader.GetInt16(0) * reader.GetUInt16(1));
                reader.Close();
            }
            DBConnect.Close(ref conn);
            return r;
        }

        public void DBIns(uint slId, List<QuestSheet> l)
        {
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return;
            StringBuilder vals = new StringBuilder();
            foreach (QuestSheet qs in l)
                qs.DBAppendInsQry(slId, ref vals);
            vals.Remove(vals.Length - 1, 1);//remove the last comma
            DBConnect.Ins(conn, "questsh", "slId,lv,id,vQuest", vals.ToString());//todo: catch exception
            DBConnect.Close(ref conn);
        }

        public ushort DBCurQSId(uint slId, ExamLvl lv)
        {
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return 0;
            ushort id = (ushort)DBConnect.Max(conn, "questsh", "id",
                    "slId=" + slId + " AND lv=" + (short)lv);
            DBConnect.Close(ref conn);
            return id;
        }
    }
}
