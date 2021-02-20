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
        public static int globalMaxID = -1;
        public int ID;
        List<Question> vQuest;
        Dictionary<int, PassageQuestion> PassageQuestions;
        public byte[] aQuest;
        public int Count { get { return vQuest.Count; } }
        public string CountPassage {
            get {
                StringBuilder sb = new StringBuilder();
                sb.Append(PassageQuestions.Count);
                if (PassageQuestions.Count == 0)
                    return sb.ToString();
                else
                    sb.Append('(');
                foreach (PassageQuestion p in PassageQuestions.Values)
                    sb.Append(p.Questions.Count + ", ");
                sb.Remove(sb.Length - 2, 2);//remove last comma and space
                sb.Append(')');
                return sb.ToString();
            } }

        public QuestSheet()
        {
            vQuest = new List<Question>();
            PassageQuestions = new Dictionary<int, PassageQuestion>();
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
                byte[] b = Encoding.UTF8.GetBytes(q.Stem);
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
                q.Stem = Encoding.UTF8.GetString(buf, offs, sz);
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
            Tuple<List<Question>, List<PassageQuestion>> tuple = p.ParseTokens(PlainTextQueue.GetTextQueue(filePath));
            vQuest = tuple.Item1;
            PassageQuestions = new Dictionary<int, PassageQuestion>();
            int tempt_ID = -1;
            foreach(PassageQuestion passage in tuple.Item2)
            {
                while (PassageQuestions.ContainsKey(tempt_ID))
                    --tempt_ID;
                PassageQuestions.Add(tempt_ID, passage);
            }
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

        public List<Question> ShallowCopyIndependentQuestions()
        {
            List<Question> l = new List<Question>();
            foreach (Question q in vQuest)
                l.Add(q);
            return l;
        }

        public List<PassageQuestion> ShallowCopyPassages()
        {
            List<PassageQuestion> l = new List<PassageQuestion>();
            foreach (PassageQuestion p in PassageQuestions.Values)
                l.Add(p);
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
                System.Windows.MessageBox.Show(Txt.s._((int)TxI.DB_NOK));
                return;
            }
            List<Question> allQuestions = DBSelectNondeletedQuestions(conn);
            SortedSet<int> passageIDs = new SortedSet<int>();
            foreach (Question q in allQuestions)
                if (q.PassageID == -1)
                    vQuest.Add(q);
                else if (!passageIDs.Contains(q.PassageID))
                    passageIDs.Add(q.PassageID);
            DBSelectPassage(conn, passageIDs);
            DBConnect.Close(ref conn);
            foreach(Question q in allQuestions)
            {
                if (q.PassageID > -1 && PassageQuestions.ContainsKey(q.PassageID))
                    PassageQuestions[q.PassageID].Questions.Add(q);
            }
        }

        private void DBSelectPassage(MySqlConnection conn, SortedSet<int> IDs)
        {
            PassageQuestions = new Dictionary<int, PassageQuestion>();
            if (IDs.Count == 0)
                return;
            StringBuilder condition_IDS = new StringBuilder();
            condition_IDS.Append("(");
            foreach (int id in IDs)
                condition_IDS.Append(id + ",");
            condition_IDS.Remove(condition_IDS.Length - 1, 1);//remove last comma
            condition_IDS.Append(")");
            string query = DBConnect.mkQrySelect("sqz_passage",
                "id,psg", "id IN " + condition_IDS);
            string eMsg;
            MySqlDataReader reader = DBConnect.exeQrySelect(conn, query, out eMsg);
            if (reader != null)
            {
                while (reader.Read())
                {
                    PassageQuestion p = new PassageQuestion(reader.GetInt32(0));
                    p.Passage = reader.GetString(1);
                    PassageQuestions.Add(p.ID, p);
                }
                reader.Close();
            }
            else
                System.Windows.MessageBox.Show(eMsg.ToString());
        }

        private List<Question> DBSelectNondeletedQuestions(MySqlConnection conn)
        {
            string query = DBConnect.mkQrySelect("sqz_question",
                "id,pid,stmt,ans0,ans1,ans2,ans3,akey", "deleted=0");
            string eMsg;
            MySqlDataReader reader = DBConnect.exeQrySelect(conn, query, out eMsg);
            List<Question> questions = new List<Question>();

            if (reader != null)
            {
                while (reader.Read())
                {
                    Question q = new Question();
                    q.uId = reader.GetInt32(0);
                    if (reader.IsDBNull(1))
                        q.PassageID = -1;
                    else
                        q.PassageID = reader.GetInt32(1);
                    q.Stem = reader.GetString(2);
                    q.vAns = new string[Question.N_ANS];
                    for (int j = 0; j < Question.N_ANS; ++j)
                        q.vAns[j] = reader.GetString(3 + j);
                    string x = reader.GetString(7);
                    q.vKeys = new bool[Question.N_ANS];
                    for (int j = 0; j < Question.N_ANS; ++j)
                        q.vKeys[j] = (x[j] == Question.C1);
                    questions.Add(q);
                }
                reader.Close();
            }
            else
                System.Windows.MessageBox.Show(eMsg.ToString());
            return questions;
        }

        public void DBIns()
        {
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return;
            StringBuilder questionVals = new StringBuilder();
            foreach (Question q in vQuest)
                AppendQuestionInsertQuery(q, questionVals);
            if (PassageQuestion.GetMaxID_inDB() &&
                System.Windows.MessageBox.Show("Cannot get PassageQuestion.GetMaxID_inDB. Choose Yes to continue and get risky.", "Warning!",
                System.Windows.MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.No)
                return;
            StringBuilder passageVals = new StringBuilder();
            foreach (PassageQuestion p in PassageQuestions.Values)
            {
                p.AccquireGlobalMaxID();
                passageVals.Append("(" + p.ID + ",'" + DBConnect.SafeSQL_Text(p.Passage) + "'),");
                foreach (Question q in p.Questions)
                    AppendQuestionInsertQuery(q, questionVals);
            }
            questionVals.Remove(questionVals.Length - 1, 1);//remove the last comma
            passageVals.Remove(passageVals.Length - 1, 1);//remove the last comma
            string eMsg;
            if (DBConnect.Ins(conn, "sqz_passage", "id,psg",
                passageVals.ToString(), out eMsg) < 0)
            {
                System.Windows.MessageBox.Show("Error inserting passages:\n" + eMsg);
                return;
            }
            if (DBConnect.Ins(conn, "sqz_question", "pid,deleted,stmt,ans0,ans1,ans2,ans3,akey",
                questionVals.ToString(), out eMsg) < 0)
            {
                System.Windows.MessageBox.Show("Error inserting questions:\n" + eMsg);
                return;
            }
            DBConnect.Close(ref conn);
        }

        private void AppendQuestionInsertQuery(Question q, StringBuilder query)
        {
            if (q.PassageID < 0)
                query.Append("(NULL,0,'");
            else
                query.Append("(" + q.PassageID + ",0,'");
            query.Append(DBConnect.SafeSQL_Text(q.Stem) + "','");
            for (int i = 0; i < Question.N_ANS; ++i)
                query.Append(DBConnect.SafeSQL_Text(q.vAns[i]) + "','");
            for (int i = 0; i < Question.N_ANS; ++i)
                if (q.vKeys[i])
                    query.Append(Question.C1);
                else
                    query.Append(Question.C0);
            query.Append("'),");
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
                    q.Stem = reader.GetString(0);
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
                    vQuest.Add(q);
                }
                reader.Close();
            }
            vQuest.Sort(qComparer);
            return false;
        }

        public bool AccquireGlobalMaxID()
        {
            if (-1 < globalMaxID)
            {
                ID = ++globalMaxID;
                return false;
            }
            return true;
        }

        public static bool GetMaxID_inDB(DateTime dt)
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
            globalMaxID = uid;

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
