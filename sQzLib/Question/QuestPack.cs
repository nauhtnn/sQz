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
        int mNextQSIdx;
        int mMaxQSIdx;
        public QuestPack()
        {
            mDt = DT.INV_;
            mNextQSIdx = 0;
            mMaxQSIdx = -1;
            vSheet = new SortedList<int, QuestSheet>();
        }

        //only Operation0 uses this.
        //optimization: return byte[] instead of List<byte[]>.
        public List<byte[]> ToByte()
        {
            List<byte[]> l = new List<byte[]>();
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
                if (!vSheet.ContainsKey(qs.ID))
                {
                    vSheet.Add(qs.ID, qs);
                }
                --nSh;
            }
            mNextQSIdx = 0;
            mMaxQSIdx = vSheet.Keys.Count - 1;
            return false;
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

        public bool DBSelectQS(DateTime dt, out string eMsg)
        {
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
            {
                eMsg = Txt.s._((int)TxI.DB_NOK);
                return true;
            }
            string qry = DBConnect.mkQrySelect("sqz_qsheet",
                "id", "dt='" + dt.ToString(DT._) + "'");
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
                    if (qs.DBSelect(conn, dt, qsid, out eMsg))
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
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
            {
                eMsg = Txt.s._((int)TxI.DB_NOK);
                return true;
            }
            eMsg = null;
            string cond1 = "dt='" + mDt.ToString(DT._) +
                    "' AND qsid=";
            string cond2 = "dt='" + mDt.ToString(DT._) +
                    "' AND id=";
            foreach (QuestSheet qs in vSheet.Values)
            {
                //int n = DBConnect.Count(conn, "sqz_nee_qsheet", "qsid", cond1 + qs.uId, out eMsg);
                //if (0 < n)
                //    continue;
                if (DBConnect.Delete(conn, "sqz_qsheet_quest", cond1 + qs.ID, out eMsg) < 0)
                    return true;
                if (DBConnect.Delete(conn, "sqz_qsheet", cond2 + qs.ID, out eMsg) < 0)
                    return true;
            }
            return false;
        }

        public List<QuestSheet> GenQPack3(int numberOfSheet)
        {
            string emsg;
            List<QuestSheet> sheets = new List<QuestSheet>();
            QuestSheet independentQuestions = new QuestSheet();
            independentQuestions.DBSelect(-1, out emsg);
            Random rand = new Random();
            while (0 < numberOfSheet)
            {
                --numberOfSheet;
                QuestSheet qs = independentQuestions.RandomizeDeepCopy(rand);
                if (!qs.AccquireGlobalMaxID())//todo: better error handle
                {
                    vSheet.Add(qs.ID, qs);
                    sheets.Add(qs);
                }
            }
            if (DBIns(mDt, sheets) == null)
                return sheets;
            return new List<QuestSheet>();
        }

        public static string DBIns(DateTime dt, List<QuestSheet> l)
        {
            if (l.Count == 0)
                return Txt.s._((int)TxI.DB_DAT_NOK);
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return Txt.s._((int)TxI.DB_NOK);
            StringBuilder vals = new StringBuilder();
            string prefx = "('" + dt.ToString(DT._) + "',";
            foreach (QuestSheet qs in l)
                vals.Append(prefx + "," + qs.ID + "),");
            vals.Remove(vals.Length - 1, 1);//remove the last comma
            string eMsg;
            if(DBConnect.Ins(conn, "sqz_qsheet", "dt,id", vals.ToString(), out eMsg) < 0)
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

        //public List<int[]> GetNMod()
        //{
        //    if (vSheet.Values.Count == 0)
        //        return null;
        //    return vSheet.First().Value.GetNMod();
        //}

        public void WriteTxt()
        {
            //string extension = ".txt";
            //foreach (QuestSheet qs in vSheet.Values)
            //    qs.WriteTxt(qs.eLv.ToString() + qs.uId + extension);
        }

        public void WriteDocx()
        {
            //string extension = ".docx";
            //foreach (QuestSheet qs in vSheet.Values)
            //    QuestSheetDocxPrinter.GetInstance().Print(qs.eLv.ToString() + qs.uId + extension,
            //        qs.ToListOfStrings(), mDt.ToString(DT.RR), qs.tId);
        }
    }
}
