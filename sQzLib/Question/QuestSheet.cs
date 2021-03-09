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
        public List<QSheetSection> Sections;
        public byte[] aQuest;
        public int TestType_in_DB;
        //public int Count { get { return IndependentQuestions.Count; } }
        //public string CountPassage {
        //    get {
        //        if (Passages.Count == 0)
        //            return "()";
        //        StringBuilder sb = new StringBuilder();
        //        sb.Append("(");
        //        foreach (BasicPassageSection p in Passages.Values)
        //            sb.Append(p.Questions.Count + ", ");
        //        sb.Remove(sb.Length - 2, 2);//remove last comma and space
        //        sb.Append(')');
        //        return sb.ToString();
        //    } }

        public void UpdateQuestIndicesInRequirementPassage()
        {
            int startQuestIdxLabel = 1;
            foreach(QSheetSection section in Sections)
            {
                section.UpdateQuestIndices(startQuestIdxLabel);
                startQuestIdxLabel += section.Questions.Count;
            }
        }

        public int CountAllQuestions()
        {
            int count = 0;
            foreach (QSheetSection section in Sections)
                count += section.CountQuestions();
            return count;
        }

        public QuestSheet()
        {
            Sections = new List<QSheetSection>();
            aQuest = null;
            ID = -1;
        }

        //public Question Q(int idx)
        //{
        //    return IndependentQuestions[idx];
        //}

        //public void Add(Question q)
        //{
        //    IndependentQuestions.Add(q);
        //}

        public void Clear()
        {
            foreach (QSheetSection section in Sections)
                section.Clear();
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

        //public static int DBGetND(IUx iu)
        //{
        //    MySqlConnection conn = DBConnect.Init();
        //    if (conn == null)
        //        return 0;
        //    string emsg;
        //    int n = DBConnect.Count(conn, "sqz_question", "id",
        //            "AND deleted=0", out emsg);
        //    if (n < 0)
        //        n = 0;
        //    return n;
        //}

        public void DBAppendQryIns(string prefx, StringBuilder vals)
        {
            int idx = -1;
            foreach (QSheetSection section in Sections)
                section.DBAppendQryIns(prefx, ref idx, ID, vals);
        }

        //only Operation0 uses this.
        public void ExtractKey(AnswerSheet anssh)
        {
            anssh.QuestSheetID = ID;
            int bytes_length;
            int question_count = 0;
            if (0 < Sections.Count)
            {
                foreach (QSheetSection section in Sections)
                    question_count += section.CountQuestions();
                bytes_length = question_count * Question.NUMBER_OF_OPTIONS;
            }
            else
                bytes_length = 0;
            anssh.BytesOfAnswer_Length = bytes_length;
            anssh.BytesOfAnswer = new byte[bytes_length];
            int i = -1;
            foreach(QSheetSection section in Sections)
                foreach (Question q in section.Questions)
                    foreach (bool x in q.vKeys)
                        anssh.BytesOfAnswer[++i] = Convert.ToByte(x);
        }

        

        

        private Question ReadBytesOfQuestion(byte[] buf, ref int offs)
        {
            Question q = new Question();
            q.Stem = Utils.ReadBytesOfString(buf, ref offs);
            if (q.Stem == null)
                return null;
            //ans
            q.vAns = new string[Question.NUMBER_OF_OPTIONS];
            for (int j = 0; j < Question.NUMBER_OF_OPTIONS; ++j)
            {
                q.vAns[j] = Utils.ReadBytesOfString(buf, ref offs);
                if (q.vAns[j] == null)
                    return null;
            }
            return q;
        }

        private void AppendBytesOf(Question q, List<byte[]> byteList)
        {
            Utils.AppendBytesOfString(q.Stem, byteList);
            foreach (string option in q.vAns)
                Utils.AppendBytesOfString(option, byteList);
        }

        private List<Question> ReadBytesOfQuestions(byte[] buf, ref int offs)
        {
            int l = buf.Length - offs;
            if (l < 4)
                return null;
            int n = BitConverter.ToInt32(buf, offs);
            offs += 4;
            //l -= 4;
            List<Question> questions = new List<Question>();
            while (0 < n)
            {
                Question q = ReadBytesOfQuestion(buf, ref offs);
                if (q == null)
                    return null;
                --n;
                questions.Add(q);
            }
            return questions;
        }

        private BasicPassageSection ReadBytesOfPassage(byte[] buf, ref int offs)
        {
            if (buf.Length < offs + 4)
                return null;
            BasicPassageSection p = new BasicPassageSection(BitConverter.ToInt32(buf, offs));
            offs += 4;
            p.Passage = Utils.ReadBytesOfString(buf, ref offs);
            if (p.Passage == null)
                return null;
            p.Questions = ReadBytesOfQuestions(buf, ref offs);
            if (p.Questions == null)
                return null;
            return p;
        }

        private Dictionary<int, BasicPassageSection> ReadBytesOfPassages(byte[] buf, ref int offs)
        {
            int l = buf.Length - offs;
            if (l < 4)
                return null;
            int n = BitConverter.ToInt32(buf, offs);
            offs += 4;
            //l -= 4;
            Dictionary<int, BasicPassageSection> passages = new Dictionary<int, BasicPassageSection>();
            while (0 < n)
            {
                BasicPassageSection p = ReadBytesOfPassage(buf, ref offs);
                if (p == null)
                    return null;
                --n;
                passages.Add(p.ID, p);
            }
            return passages;
        }

        private void AppendBytesOf(BasicPassageSection p, List<byte[]> byteList)
        {
            byteList.Add(BitConverter.GetBytes(p.ID));
            Utils.AppendBytesOfString(p.Passage, byteList);
            byteList.Add(BitConverter.GetBytes(p.Questions.Count));
            foreach (Question q in p.Questions)
                AppendBytesOf(q, byteList);
        }

        public List<byte[]> ToByte()
        {
            throw new NotImplementedException();
            //List<byte[]> l = new List<byte[]>();
            //l.Add(BitConverter.GetBytes(ID));
            //l.Add(BitConverter.GetBytes(IndependentQuestions.Count));
            //foreach (Question q in IndependentQuestions)
            //    AppendBytesOf(q, l);
            //l.Add(BitConverter.GetBytes(Passages.Count));
            //foreach (BasicPassageSection p in Passages.Values)
            //    AppendBytesOf(p, l);
            //return l;
        }

        public bool ReadByte(byte[] buf, ref int offs)
        {
            throw new NotImplementedException();
            //if (buf == null)
            //    return true;
            //int offs0 = offs;
            //int l = buf.Length - offs;
            ////
            //if (l < 4)
            //    return true;
            //ID = BitConverter.ToInt32(buf, offs);
            //offs += 4;
            ////l -= 4;
            //IndependentQuestions = ReadBytesOfQuestions(buf, ref offs);
            //if (IndependentQuestions == null)
            //    return true;
            //Passages = ReadBytesOfPassages(buf, ref offs);
            //if (Passages == null)
            //    return true;
            //if (!Array.Equals(buf, aQuest))
            //{
            //    int sz = offs - offs0;
            //    if (sz == buf.Length)
            //        aQuest = buf.Clone() as byte[];
            //    else
            //    {
            //        aQuest = new byte[sz];
            //        Buffer.BlockCopy(buf, offs0, aQuest, 0, sz);
            //    }
            //}
            //return false;
        }

        //only Prep0 uses this.
        public void LoadFromFile(string filePath)
        {
            BasicRich_PlainTextParsingMgr parsingMgr = new BasicRich_PlainTextParsingMgr();
            Sections = parsingMgr.ParseTokens(BasicRich_PlainTextQueue.GetTextQueue(filePath));
        }

        public void WriteTxt(string fpath)
        {
            System.IO.File.WriteAllText(fpath, ToString());
        }

        public IEnumerable<string> ToListOfStrings()
        {
            throw new NotImplementedException();
            //IEnumerable<string> s = new LinkedList<string>();
            //foreach (Question q in IndependentQuestions)
            //    s = s.Concat(q.ToListOfStrings()) as IEnumerable<string>;
                
            //return s;
        }

        public List<Question> ShallowCopyIndependentQuestions()
        {
            throw new NotImplementedException();
            //List<Question> l = new List<Question>();
            //foreach (Question q in IndependentQuestions)
            //    l.Add(q);
            //return l;
        }

        public List<BasicPassageSection> ShallowCopyPassages()
        {
            throw new NotImplementedException();
            //List<BasicPassageSection> l = new List<BasicPassageSection>();
            //foreach (BasicPassageSection p in Passages.Values)
            //    l.Add(p);
            //return l;
        }

        public QuestSheet DeepCopy()
        {
            throw new NotImplementedException();
            //QuestSheet qs = new QuestSheet();
            //qs.ID = ID;
            //foreach (Question qi in IndependentQuestions)
            //    qs.IndependentQuestions.Add(qi.DeepCopy());
            //return qs;
        }

        public void Randomize(Random rand)
        {
            throw new NotImplementedException();
            //List<Question> qs = new List<Question>();
            //int n = IndependentQuestions.Count;
            //while (0 < n)
            //{
            //    int sel = rand.Next() % n;
            //    qs.Add(IndependentQuestions[sel]);
            //    IndependentQuestions.RemoveAt(sel);
            //    --n;
            //}
            //IndependentQuestions = qs;
            //foreach (Question q in IndependentQuestions)
            //    q.Randomize(rand);
        }

        public QuestSheet RandomizeDeepCopy(Random rand)
        {
            throw new NotImplementedException();
            //QuestSheet sheet = new QuestSheet();
            //sheet.ID = ID;
            //sheet.IndependentQuestions = RandomizeDeepCopy(rand, IndependentQuestions);
            //foreach(BasicPassageSection p in Passages.Values)
            //{
            //    BasicPassageSection passage = new BasicPassageSection(p.ID);
            //    passage.Passage = p.Passage;
            //    passage.Questions = RandomizeDeepCopy_KeepQuestionOrder(rand, p.Questions);
            //    sheet.Passages.Add(passage.ID, passage);
            //}

            //return sheet;
        }

        private List<Question> RandomizeDeepCopy_KeepQuestionOrder(Random rand, List<Question> originalList)
        {
            List<Question> newList = new List<Question>();
            foreach (Question q in originalList)
                newList.Add(q.RandomizeDeepCopy(rand));
            return newList;
        }

        private List<Question> RandomizeDeepCopy(Random rand, List<Question> originalList)
        {
            List<Question> tempList = new List<Question>();
            foreach (Question q in originalList)
                tempList.Add(q.RandomizeDeepCopy(rand));
            //randomize
            List<Question> newList = new List<Question>();
            int n = tempList.Count;
            while (0 < n)
            {
                int idx = rand.Next() % n;
                newList.Add(tempList.ElementAt(idx));
                tempList.RemoveAt(idx);
                --n;
            }
            return newList;
        }

        //only Server0 uses this.
        public void DBSelectNondeletedQuestions(int testType)
        {
            TestType_in_DB = testType;
            Sections.Clear();
            MySqlConnection conn = DBConnect.OpenNewConnection();
            if (conn == null)
            {
                System.Windows.MessageBox.Show(Txt.s._((int)TxI.DB_NOK));
                return;
            }
            List<Question> allQuestions = DBSelectQuestions(conn, "deleted=0 AND t_type=" + TestType_in_DB);
            DBSelectSections(conn, allQuestions);
            DBConnect.Close(ref conn);
        }

        private void DBSelectSections(MySqlConnection conn, List<Question> questions)
        {
            Dictionary<int, QSheetSection> tempSections = new Dictionary<int, QSheetSection>();
            foreach (Question q in questions)
                if (!tempSections.ContainsKey(q.SectionID))
                    tempSections.Add(q.SectionID, null);
            if (tempSections.Count == 0)
                return;
            StringBuilder condition_IDs = new StringBuilder();
            condition_IDs.Append("(");
            foreach (int id in tempSections.Keys)
                condition_IDs.Append(id + ",");
            condition_IDs.Remove(condition_IDs.Length - 1, 1);//remove last comma
            condition_IDs.Append(")");
            string query = DBConnect.mkQrySelect("sqz_section",
                "id,s_type,req,psg", "id IN " + condition_IDs);
            string eMsg;
            MySqlDataReader reader = DBConnect.exeQrySelect(conn, query, out eMsg);
            if (reader != null)
            {
                while (reader.Read())
                {
                    int sectionTypeID = reader.GetInt32(1);
                    if (!Enum.IsDefined(typeof(SectionTypeID), sectionTypeID))
                        continue;
                    switch((SectionTypeID)sectionTypeID)
                    {
                        case SectionTypeID.DefaultIndependentQuestions:
                            IndependentQSection ind_section = new IndependentQSection(reader.GetInt32(0));
                            ind_section.Requirements = reader.GetString(2);
                            Sections.Add(ind_section);
                            if(tempSections[ind_section.ID] == null)
                                tempSections[ind_section.ID] = ind_section;
                            else
                                System.Windows.MessageBox.Show("Warining: DBSelectSections has duplicated section ID: " + ind_section.ID);
                            break;
                        case SectionTypeID.PassageWithBlanks:
                        case SectionTypeID.BasicPassage:
                            BasicPassageSection passage_section = new BasicPassageSection(reader.GetInt32(0));
                            passage_section.Requirements = reader.GetString(2);
                            passage_section.Passage = reader.GetString(3);
                            Sections.Add(passage_section);
                            if (tempSections[passage_section.ID] == null)
                                tempSections[passage_section.ID] = passage_section;
                            else
                                System.Windows.MessageBox.Show("Warining: DBSelectSections has duplicated section ID: " + passage_section.ID);
                            break;
                    }
                }
                reader.Close();
            }
            else
                System.Windows.MessageBox.Show(eMsg.ToString());
            foreach (Question q in questions)
            {
                if (q.SectionID > -1 && tempSections.ContainsKey(q.SectionID))
                    tempSections[q.SectionID].Questions.Add(q);
            }
        }

        private Question DBReader_CreateQuestion(MySqlDataReader reader)
        {
            Question q = new Question();
            q.uId = reader.GetInt32(0);
            if (reader.IsDBNull(1))
                q.SectionID = -1;
            else
                q.SectionID = reader.GetInt32(1);
            q.Stem = reader.GetString(2);
            q.vAns = new string[Question.NUMBER_OF_OPTIONS];
            for (int j = 0; j < Question.NUMBER_OF_OPTIONS; ++j)
                q.vAns[j] = reader.GetString(3 + j);
            string x = reader.GetString(7);
            q.vKeys = new bool[Question.NUMBER_OF_OPTIONS];
            for (int j = 0; j < Question.NUMBER_OF_OPTIONS; ++j)
                q.vKeys[j] = (x[j] == Question.C1);
            return q;
        }

        private List<Question> DBSelectQuestions(MySqlConnection conn, string condition)
        {
            string query = DBConnect.mkQrySelect("sqz_question",
                "id,secid,stem,ans0,ans1,ans2,ans3,akey", condition);
            string eMsg;
            MySqlDataReader reader = DBConnect.exeQrySelect(conn, query, out eMsg);
            List<Question> questions = new List<Question>();

            if (reader != null)
            {
                while (reader.Read())
                    questions.Add(DBReader_CreateQuestion(reader));
                reader.Close();
            }
            else
                System.Windows.MessageBox.Show(eMsg.ToString());
            return questions;
        }

        public void DBInsertOriginQuestions()
        {
            if (!QSheetSection.GetMaxID_inDB())
                return;
            MySqlConnection conn = DBConnect.OpenNewConnection();
            if (conn == null)
                return;
            StringBuilder questionVals = new StringBuilder();
            StringBuilder sectionVals = new StringBuilder();
            foreach (QSheetSection section in Sections)
            {
                section.AccquireGlobalMaxID();
                sectionVals.Append("(" + section.ID + "," + section.GetSectionTypeID() +
                    ",'" + DBConnect.SafeSQL_Text(section.Requirements) + "',");
                BasicPassageSection passageSection = section as BasicPassageSection;
                if (passageSection != null)
                    sectionVals.Append("'" + DBConnect.SafeSQL_Text(passageSection.Passage) + "'),");
                else
                    sectionVals.Append("NULL),");
                //sectionVals.Append("NULL),"); TODO: manual config later
                foreach (Question q in section.Questions)
                    AppendQuestionInsertQuery(q, questionVals);
            }
            string eMsg;
            if (sectionVals.Length > 0)
            {
                sectionVals.Remove(sectionVals.Length - 1, 1);//remove the last comma
                if (DBConnect.Ins(conn, "sqz_section", "id,s_type,req,psg",
                sectionVals.ToString(), out eMsg) < 0)
                    System.Windows.MessageBox.Show("Error inserting passages:\n" + eMsg);
            }
            if (questionVals.Length > 0)
            {
                DB_InsertTestType_ifNExists(conn, TestType_in_DB);
                questionVals.Remove(questionVals.Length - 1, 1);//remove the last comma
                if (DBConnect.Ins(conn, "sqz_question", "t_type,secid,deleted,stem,ans0,ans1,ans2,ans3,akey",
                questionVals.ToString(), out eMsg) < 0)
                    System.Windows.MessageBox.Show("Error inserting questions:\n" + eMsg);
            }

            DBConnect.Close(ref conn);
        }

        public static void DB_InsertTestType_ifNExists(MySqlConnection conn, int testType)
        {
            string emsg;
            if (DBConnect.NExist(conn, "sqz_test_type", "id=" + testType, out emsg))
                DBConnect.Ins(conn, "sqz_test_type", "id", "(" + testType + ")", out emsg);
        }

        private void AppendQuestionInsertQuery(Question q, StringBuilder query)
        {
            query.Append("(" + TestType_in_DB + ",");
            if (q.SectionID < 0)
                query.Append("NULL,0,'");
            else
                query.Append(q.SectionID + ",0,'");
            query.Append(DBConnect.SafeSQL_Text(q.Stem) + "','");
            for (int i = 0; i < Question.NUMBER_OF_OPTIONS; ++i)
                query.Append(DBConnect.SafeSQL_Text(q.vAns[i]) + "','");
            for (int i = 0; i < Question.NUMBER_OF_OPTIONS; ++i)
                if (q.vKeys[i])
                    query.Append(Question.C1);
                else
                    query.Append(Question.C0);
            query.Append("'),");
        }

        public bool DBSelect(MySqlConnection conn, DateTime dt, int sheetID, out string eMsg)
        {
            Sections.Clear();
            ID = sheetID;
            string qry = DBConnect.mkQrySelect("sqz_qsheet_quest", "qid,asort,idx",
                "dt='" + dt.ToString(DT._) +
                "' AND qsid=" + sheetID);
            MySqlDataReader reader = DBConnect.exeQrySelect(conn, qry, out eMsg);
            if (reader == null)
                return true;
            List<uint> questionIDs = new List<uint>();
            List<string> options_sorts = new List<string>();
            QIdxComparer<Question> qComparer = new QIdxComparer<Question>();
            while (reader.Read())
            {
                uint qid = reader.GetUInt32(0);
                questionIDs.Add(qid);
                options_sorts.Add(reader.GetString(1));
                qComparer.Add((int)qid, reader.GetInt32(2));
            }
            reader.Close();
            StringBuilder condition_IDs = new StringBuilder();
            condition_IDs.Append("(");
            foreach (int id in questionIDs)
                condition_IDs.Append(id.ToString() + ",");
            condition_IDs.Remove(condition_IDs.Length - 1, 1); //remove the last comma
            condition_IDs.Append(")");

            List<Question> questions = DBSelectQuestions(conn, "id IN " + condition_IDs);
            int i = -1;
            foreach (Question q in questions)
            {
                ++i;
                string[] sorted_answers = new string[Question.NUMBER_OF_OPTIONS];
                bool[] sorted_keys = new bool[4];
                for (int j = 0; j < 4; ++j)
                {
                    sorted_answers[j] = q.vAns[options_sorts[i][j] - Question.C0];
                    sorted_keys[j] = q.vKeys[options_sorts[i][j] - Question.C0];
                }
                q.vAns = sorted_answers;
                q.vKeys = sorted_keys;
            }
            //foreach (Question q in questions)
            //    if (q.PassageID == -1)
            //        IndependentQuestions.Add(q);
            //IndependentQuestions.Sort(qComparer);
            DBSelectSections(conn, questions);

            return false;
        }

        public void AccquireGlobalMaxID()
        {
            ID = ++globalMaxID;
        }

        public static bool GetMaxID_inDB(DateTime dt)
        {
            MySqlConnection conn = DBConnect.OpenNewConnection();
            if (conn == null)
                return false;
            int uid = DBConnect.MaxInt(conn, "sqz_qsheet", "id",
                    "dt='" + dt.ToString(DT._) + "'");
            DBConnect.Close(ref conn);
            if (uid < 0 &&
                System.Windows.MessageBox.Show("Cannot get QuestSheet.GetMaxID_inDB. Choose Yes to continue and get risky!",
                    "Warning!", System.Windows.MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.No)
                    return false;

            globalMaxID = uid;

            return true;
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
