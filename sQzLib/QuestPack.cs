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
        public DateTime mDt;
        public ExamLv eLv;
        public Dictionary<int, QuestSheet> vSheet;
        int mNextQSIdx;
        int mMaxQSIdx;
        public QuestPack()
        {
            mDt = DT.INV_;
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

        public List<int> DBSelectId(DateTime dt)
        {
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return null;
            string qry = DBConnect.mkQrySelect("qs", "lv,id", "dt=" + dt.ToString(DT._));
            string eMsg;
            MySqlDataReader reader = DBConnect.exeQrySelect(conn, qry, out eMsg);
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
            string emsg;
            List<QuestSheet> l = new List<QuestSheet>();
            bool cont;
            while (0 < n)
            {
                --n;
                cont = false;
                QuestSheet qs = new QuestSheet();
                int j = -1;
                foreach (IUx i in QuestSheet.GetIUs(eLv))
                    if (qs.DBSelect(i, vn[++j], out emsg))
                    {
                        WPopup.s.ShowDialog(emsg);
                        cont = true;
                        break;
                    }
                if (cont)
                    continue;
                qs.eLv = eLv;
                if(!qs.UpdateCurQSId())//todo: better error handle
                {
                    vSheet.Add(qs.uId, qs);
                    l.Add(qs);
                }
            }
            if(DBIns(mDt, l) == null)
                return l;
            return new List<QuestSheet>();
        }

        public List<QuestSheet> GenQPack2(int n, int[] vn)
        {
            List<QuestSheet> l = new List<QuestSheet>();
            int i;
            for (i = 0; i < n; ++i)
                l.Add(new QuestSheet());
            i = 0;
            Random rand = new Random();
            foreach (IUx iu in QuestSheet.GetIUs(eLv))
            {
                //
                QuestSheet qs0 = new QuestSheet();
                qs0.DBSelect(iu);
                //
                foreach (QuestSheet qs in l)
                {
                    List<Question> vq = qs0.ShallowCopy();
                    int ni = vn[i];
                    while (0 < ni)
                    {
                        int idx = rand.Next() % ni;
                        qs.vQuest.Add(vq.ElementAt(idx).DeepCopy());
                        vq.RemoveAt(idx);
                        --ni;
                    }
                }
                ++i;
            }
            List<int> eidx = new List<int>();
            i = -1;
            foreach (QuestSheet qs in l)
            {
                qs.eLv = eLv;
                qs.Randomize(rand);
                if (!qs.UpdateCurQSId())//todo: better error handle
                    vSheet.Add(qs.uId, qs);
                else
                    eidx.Add(++i);
            }
            foreach (int idx in eidx)
                l.RemoveAt(idx);

            if (DBIns(mDt, l) == null)
                return l;
            return new List<QuestSheet>();
        }

        public List<QuestSheet> GenQPack3(int n, int[] vn)
        {
            string emsg;
            List<QuestSheet> l = new List<QuestSheet>();
            QuestSheet qs0 = new QuestSheet();
            int j = -1;
            foreach (IUx i in QuestSheet.GetIUs(eLv))
                if (qs0.DBSelect(i, vn[++j], out emsg))
                {
                    WPopup.s.ShowDialog(emsg);
                    return new List<QuestSheet>();
                }
            Random rand = new Random();
            while (0 < n)
            {
                --n;
                QuestSheet qs = qs0.RandomizeDeepCopy(rand);
                qs.eLv = eLv;
                if (!qs.UpdateCurQSId())//todo: better error handle
                {
                    vSheet.Add(qs.uId, qs);
                    l.Add(qs);
                }
            }
            if (DBIns(mDt, l) == null)
                return l;
            return new List<QuestSheet>();
        }

        public static string DBIns(DateTime dt, List<QuestSheet> l)
        {
            if (l.Count == 0)
                return Txt.s._[(int)TxI.DB_DAT_NOK];
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return Txt.s._[(int)TxI.DB_NOK];
            StringBuilder vals = new StringBuilder();
            string prefx = "('" + dt.ToString(DT._) + "',";
            foreach (QuestSheet qs in l)
                vals.Append(prefx + "'" + qs.eLv.ToString() + "'," + qs.uId +
                    ",'" + dt.ToString(DT.hh) + "'),");
            vals.Remove(vals.Length - 1, 1);//remove the last comma
            string eMsg;
            if(DBConnect.Ins(conn, "sqz_qsheet", "dt,lv,id,t", vals.ToString(), out eMsg) < 0)
            {
                DBConnect.Close(ref conn);
                if (eMsg == null)
                    eMsg = Txt.s._[(int)TxI.DB_EXCPT] + Txt.s._[(int)TxI.QS_ID_EXISTS];
                return eMsg;
            }
            vals.Clear();
            prefx = "('" + dt.ToString(DT._) + "',";
            foreach(QuestSheet qs in l)
                foreach (Question q in qs.vQuest)
                {
                    vals.Append(prefx + "'" + qs.eLv.ToString() + "'," +
                        qs.uId + "," + q.uId + ",");
                    foreach (int i in q.vAnsSort)
                        vals.Append(i.ToString());
                    vals.Append("),");
                }
            vals.Remove(vals.Length - 1, 1);//remove the last comma
            if (DBConnect.Ins(conn, "sqz_qsheet_quest", "dt,lv,qsid,qid,asort", vals.ToString(), out eMsg) < 0)
            {
                DBConnect.Close(ref conn);
                return eMsg;
            }
            DBConnect.Close(ref conn);
            return null;
        }

        public static string DBIns0(DateTime dt, List<QuestSheet> l)
        {
            if (l.Count == 0)
                return Txt.s._[(int)TxI.DB_DAT_NOK];
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return Txt.s._[(int)TxI.DB_NOK];
            StringBuilder vals = new StringBuilder();
            string prefx = "('" + dt.ToString(DT._) + "',";
            foreach (QuestSheet qs in l)
                vals.Append(prefx + "'" + qs.eLv.ToString() + "'," + qs.uId +
                    ",'" + dt.ToString(DT.hh) + "'),");
            vals.Remove(vals.Length - 1, 1);//remove the last comma
            string eMsg;
            if (DBConnect.Ins(conn, "sqz_qsheet", "dt,lv,id,t", vals.ToString(), out eMsg) < 0)
            {
                DBConnect.Close(ref conn);
                if (eMsg == null)
                    eMsg = Txt.s._[(int)TxI.DB_EXCPT] + Txt.s._[(int)TxI.QS_ID_EXISTS];
                return eMsg;
            }
            vals.Clear();
            prefx = "('" + dt.ToString(DT._) + "',";
            foreach (QuestSheet qs in l)
                foreach (Question q in qs.vQuest)
                    vals.Append(prefx + "'" + qs.eLv.ToString() + "'," +
                        qs.uId + "," + q.uId + "),");
            vals.Remove(vals.Length - 1, 1);//remove the last comma
            if (DBConnect.Ins(conn, "sqz_qsheet_quest", "dt,lv,qsid,qid", vals.ToString(), out eMsg) < 0)
            {
                DBConnect.Close(ref conn);
                return eMsg;
            }
            DBConnect.Close(ref conn);
            return null;
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
