using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

/*
CREATE TABLE IF NOT EXISTS `qs` (`dt` DATE,
`id` SMALLINT UNSIGNED, `lv` CHAR, `vquest` VARCHAR(1024),
PRIMARY KEY(`dt`,`id`), FOREIGN KEY(`dt`) REFERENCES `board`(`dt`));
*/

namespace sQzLib
{
    public class QuestSheet
    {
        public ExamLv eLv;
        public static int guDBCurAId;
        public static int guDBCurBId;
        public int uId;
        public int LvId { get { return (eLv == ExamLv.A) ? uId : uId + ExamineeA.LV_CAP; } }
        public string tId { get { return eLv.ToString() + uId.ToString("d3"); } }
        public int mDiff;
        public List<Question> vQuest;
        public byte[] aQuest;

        public QuestSheet()
        {
            eLv = ExamLv.A;
            vQuest = new List<Question>();
            aQuest = null;
            uId = 0;
            mDiff = 0;
        }

        public static IUx[] GetIUs(ExamLv lv)
        {
            IUx[] x;
            if (lv == ExamLv.A)
            {
                x = new IUx[6];
                x[0] = IUx._1;
                x[1] = IUx._2;
                x[2] = IUx._3;
                x[3] = IUx._4;
                x[4] = IUx._5;
                x[5] = IUx._6;
            }
            else
            {
                x = new IUx[3];
                x[0] = IUx._7;
                x[1] = IUx._8;
                x[2] = IUx._10;
            }
            return x;
        }

        public static bool ParseLvId(string s, out ExamLv lv, out int id)
        {
            if (s == null || s.Length != 4)
            {
                lv = ExamLv.A;
                id = ushort.MaxValue;
                return true;
            }
            s = s.ToUpper();
            if (!Enum.TryParse(s.Substring(0, 1), out lv))
            {
                id = ushort.MaxValue;
                return true;
            }
            if (!int.TryParse(s.Substring(1), out id))
                return true;
            if (id < 1 || ExamineeA.LV_CAP < id)
                return true;
            return false;
        }

        //only Optimization0 uses this.
        //optimization: return List<byte[]> instead of byte[]
        public List<byte[]> ToByte()
        {
            List<byte[]> l = new List<byte[]>();
            l.Add(BitConverter.GetBytes(uId));
            l.Add(BitConverter.GetBytes(vQuest.Count));
            foreach (Question q in vQuest)
            {
                //qType
                //l.Add(BitConverter.GetBytes((int)q.qType));
                //stmt
                byte[] b = Encoding.UTF8.GetBytes(q.mStmt);
                l.Add(BitConverter.GetBytes(b.Length));
                l.Add(b);
                //ans
                //l.Add(BitConverter.GetBytes(q.nAns));
                for (int j = 0; j < q.nAns; ++j)
                {
                    //each ans
                    b = Encoding.UTF8.GetBytes(q.vAns[j]);
                    l.Add(BitConverter.GetBytes(b.Length));
                    l.Add(b);
                }
            }
            return l;
        }

        public bool ReadByte(byte[] buf, ref int offs)
        {
            if (buf == null)
                return true;
            int offs0 = offs;
            int l = buf.Length - offs;
            //
            if (l < 8)
                return true;
            uId = BitConverter.ToInt32(buf, offs);
            offs += 4;
            l -= 4;
            int nq = BitConverter.ToInt32(buf, offs);
            offs += 4;
            l -= 4;
            //
            vQuest = new List<Question>();
            while (0 < nq)
            {
                Question q = new Question();
                //qType
                //if (l < 4)
                //    break;
                //q.qType = (QuestType)BitConverter.ToInt32(buf, offs);
                //l -= 4;
                //offs += 4;
                //stmt
                if (l < 4)
                    return true;
                int sz = BitConverter.ToInt32(buf, offs);
                l -= 4;
                offs += 4;
                if (l < sz)
                    return true;
                q.mStmt = Encoding.UTF8.GetString(buf, offs, sz);
                l -= sz;
                offs += sz;
                //ans
                //if (l < 4)
                //{
                //    err = true;
                //    break;
                //}
                //q.nAns = BitConverter.ToInt32(buf, offs);
                q.nAns = 4;//todo
                //l -= 4;
                //offs += 4;
                q.vAns = new string[q.nAns];
                for (int j = 0; j < q.nAns; ++j)
                {
                    //each ans
                    if (l < 4)
                        return true;
                    sz = BitConverter.ToInt32(buf, offs);
                    l -= 4;
                    offs += 4;
                    if (l < sz)
                        return true;
                    q.vAns[j] = Encoding.UTF8.GetString(buf, offs, sz);
                    l -= sz;
                    offs += sz;
                }
                --nq;
                vQuest.Add(q);
            }
            if (!Array.Equals(buf, aQuest))
            {
                int sz = offs - offs0;
                if (sz == buf.Length)
                    aQuest = buf.Clone() as byte[];
                else
                {
                    aQuest = new byte[sz];
                    Buffer.BlockCopy(buf, offs0, aQuest, 0, sz);
                }
            }
            return false;
        }

        //only Prep0 uses this.
        public void ReadTxt(string buf)
        {
            vQuest.Clear();
            if (buf == null)
                return;
            Question.StartRead(Utils.Split(buf, '\n'), null);
            Question q = new Question();
            while (q.Read())
            {
                vQuest.Add(q);
                q = new Question();
            }
            q = null;
        }

        public List<Question> ShallowCopy()
        {
            List<Question> l = new List<Question>();
            foreach (Question q in vQuest)
                l.Add(q);
            return l;
        }

        public QuestSheet DeepCopy()
        {
            QuestSheet qs = new QuestSheet();
            qs.eLv = eLv;
            qs.uId = uId;
            qs.mDiff = mDiff;
            foreach (Question qi in vQuest)
                qs.vQuest.Add(qi.DeepCopy());
            return qs;
        }

        public void Randomize(Random rand)
        {
            List<Question> qs = new List<Question>();
            int n = vQuest.Count;
            while (0 < n)
            {
                int sel = rand.Next() % n;
                qs.Add(vQuest[sel]);
                vQuest.RemoveAt(sel);
                --n;
            }
            vQuest = qs;
            foreach (Question q in vQuest)
                q.Randomize(rand);
        }

        public QuestSheet RandomizeDeepCopy(Random rand)
        {
            QuestSheet qs = new QuestSheet();
            qs.eLv = eLv;
            qs.uId = uId;
            qs.mDiff = mDiff;
            foreach (Question qi in vQuest)
                qs.vQuest.Add(qi.RandomizeDeepCopy(rand));
            //randomize
            List<Question> lq = new List<Question>();
            List<int> l = new List<int>();
            int n = qs.vQuest.Count;
            for (int i = 0; i < n; ++i)
                l.Add(i);
            while (0 < n)
            {
                int idx = rand.Next() % n;
                lq.Add(qs.vQuest.ElementAt(idx));
                qs.vQuest.RemoveAt(idx);
                --n;
            }
            qs.vQuest = lq;

            return qs;
        }

        public int[] DBCount(out string eMsg)
		{
			MySqlConnection conn = DBConnect.Init();
            if (conn == null)
			{
				eMsg = Txt.s._[(int)TxI.DB_NOK];
                return null;
			}
            IUx[] ius = new IUx[GetIUs(ExamLv.A).Count() + GetIUs(ExamLv.B).Count()];
            int i = -1;
            foreach (IUx iu in GetIUs(ExamLv.A))
                ius[++i] = iu;
            foreach (IUx iu in GetIUs(ExamLv.B))
                ius[++i] = iu;
			int[] nn = new int[ius.Length];
			i = -1;
			StringBuilder emsg = new StringBuilder();
			foreach(IUx iu in ius)
			{
				nn[++i] = DBConnect.Count(conn, "sqz_question", "id",
					"moid=" + (int)iu + " AND del=0", out eMsg);
				if(eMsg != null)
					emsg.Append(iu.ToString() + '-' + eMsg);
			}
			DBConnect.Close(ref conn);
			eMsg = emsg.ToString();
			return nn;
		}

        //only Server0 uses this.
        public void DBSelect(IUx eIU)
        {
            vQuest.Clear();
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
			{
                return;
			}
            string qry = DBConnect.mkQrySelect("sqz_question",
                "id,stmt,ans0,ans1,ans2,ans3,`key`", "moid=" + (int)eIU + " AND del=0");
            string emsg;
            MySqlDataReader reader = DBConnect.exeQrySelect(conn, qry, out emsg);
            if (reader != null)
            {
                while (reader.Read())
                {
                    Question q = new Question();
                    q.uId = reader.GetInt32(0);
                    q.mStmt = reader.GetString(1);
                    q.nAns = 4;
                    q.vAns = new string[4];
                    for (int i = 0; i < 4; ++i)
                        q.vAns[i] = reader.GetString(2 + i);
                    string x = reader.GetString(6);
                    q.vKeys = new bool[4];
                    for (int i = 0; i < 4; ++i)
                        q.vKeys[i] = (x[i] == '1');
                    q.mIU = eIU;
                    vQuest.Add(q);
                }
                reader.Close();
            }
            else
                WPopup.s.ShowDialog(emsg);
            DBConnect.Close(ref conn);
        }

        //only Server0 uses this.
        public bool DBSelect(Random rand, IUx iu, int n, out string eMsg)
        {
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
            {
                eMsg = Txt.s._[(int)TxI.DB_NOK];
                return true;
            }
            //randomize
            int nn = DBConnect.Count(conn, "sqz_question", "id",
                "moid=" + (int)iu + " AND del=0", out eMsg);
            if (nn < 1 || nn < n)
                return true;
            List<int> vIds = new List<int>();
            int i;
            for (i = 0; i < nn; ++i)
                vIds.Add(i);
            int[] vSel = new int[n];
            i = n;
            while (0 < i)
            {
                --i;
                int idx = rand.Next() % nn;
                --nn;
                vSel[i] = vIds[idx];
                vIds.RemoveAt(idx);
            }
            Array.Sort(vSel);
            //
            string qry = DBConnect.mkQrySelect("sqz_question",
                "id,stmt,ans0,ans1,ans2,ans3,`key`", "moid=" + (int)iu + " AND del=0");
            MySqlDataReader reader = DBConnect.exeQrySelect(conn, qry, out eMsg);
            i = 0;
            int ii = -1;
            if (reader != null)
            {
                while (reader.Read() && i < n)
                {
                    if (++ii != vSel[i])
                        continue;
                    ++i;
                    Question q = new Question();
                    q.uId = reader.GetInt32(0);
                    q.mStmt = reader.GetString(1);
                    q.nAns = 4;
                    q.vAns = new string[4];
                    for (int j = 0; j < 4; ++j)
                        q.vAns[j] = reader.GetString(2 + j);
                    string x = reader.GetString(6);
                    q.vKeys = new bool[4];
                    for (int j = 0; j < 4; ++j)
                        q.vKeys[j] = (x[j] == '1');
                    q.mIU = iu;
                    vQuest.Add(q);
                }
                reader.Close();
            }
            DBConnect.Close(ref conn);
            return false;
        }

        public void DBInsert(IUx eIU)
        {
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return;
            StringBuilder vals = new StringBuilder();
            foreach (Question q in vQuest)
            {
                vals.Append("(" + (int)eIU + ",0," + mDiff + ",'");
                vals.Append(q.mStmt.Replace("'", "\\'") + "','");
                for (int i = 0; i < q.nAns; ++i)
                    vals.Append(q.vAns[i].Replace("'", "\\'") + "','");
                for (int i = 0; i < q.nAns; ++i)
                    if (q.vKeys[i])
                        vals.Append('1');
                    else
                        vals.Append('0');
                vals.Append("'),");
            }
            vals.Remove(vals.Length - 1, 1);//remove the last comma
            string eMsg;
            DBConnect.Ins(conn, "sqz_question", "moid,del,diff,stmt,ans0,ans1,ans2,ans3,`key`",
                vals.ToString(), out eMsg);
            DBConnect.Close(ref conn);
        }

        public bool DBSelect(MySqlConnection conn, DateTime dt, ExamLv lv, int id, out string eMsg)
        {
            vQuest.Clear();
            uId = id;
            string qry = DBConnect.mkQrySelect("sqz_qsheet_quest", "qid,asort,idx",
                "dt='" + dt.ToString(DT._) + "' AND lv='" + lv.ToString() +
                "' AND qsid=" + id);
            MySqlDataReader reader = DBConnect.exeQrySelect(conn, qry, out eMsg);
            if (reader == null)
                return true;
            List<uint> qids = new List<uint>();
            List<string> asorts = new List<string>();
            QIdxComparer<Question> qComparer = new QIdxComparer<Question>();
            while (reader.Read())
            {
                uint qid = reader.GetUInt32(0);
                qids.Add(qid);
                asorts.Add(reader.GetString(1));
                qComparer.Add((int)qid, reader.GetInt16(2));
            }
            reader.Close();
            int i = -1;
            foreach(int qid in qids)
            {
                ++i;
                qry = DBConnect.mkQrySelect("sqz_question",
                    "stmt,ans0,ans1,ans2,ans3,`key`,moid", "id=" + qid);
                reader = DBConnect.exeQrySelect(conn, qry, out eMsg);
                if (reader == null)
                    return true;
                while (reader.Read())
                {
                    Question q = new Question();
                    q.uId = qid;
                    q.mStmt = reader.GetString(0);
                    q.nAns = 4;
                    string[] anss = new string[4];
                    for (int j = 0; j < 4; ++j)
                        anss[j] = reader.GetString(1 + j);
                    string x = reader.GetString(5);
                    bool[] keys = new bool[4];
                    for (int j = 0; j < 4; ++j)
                        keys[j] = (x[j] == '1');
                    q.vAns = new string[4];
                    q.vKeys = new bool[4];
                    for(int j = 0; j < 4; ++j)
                    {
                        q.vAns[j] = anss[asorts[i][j] - '0'];
                        q.vKeys[j] = keys[asorts[i][j] - '0'];
                    }
                    int iu;
                    if (Enum.IsDefined(typeof(IUx), iu = reader.GetInt32(6)))
                        q.mIU = (IUx)iu;
                    vQuest.Add(q);
                }
                reader.Close();
            }
            vQuest.Sort(qComparer);
            return false;
        }

        public bool UpdateCurQSId()
        {
            if (eLv == ExamLv.A && -1 < guDBCurAId)
            {
                uId = ++guDBCurAId;
                return false;
            }
            if (eLv == ExamLv.B && -1 < guDBCurBId)
            {
                uId = ++guDBCurBId;
                return false;
            }
            return true;
        }

        public static bool DBUpdateCurQSId(DateTime dt)
        {
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return true;
            int uid = DBConnect.MaxInt(conn, "sqz_qsheet", "id",
                    "dt='" + dt.ToString(DT._) + "' AND lv='" + ExamLv.A.ToString() + "'");
            if (uid < 0)
            {
                DBConnect.Close(ref conn);
                return true;
            }
            guDBCurAId = uid;

            uid = DBConnect.MaxInt(conn, "sqz_qsheet", "id",
                    "dt='" + dt.ToString(DT._) + "' AND lv='" + ExamLv.B.ToString() + "'");
            if (uid < 0)
            {
                DBConnect.Close(ref conn);
                return true;
            }
            guDBCurBId = uid;

            return false;
        }
    }

    public class QIdxComparer<T> : Comparer<T>
    {
        SortedList<int, int> vIdx;

        public QIdxComparer()
        {
            vIdx = new SortedList<int, int>();
        }

        public void Add(int qid, int idx)
        {
            vIdx.Add(qid, idx);
        }

        public override int Compare(T x, T y)
        {
            Question qx = x as Question;
            if (qx == null)
                return 0;
            Question qy = y as Question;
            if (qy == null)
                return 0;
            if (vIdx[qx.uId] == vIdx[qy.uId])
                return 0;
            else if (vIdx[qx.uId] < vIdx[qy.uId])
                return -1;
            return 1;

        }
    }
}
