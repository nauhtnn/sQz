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
        public SortedList<int, QuestSheet> vSheet;
        public int TestType;
        int mNextQSIdx;
        int mMaxQSIdx;
        public QuestPack()
        {
            mDt = DT.INVALID;
            mNextQSIdx = 0;
            mMaxQSIdx = -1;
            vSheet = new SortedList<int, QuestSheet>();
        }

        //only Operation0 uses this.
        //optimization: return byte[] instead of List<byte[]>.
        public List<byte[]> ToByte()
        {
            List<byte[]> l = new List<byte[]>();
            l.Add(BitConverter.GetBytes(TestType));
            l.Add(BitConverter.GetBytes(vSheet.Values.Count));
            foreach (QuestSheet qs in vSheet.Values)
                foreach (byte[] i in qs.ToByte())
                    l.Add(i);
            return l;
        }

        //only Operation1 uses this.
        public bool ReadByte(byte[] buf, ref int offs)
        {
            vSheet.Clear();
            if (buf == null)
                return false;
            int offs0 = offs;
            int l = buf.Length - offs;


            if (l < 4)
                return false;
            TestType = BitConverter.ToInt32(buf, offs);
            offs += 4;
            l -= 4;

            if (l < 4)
                return false;
            int nSh = BitConverter.ToInt32(buf, offs);
            offs += 4;
            l -= 4;
            if (nSh < 0)
                return false;
            while(0 < nSh)
            {
                QuestSheet qs = new QuestSheet();
                if(qs.ReadByte(buf, ref offs))
                    return false;
                if (!vSheet.ContainsKey(qs.ID))
                {
                    vSheet.Add(qs.ID, qs);
                }
                --nSh;
            }
            mNextQSIdx = 0;
            mMaxQSIdx = vSheet.Keys.Count - 1;
            return true;
        }

        //public static List<string> DBSelectQStId(DateTime dt)
        //{
        //    MySqlConnection conn = DBConnect.Init();
        //    if (conn == null)
        //        return null;
        //    string qry = DBConnect.mkQrySelect("sqz_qsheet", "lv,id",
        //        "dt='" + dt.ToString(DT._) + "' AND t='" + dt.ToString(DT.hh) + "'");
        //    string eMsg;
        //    MySqlDataReader reader = DBConnect.exeQrySelect(conn, qry, out eMsg);
        //    List<string> l = new List<string>();
        //    if (reader != null)
        //    {
        //        while (reader.Read())
        //        {
        //            ExamLv lv;
        //            if (Enum.TryParse(reader.GetString(0), out lv))
        //                break;
        //            l.Add(lv.ToString() + reader.GetUInt16(1).ToString("d3"));
        //        }
        //        reader.Close();
        //    }
        //    DBConnect.Close(ref conn);
        //    return l;
        //}

        public List<string> SelectQStId()
        {
            List<string> l = new List<string>();
            foreach (QuestSheet qs in vSheet.Values)
                l.Add(qs.ID.ToString("d3"));
            return l;
        }

        public bool DBSelectQS(out string eMsg)
        {
            MySqlConnection conn = DBConnect.OpenNewConnection();
            if (conn == null)
            {
                eMsg = Txt.s._((int)TxI.DB_NOK);
                return true;
            }
            string qry = DBConnect.mkQrySelect("sqz_qsheet",
                "id", "dt='" + mDt.ToString(DT._) + "' AND t_type=" + TestType);
            MySqlDataReader reader = DBConnect.exeQrySelect(conn, qry, out eMsg);
            List<int> qsids = new List<int>();
            if (reader != null)
            {
                while (reader.Read())
                    qsids.Add(reader.GetUInt16(0));
                reader.Close();
                foreach(int qsid in qsids)
                {
                    QuestSheet qs = new QuestSheet();
                    if (qs.DBSelect(conn, mDt, qsid, out eMsg))
                    {
                        DBConnect.Close(ref conn);
                        return true;
                    }
                    vSheet.Add(qs.ID, qs);
                }
            }
            DBConnect.Close(ref conn);
            return false;
        }

        public bool DBDelete(out string eMsg)
        {
            if (vSheet.Count == 0)
            {
                eMsg = string.Empty;
                return false;
            }
            MySqlConnection conn = DBConnect.OpenNewConnection();
            if (conn == null)
            {
                eMsg = Txt.s._((int)TxI.DB_NOK);
                return true;
            }
            eMsg = null;
            StringBuilder cond1 = new StringBuilder();
            cond1.Append("dt='" + mDt.ToString(DT._) +
                    "' AND qsid IN (");
            StringBuilder cond2 = new StringBuilder();
            cond2.Append("dt='" + mDt.ToString(DT._) +
                    "' AND id IN (");
            foreach (QuestSheet qs in vSheet.Values)
            {
                //int n = DBConnect.Count(conn, "sqz_nee_qsheet", "qsid", cond1 + qs.uId, out eMsg);
                //if (0 < n)
                //    continue;
                cond1.Append(qs.ID + ",");
                cond2.Append(qs.ID + ",");
            }
            cond1.Remove(cond1.Length - 1, 1);//remove last comma
            cond1.Append(")");
            cond2.Remove(cond2.Length - 1, 1);//remove last comma
            cond2.Append(")");
            if (DBConnect.Delete(conn, "sqz_qsheet_quest", cond1.ToString(), out eMsg) < 0)
            {
                DBConnect.Close(ref conn);
                return true;
            }
            if (DBConnect.Delete(conn, "sqz_qsheet", cond2.ToString(), out eMsg) < 0)
            {
                DBConnect.Close(ref conn);
                return true;
            }
            DBConnect.Close(ref conn);
            return false;
        }

        public List<QuestSheet> GenQPack3(int testType, int numberOfSheet)
        {
            List<QuestSheet> sheets = new List<QuestSheet>();
            QuestSheet originSheet = new QuestSheet();
            originSheet.DBSelectNondeletedQuestions(testType);
            Random rand = new Random();
            while (0 < numberOfSheet)
            {
                --numberOfSheet;
                QuestSheet qs = originSheet.RandomizeDeepCopy_SectionsOnly(rand);
                qs.AccquireGlobalMaxID();
                qs.TestType = testType;
                vSheet.Add(qs.ID, qs);
                sheets.Add(qs);
            }
            if (DBInsertQSheets(mDt, sheets) == null)
                return sheets;
            return new List<QuestSheet>();
        }

        public static string DBInsertQSheets(DateTime dt, List<QuestSheet> l)
        {
            if (l.Count == 0)
                return Txt.s._((int)TxI.DB_DAT_NOK);
            MySqlConnection conn = DBConnect.OpenNewConnection();
            if (conn == null)
                return Txt.s._((int)TxI.DB_NOK);
            StringBuilder vals = new StringBuilder();
            string prefx = "('" + dt.ToString(DT._) + "',";
            foreach (QuestSheet qs in l)
                vals.Append(prefx + qs.ID + ","+ qs.TestType + "),");
            vals.Remove(vals.Length - 1, 1);//remove the last comma
            string eMsg;
            if(DBConnect.Ins(conn, "sqz_qsheet", "dt,id,t_type", vals.ToString(), out eMsg) < 0)
            {
                DBConnect.Close(ref conn);
                if (eMsg == null)
                    eMsg = Txt.s._((int)TxI.DB_EXCPT) + Txt.s._((int)TxI.QS_ID_EXISTS);
                return eMsg;
            }
            vals.Clear();
            prefx = "('" + dt.ToString(DT._) + "',";
            foreach (QuestSheet qs in l)
                qs.DBAppendQryIns(prefx, vals);
            vals.Remove(vals.Length - 1, 1);//remove the last comma
            if (DBConnect.Ins(conn, "sqz_qsheet_quest", "dt,qsid,qid,asort,idx", vals.ToString(), out eMsg) < 0)
            {
                DBConnect.Close(ref conn);
                return eMsg;
            }
            DBConnect.Close(ref conn);
            return null;
        }

        public byte[] GetBytes_NextQSheet()
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
