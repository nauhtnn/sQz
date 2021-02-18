using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace sQzLib
{
    public class QuestSheet
    {
        public static int globalMaxID_inDB;
        public int ID;
        List<Question> vQuest;
        List<PassageQuestion> PassageQuestions;
        public byte[] aQuest;
        public int Count { get { return vQuest.Count; } }
        public int CountD
        {
            get
            {
                int n = 0;
                foreach (Question q in vQuest)
                    if (q.bDiff)
                        ++n;
                return n;
            }
        }

        public QuestSheet()
        {
            vQuest = new List<Question>();
            aQuest = null;
            ID = -1;
        }

        public Question Q(int idx)
        {
            return vQuest[idx];
        }

        public void Add(Question q)
        {
            vQuest.Add(q);
        }

        public void Clear()
        {
            vQuest.Clear();
        }

        //public static List<int[]> DBGetNMod(ExamLv lv)
        //{
        //    List<int[]> rv = new List<int[]>();
        //    IUx[] viu = GetIUs(lv);
        //    int[] vn = new int[viu.Length];
        //    int[] vnd = new int[viu.Length];
        //    MySqlConnection conn = DBConnect.Init();
        //    if(conn == null)
        //    {
        //        for(int i = 0; i < viu.Length; ++i)
        //            vn[i] = 0;
        //        rv.Add(vn);
        //        rv.Add(vn);
        //        return rv;
        //    }
        //    int j = -1;
        //    foreach(IUx i in viu)
        //    {
        //        string emsg;
        //        int n = DBConnect.Count(conn, "sqz_question", "id",
        //            "moid=" + (int)i + " AND del=0", out emsg);
        //        if (n < 0)
        //            n = 0;
        //        vn[++j] = n;
        //        n = DBConnect.Count(conn, "sqz_question", "id",
        //            "moid=" + (int)i + " AND diff=1 AND del=0", out emsg);
        //        if (n < 0)
        //            n = 0;
        //        vnd[j] = n;
        //    }
        //    rv.Add(vn);
        //    rv.Add(vnd);
        //    return rv;
        //}

        public static int DBGetND(IUx iu)
        {
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return 0;
            string emsg;
            int n = DBConnect.Count(conn, "sqz_question", "id",
                    "AND deleted=0", out emsg);
            if (n < 0)
                n = 0;
            return n;
        }

        public void DBAppendQryIns(string prefx, StringBuilder vals)
        {
            int idx = -1;
            foreach (Question q in vQuest)
            {
                vals.Append(prefx +
                    ID + "," + q.uId + ",'");
                foreach (int i in q.vAnsSort)
                    vals.Append(i.ToString());
                vals.Append("'," + ++idx + "),");
            }
        }

        //only Operation0 uses this.
        public void ExtractKey(AnsSheet anssh)
        {
            anssh.questSheetID = ID;
            if (0 < vQuest.Count)
                anssh.aAns = new byte[vQuest.Count * Question.N_ANS];
            else
                return;
            int i = -1;
            foreach (Question q in vQuest)
                foreach (bool x in q.vKeys)
                    anssh.aAns[++i] = Convert.ToByte(x);
        }

        public List<byte[]> ToByte()
        {
            List<byte[]> l = new List<byte[]>();
            l.Add(BitConverter.GetBytes(ID));
            l.Add(BitConverter.GetBytes(vQuest.Count));
            foreach (Question q in vQuest)
            {
                //stmt
                byte[] b = Encoding.UTF8.GetBytes(q.Stmt);
                l.Add(BitConverter.GetBytes(b.Length));
                l.Add(b);
                //ans
                for (int j = 0; j < Question.N_ANS; ++j)
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
            ID = BitConverter.ToInt32(buf, offs);
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
                //stmt
                if (l < 4)
                    return true;
                int sz = BitConverter.ToInt32(buf, offs);
                l -= 4;
                offs += 4;
                if (l < sz)
                    return true;
                q.Stmt = Encoding.UTF8.GetString(buf, offs, sz);
                l -= sz;
                offs += sz;
                //ans
                q.vAns = new string[Question.N_ANS];
                for (int j = 0; j < Question.N_ANS; ++j)
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
        public void LoadFromFile(string filePath)
        {
            vQuest.Clear();
            PlainTextQuestParser p = new PlainTextQuestParser();
            Tuple<List<Question>, List<PassageQuestion>> tuple = p.ParseLines(PlainTextQueue.GetTextQueue(filePath));
            vQuest = tuple.Item1;
            PassageQuestions = tuple.Item2;
        }

        public void WriteTxt(string fpath)
        {
            System.IO.File.WriteAllText(fpath, ToString());
        }

        public IEnumerable<string> ToListOfStrings()
        {
            IEnumerable<string> s = new LinkedList<string>();
            foreach (Question q in vQuest)
                s = s.Concat(q.ToListOfStrings()) as IEnumerable<string>;
                
            return s;
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
            qs.ID = ID;
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
            qs.ID = ID;
            foreach (Question qi in vQuest)
                qs.vQuest.Add(qi.RandomizeDeepCopy(rand));
            //randomize
            List<Question> lq = new List<Question>();
            int n = qs.vQuest.Count;
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

        //      public int[] DBCount(out string eMsg)
        //{
        //	MySqlConnection conn = DBConnect.Init();
        //          if (conn == null)
        //	{
        //		eMsg = Txt.s._((int)TxI.DB_NOK);
        //              return null;
        //	}
        //          IUx[] ius = new IUx[GetIUs(ExamLv.A).Count() + GetIUs(ExamLv.B).Count()];
        //          int i = -1;
        //          foreach (IUx iu in GetIUs(ExamLv.A))
        //              ius[++i] = iu;
        //          foreach (IUx iu in GetIUs(ExamLv.B))
        //              ius[++i] = iu;
        //	int[] nn = new int[ius.Length];
        //	i = -1;
        //	StringBuilder emsg = new StringBuilder();
        //	foreach(IUx iu in ius)
        //	{
        //		nn[++i] = DBConnect.Count(conn, "sqz_question", "id",
        //			"moid=" + (int)iu + " AND del=0", out eMsg);
        //		if(eMsg != null)
        //			emsg.Append(iu.ToString() + '-' + eMsg);
        //	}
        //	DBConnect.Close(ref conn);
        //	eMsg = emsg.ToString();
        //	return nn;
        //}

        //only Server0 uses this.
        public void DBSelect()
        {
            vQuest.Clear();
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
            {
                return;
            }
            string qry = DBConnect.mkQrySelect("sqz_question",
                "id,stmt,ans0,ans1,ans2,ans3,akey", "deleted=0");
            string emsg;
            MySqlDataReader reader = DBConnect.exeQrySelect(conn, qry, out emsg);
            if (reader != null)
            {
                while (reader.Read())
                {
                    Question q = new Question();
                    q.uId = reader.GetInt32(0);
                    q.Stmt = reader.GetString(1);
                    q.vAns = new string[4];
                    for (int i = 0; i < 4; ++i)
                        q.vAns[i] = reader.GetString(2 + i);
                    string x = reader.GetString(6);
                    q.vKeys = new bool[4];
                    for (int i = 0; i < 4; ++i)
                        q.vKeys[i] = (x[i] == Question.C1);
                    vQuest.Add(q);
                }
                reader.Close();
            }
            else
                WPopup.s.ShowDialog(emsg);
            DBConnect.Close(ref conn);
        }

        //only Server0 uses this.
        public bool DBSelect(int passageID, out string eMsg)
        {
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
            {
                eMsg = Txt.s._((int)TxI.DB_NOK);
                return true;
            }
            string condition;
            if (passageID < 0)
                condition = "pid IS NULL";
            else
                condition = "pid=" + passageID;
            string query = DBConnect.mkQrySelect("sqz_question",
                "id,stmt,ans0,ans1,ans2,ans3,akey", "deleted=0 AND " + condition);
            MySqlDataReader reader = DBConnect.exeQrySelect(conn, query, out eMsg);
            
            if (reader != null)
            {
                while (reader.Read())
                {
                    Question q = new Question();
                    q.uId = reader.GetInt32(0);
                    q.Stmt = reader.GetString(1);
                    q.vAns = new string[Question.N_ANS];
                    for (int j = 0; j < Question.N_ANS; ++j)
                        q.vAns[j] = reader.GetString(2 + j);
                    string x = reader.GetString(5);
                    q.vKeys = new bool[Question.N_ANS];
                    for (int j = 0; j < Question.N_ANS; ++j)
                        q.vKeys[j] = (x[j] == Question.C1);
                    vQuest.Add(q);
                }
                reader.Close();
            }
            DBConnect.Close(ref conn);
            return false;
        }

        public void DBIns()
        {
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return;
            StringBuilder vals = new StringBuilder();
            foreach (Question q in vQuest)
            {
                vals.Append("(NULL,0,'");
                vals.Append(q.Stmt.Replace("'", "\\'") + "','");
                for (int i = 0; i < Question.N_ANS; ++i)
                    vals.Append(q.vAns[i].Replace("'", "\\'") + "','");
                for (int i = 0; i < Question.N_ANS; ++i)
                    if (q.vKeys[i])
                        vals.Append(Question.C1);
                    else
                        vals.Append(Question.C0);
                vals.Append("'),");
            }
            vals.Remove(vals.Length - 1, 1);//remove the last comma
            string eMsg;
            DBConnect.Ins(conn, "sqz_question", "pid,deleted,stmt,ans0,ans1,ans2,ans3,akey",
                vals.ToString(), out eMsg);
            DBConnect.Close(ref conn);
        }

        public bool DBSelect(MySqlConnection conn, DateTime dt, int id, out string eMsg)
        {
            vQuest.Clear();
            ID = id;
            string qry = DBConnect.mkQrySelect("sqz_qsheet_quest", "qid,asort,idx",
                "dt='" + dt.ToString(DT._) +
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
                    "stmt,ans0,ans1,ans2,ans3,akey", "id=" + qid);
                reader = DBConnect.exeQrySelect(conn, qry, out eMsg);
                if (reader == null)
                    return true;
                while (reader.Read())
                {
                    Question q = new Question();
                    q.uId = qid;
                    q.Stmt = reader.GetString(0);
                    string[] anss = new string[Question.N_ANS];
                    for (int j = 0; j < Question.N_ANS; ++j)
                        anss[j] = reader.GetString(1 + j);
                    string x = reader.GetString(5);
                    bool[] keys = new bool[Question.N_ANS];
                    for (int j = 0; j < Question.N_ANS; ++j)
                        keys[j] = (x[j] == Question.C1);
                    q.vAns = new string[4];
                    q.vKeys = new bool[4];
                    for(int j = 0; j < 4; ++j)
                    {
                        q.vAns[j] = anss[asorts[i][j] - Question.C0];
                        q.vKeys[j] = keys[asorts[i][j] - Question.C0];
                    }
                    int iu;
                    if (Enum.IsDefined(typeof(IUx), iu = reader.GetInt32(7)))
                        q.eIU = (IUx)iu;
                    vQuest.Add(q);
                }
                reader.Close();
            }
            vQuest.Sort(qComparer);
            return false;
        }

        public bool UpdateCurQSId()
        {
            if (-1 < globalMaxID_inDB)
            {
                ID = ++globalMaxID_inDB;
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
                    "dt='" + dt.ToString(DT._) + "'");
            if (uid < 0)
            {
                DBConnect.Close(ref conn);
                return true;
            }
            globalMaxID_inDB = uid;

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
            if (vIdx[qx.uId] < vIdx[qy.uId])
                return -1;
            else if (vIdx[qx.uId] == vIdx[qy.uId])
                return 0;
            return 1;

        }
    }

    public enum QuestDiff
    {
        Easy,
        Diff,
        Both
    }
}
