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
        public bool bAlt;
        public SortedList<int, QuestSheet> vSheet;
        int mNextQSIdx;
        int mMaxQSIdx;
        protected QuestPack()
        {
            mDt = DT.INV_;
            mNextQSIdx = 0;
            mMaxQSIdx = -1;
            bAlt = false;
            vSheet = new SortedList<int, QuestSheet>();
        }

        public QuestPack(bool alt)
        {
            mDt = DT.INV_;
            mNextQSIdx = 0;
            mMaxQSIdx = -1;
            bAlt = alt;
            vSheet = new SortedList<int, QuestSheet>();
        }

        //only Operation0 uses this.
        //optimization: return byte[] instead of List<byte[]>.
        public List<byte[]> ToByte()
        {
            List<byte[]> l = new List<byte[]>();
            l.Add(BitConverter.GetBytes((int)eLv));
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
            string tlv = eLv.ToString();
            foreach (QuestSheet qs in vSheet.Values)
                l.Add(tlv + qs.uId.ToString("d3"));
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
                "id", "dt='" + dt.ToString(DT._) + "' AND t='" + dt.ToString(DT.hh) +
                "' AND lv='" + eLv.ToString() + "' AND alt=" +
                (bAlt ? '1' : '0'));
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
                    if (qs.DBSelect(conn, dt, eLv, qsid, out eMsg))
                    {
                        DBConnect.Close(ref conn);
                        return true;
                    }
                    vSheet.Add(qs.uId, qs);
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
                    "' AND lv='" + eLv.ToString() + "' AND qsid=";
            string cond2 = "dt='" + mDt.ToString(DT._) +
                    "' AND lv='" + eLv.ToString() + "' AND id=";
            foreach (QuestSheet qs in vSheet.Values)
            {
                //int n = DBConnect.Count(conn, "sqz_nee_qsheet", "qsid", cond1 + qs.uId, out eMsg);
                //if (0 < n)
                //    continue;
                if (DBConnect.Delete(conn, "sqz_qsheet_quest", cond1 + qs.uId, out eMsg) < 0)
                    return true;
                if (DBConnect.Delete(conn, "sqz_qsheet", cond2 + qs.uId, out eMsg) < 0)
                    return true;
            }
            return false;
        }

        public List<QuestSheet> GenQPack2(int n, int[] vn, int[] vndiff)
        {
            List<QuestSheet> l = new List<QuestSheet>();
            int i;
            for (i = 0; i < n; ++i)
                l.Add(new QuestSheet());
            foreach (QuestSheet qs in l)
                qs.bAlt = bAlt;
            i = 0;
            Random rand = new Random();
            foreach (IUx iu in QuestSheet.GetIUs(eLv))
            {
                //
                QuestSheet qs0 = new QuestSheet();
                //qs0.bAlt = bAlt;
                qs0.DBSelect(iu, QuestDiff.Easy);
                //
                foreach (QuestSheet qs in l)
                {
                    List<Question> vq = qs0.ShallowCopy();
                    int ni = vn[i] - vndiff[i];
                    while (0 < ni)
                    {
                        int idx = rand.Next() % ni;
                        qs.Add(vq.ElementAt(idx).DeepCopy());
                        vq.RemoveAt(idx);
                        --ni;
                    }
                }
                //
                qs0 = new QuestSheet();
                //qs0.bAlt = bAlt;
                qs0.DBSelect(iu, QuestDiff.Diff);
                //
                foreach (QuestSheet qs in l)
                {
                    List<Question> vq = qs0.ShallowCopy();
                    int ni = vndiff[i];
                    while (0 < ni)
                    {
                        int idx = rand.Next() % ni;
                        qs.Add(vq.ElementAt(idx).DeepCopy());
                        vq.RemoveAt(idx);
                        --ni;
                    }
                }
                //
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

        public List<QuestSheet> GenQPack3(int n, int[] vn, int[] vndiff)
        {
            string emsg;
            Random rand = new Random();
            List<QuestSheet> l = new List<QuestSheet>();
            QuestSheet qs0 = new QuestSheet();
            qs0.bAlt = bAlt;
            int j = -1;
            foreach (IUx i in QuestSheet.GetIUs(eLv))
            {
                ++j;
                if (qs0.DBSelect(rand, i, vn[j] - vndiff[j], QuestDiff.Easy, out emsg))
                {
                    WPopup.s.ShowDialog(emsg);
                    return new List<QuestSheet>();
                }
                if (qs0.DBSelect(rand, i, vndiff[j], QuestDiff.Diff, out emsg))
                {
                    WPopup.s.ShowDialog(emsg);
                    return new List<QuestSheet>();
                }
            }
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
                return Txt.s._((int)TxI.DB_DAT_NOK);
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return Txt.s._((int)TxI.DB_NOK);
            StringBuilder vals = new StringBuilder();
            string prefx = "('" + dt.ToString(DT._) + "',";
            foreach (QuestSheet qs in l)
                vals.Append(prefx + "'" + qs.eLv.ToString() + "'," + qs.uId +
                    ",'" + dt.ToString(DT.hh) + "'," +
                    (qs.bAlt ? '1' : '0') + "),");
            vals.Remove(vals.Length - 1, 1);//remove the last comma
            string eMsg;
            if(DBConnect.Ins(conn, "sqz_qsheet", "dt,lv,id,t,alt", vals.ToString(), out eMsg) < 0)
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
            if (DBConnect.Ins(conn, "sqz_qsheet_quest", "dt,lv,qsid,qid,asort,idx", vals.ToString(), out eMsg) < 0)
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

        public List<int[]> GetNMod()
        {
            if (vSheet.Values.Count == 0)
                return null;
            return vSheet.First().Value.GetNMod();
        }

        public void WriteTxt()
        {
            //string extension = ".txt";
            //foreach (QuestSheet qs in vSheet.Values)
            //    qs.WriteTxt(qs.eLv.ToString() + qs.uId + extension);
        }

        public void WriteDocx()
        {
            string extension = ".docx";
            foreach (QuestSheet qs in vSheet.Values)
                QuestSheetDocxPrinter.GetInstance().Print(qs.eLv.ToString() + qs.uId + extension,
                    qs.ToListOfStrings(), mDt.ToString(DT.RR), qs.tId);
        }
    }
}
