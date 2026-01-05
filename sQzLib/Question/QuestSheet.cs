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
        public int Subject;
        private QIdxComparer<Question> SavedQuestOrderInDB;
        private QIdxComparer<QSheetSection> SavedSectionOrderInDB;
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

        public string GetGlobalID_withSubject()
        {
            return (Subject * 1000 + ID).ToString("d4");
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
        public void ExtractKey(AnswerSheet answerSheet)
        {
            answerSheet.QuestSheetID = ID;
            answerSheet.Subject = Subject;
            int bytes_length;
            int question_count = 0;
            if (0 < Sections.Count)
            {
                foreach (QSheetSection section in Sections)
                    question_count += section.CountQuestions();
                bytes_length = question_count * OptionSelectAnswer.OPTION_COUNT;
            }
            else
                bytes_length = 0;
            answerSheet.BytesOfAnswer_Length = bytes_length;
            answerSheet.BytesOfAnswer = new byte[bytes_length];
            int i = -1;
            foreach(QSheetSection section in Sections)
                foreach (Question q in section.Questions)
                    foreach (char x in q.Answer)
                        answerSheet.BytesOfAnswer[++i] = Convert.ToByte(x);
        }

        private Question ReadBytesOfQuestion(byte[] buf, ref int offs)
        {
            Question q = new Question();
            int questionType;
            if (!Enum.IsDefined(typeof(AnswerType), questionType = BitConverter.ToInt32(buf, offs)))
                return null;
            offs += 4;
            q.QuestionType = (AnswerType) questionType;
            q.Stem = Utils.ReadBytesOfString(buf, ref offs);
            if (q.Stem == null)
                return null;
            //ans
            q.Options = new string[OptionSelectAnswer.OPTION_COUNT];
            for (int j = 0; j < OptionSelectAnswer.OPTION_COUNT; ++j)
            {
                q.Options[j] = Utils.ReadBytesOfString(buf, ref offs);
                if (q.Options[j] == null)
                    return null;
            }
            return q;
        }

        private void AppendBytesOf(Question q, List<byte[]> byteList)
        {
            byteList.Add(BitConverter.GetBytes((int)q.QuestionType));
            Utils.AppendBytesOfString(q.Stem, byteList);
            foreach (string option in q.Options)
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

        private IndependentQSection ReadBytesOfIndependentQSection(byte[] buf, ref int offs)
        {
            if (buf.Length < offs + 4)
                return null;
            IndependentQSection sec = new IndependentQSection(BitConverter.ToInt32(buf, offs));
            offs += 4;
            sec.Requirements = Utils.ReadBytesOfString(buf, ref offs);
            if (sec.Requirements == null)
                return null;
            sec.Questions = ReadBytesOfQuestions(buf, ref offs);
            if (sec.Questions == null)
                return null;
            return sec;
        }

        private bool ReadBytesOfBasicPassageSection(BasicPassageSection section, byte[] buf, ref int offs)
        {
            section.Requirements = Utils.ReadBytesOfString(buf, ref offs);
            if (section.Requirements == null)
                return false;
            section.Passage = Utils.ReadBytesOfString(buf, ref offs);
            if (section.Passage == null)
                return false;
            section.Questions = ReadBytesOfQuestions(buf, ref offs);
            if (section.Questions == null)
                return false;
            return true;
        }

        private bool ReadBytesOfSections(byte[] buf, ref int offs)
        {
            if (buf.Length - offs < 4)
                return false;
            int n = BitConverter.ToInt32(buf, offs);
            offs += 4;

            Sections.Clear();

            while (0 < n)
            {
                if (buf.Length - offs < 4)
                    return false;

                SectionTypeID sec_typeID = SectionTypeID.DefaultIndependentQuestions;
                if (Enum.IsDefined(typeof(SectionTypeID), BitConverter.ToInt32(buf, offs)))
                    sec_typeID = (SectionTypeID)BitConverter.ToInt32(buf, offs);
                else
                    return false;

                offs += 4;

                switch (sec_typeID)
                {
                    case SectionTypeID.PassageWithBlanks:
                        if (buf.Length < offs + 4)
                            return false;
                        PassageWithBlanks p_blank = new PassageWithBlanks(BitConverter.ToInt32(buf, offs));
                        offs += 4;
                        if (!ReadBytesOfBasicPassageSection(p_blank, buf, ref offs))
                            return false;
                        Sections.Add(p_blank);
                        break;
                    case SectionTypeID.BasicPassage:
                        if (buf.Length < offs + 4)
                            return false;
                        BasicPassageSection p = new BasicPassageSection(BitConverter.ToInt32(buf, offs));
                        offs += 4;
                        if (!ReadBytesOfBasicPassageSection(p, buf, ref offs))
                            return false;
                        Sections.Add(p);
                        break;
                    default:
                        IndependentQSection ind = ReadBytesOfIndependentQSection(buf, ref offs);
                        if (ind == null)
                            return false;
                        Sections.Add(ind);
                        break;
                }
                
                --n;
            }
            return true;
        }

        private void AppendBytesOf(IndependentQSection section, List<byte[]> byteList)
        {
            byteList.Add(BitConverter.GetBytes((int)SectionTypeID.DefaultIndependentQuestions));
            byteList.Add(BitConverter.GetBytes(section.ID));
            Utils.AppendBytesOfString(section.Requirements, byteList);
            byteList.Add(BitConverter.GetBytes(section.Questions.Count));
            foreach (Question q in section.Questions)
                AppendBytesOf(q, byteList);
        }

        private void AppendBytesOf(BasicPassageSection section, List<byte[]> byteList)
        {
            byteList.Add(BitConverter.GetBytes((int)section.GetSectionTypeID()));
            byteList.Add(BitConverter.GetBytes(section.ID));
            Utils.AppendBytesOfString(section.Requirements, byteList);
            Utils.AppendBytesOfString(section.Passage, byteList);
            byteList.Add(BitConverter.GetBytes(section.Questions.Count));
            foreach (Question q in section.Questions)
                AppendBytesOf(q, byteList);
        }

        public List<byte[]> ToByte()
        {
            List<byte[]> l = new List<byte[]>();
            l.Add(BitConverter.GetBytes(ID));
            l.Add(BitConverter.GetBytes(Sections.Count));
            foreach (QSheetSection section in Sections)
            {
                BasicPassageSection p_section = section as BasicPassageSection;
                if(p_section != null)
                {
                    AppendBytesOf(p_section, l);
                    continue;
                }
                IndependentQSection i_section = section as IndependentQSection;
                if (i_section != null)
                {
                    AppendBytesOf(i_section, l);
                    continue;
                }
            }
            return l;
        }

        public bool ReadByte(byte[] buf, ref int offs)
        {
            if (buf == null)
                return true;
            int offs0 = offs;
            //
            if (buf.Length - offs < 4)
                return true;
            ID = BitConverter.ToInt32(buf, offs);
            offs += 4;

            if (!ReadBytesOfSections(buf, ref offs))
                return true;
            
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

        private void ShuffleSection(Random rand)
        {
            List<QSheetSection> sections = Sections;
            Sections = new List<QSheetSection>();

            while (sections.Count > 1)
            {
                int idx = rand.Next() % sections.Count;
                QSheetSection s = sections.ElementAt(idx);
                Sections.Add(s);
                sections.Remove(s);
            }
            if (sections.Count == 1)
                Sections.Add(sections.ElementAt(0));
        }

        public QuestSheet RandomizeDeepCopy(Random rand, bool sectionShuffling)
        {
            QuestSheet sheet = new QuestSheet();
            sheet.ID = ID;
            sheet.Subject = Subject;

            sheet.Sections = RandomizeDeepCopy_KeepSectionsOrder(rand);

            if (sectionShuffling)
                ShuffleSection(rand);

            return sheet;
        }

        private List<QSheetSection> RandomizeDeepCopy_KeepSectionsOrder(Random rand)
        {
            List<QSheetSection> sections = new List<QSheetSection>();
            foreach (QSheetSection section in Sections)
            {
                PassageWithBlanks p_blank = section as PassageWithBlanks;
                if (p_blank != null)
                {
                    BasicPassageSection p2 = p_blank.Clone() as PassageWithBlanks;
                    p2.Randomize_KeepQuestionOrder(rand);
                    sections.Add(p2);
                    continue;
                }
                BasicPassageSection p = section as BasicPassageSection;
                if (p != null)
                {
                    BasicPassageSection p2 = p.Clone() as BasicPassageSection;
                    p2.Randomize_KeepQuestionOrder(rand);
                    sections.Add(p2);
                    continue;
                }
                MTFIndependentQSection m = section as MTFIndependentQSection;
                if (m != null)
                {
                    MTFIndependentQSection m2 = m.Clone() as MTFIndependentQSection;
                    m2.Randomize(rand);
                    sections.Add(m2);
                    continue;
                }
                IndependentQSection i = section as IndependentQSection;
                if (i != null)
                {
                    IndependentQSection i2 = i.Clone() as IndependentQSection;
                    i2.Randomize(rand);
                    sections.Add(i2);
                    continue;
                }
            }
            return sections;
        }

        //only Server0 uses this.
        public void DBSelectNondeletedQuestions(int subject, int singleAnswerCount, int MTF_count)
        {
            Subject = subject;
            Sections.Clear();
            MySqlConnection conn = DBConnect.OpenNewConnection();
            if (conn == null)
            {
                System.Windows.MessageBox.Show(Txt.s._((int)TxI.DB_NOK));
                return;
            }

            Random random = new Random();
            List<Question> questions =
                DBSelectQuestions(conn, "deleted=0 AND subj_id=" + Subject +
                    " AND secid=" + (int)SectionTypeID.DefaultIndependentQuestions,
                    singleAnswerCount, random);
            if (questions == null)
                return;
            List<Question> moreQuestions =
                DBSelectQuestions(conn, "deleted=0 AND subj_id=" + Subject +
                    " AND secid=" + (int)SectionTypeID.MTFIndependentQuestions,
                    MTF_count, random);

            if (moreQuestions == null)
                return;

            questions.AddRange(moreQuestions);

            DBSelectSections(conn, questions);

            DBConnect.Close(ref conn);
        }

        //only Server0 uses this.
        public void DBSelectNondeletedQuestions(int subject)
        {
            Subject = subject;
            Sections.Clear();
            MySqlConnection conn = DBConnect.OpenNewConnection();
            if (conn == null)
            {
                System.Windows.MessageBox.Show(Txt.s._((int)TxI.DB_NOK));
                return;
            }
            List<Question> allQuestions = DBSelectQuestions(conn, "deleted=0 AND subj_id=" + Subject);
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
                            Safe_AddTempSection(tempSections, ind_section);
                            break;
                        case SectionTypeID.MTFIndependentQuestions:
                            MTFIndependentQSection MTF_section = new MTFIndependentQSection(reader.GetInt32(0));
                            MTF_section.Requirements = reader.GetString(2);
                            Sections.Add(MTF_section);
                            Safe_AddTempSection(tempSections, MTF_section);
                            break;
                        case SectionTypeID.PassageWithBlanks:
                            PassageWithBlanks p_blanks = new PassageWithBlanks(reader.GetInt32(0));
                            p_blanks.Requirements = reader.GetString(2);
                            p_blanks.Passage = reader.GetString(3);
                            Sections.Add(p_blanks);
                            Safe_AddTempSection(tempSections, p_blanks);
                            break;
                        case SectionTypeID.BasicPassage:
                            BasicPassageSection basic_passage = new BasicPassageSection(reader.GetInt32(0));
                            basic_passage.Requirements = reader.GetString(2);
                            basic_passage.Passage = reader.GetString(3);
                            Sections.Add(basic_passage);
                            Safe_AddTempSection(tempSections, basic_passage);
                            break;
                        default:
                            System.Windows.MessageBox.Show("Section type ID is not handled: " + sectionTypeID);
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

            if(SavedQuestOrderInDB != null)
            {
                foreach (QSheetSection section in Sections)
                    section.Questions.Sort(SavedQuestOrderInDB);

                SavedSectionOrderInDB = new QIdxComparer<QSheetSection>();
                SavedSectionOrderInDB.vIdx = SavedQuestOrderInDB.vIdx;
                Sections.Sort(SavedSectionOrderInDB);
            }
        }

        private void Safe_AddTempSection(Dictionary<int, QSheetSection> tempSections, QSheetSection section)
        {
            if (tempSections[section.ID] == null)
                tempSections[section.ID] = section;
            else
                System.Windows.MessageBox.Show("Warining: DBSelectSections has duplicated section ID: " + section.ID);
        }

        private Question DBReader_CreateQuestion(MySqlDataReader reader)
        {
            Question q = new Question();
            q.uId = (int)reader.GetUInt32(0);
            if (reader.IsDBNull(1))
                q.SectionID = -1;
            else
                q.SectionID = reader.GetInt32(1);
            int questType = reader.GetInt32(2);
            if(Enum.IsDefined(typeof(AnswerType), questType))
                q.QuestionType = (AnswerType)questType;
            q.Stem = reader.GetString(3);
            q.Options = new string[OptionSelectAnswer.OPTION_COUNT];
            for (int j = 0; j < OptionSelectAnswer.OPTION_COUNT; ++j)
                q.Options[j] = reader.GetString(4 + j);
            string x = reader.GetString(8);
            q.Answer = new byte[OptionSelectAnswer.OPTION_COUNT];
            for (int j = 0; j < OptionSelectAnswer.OPTION_COUNT; ++j)
                q.Answer[j] = (x[j] == QuestionAnswer.TRUE) ? QuestionAnswer.TRUE : QuestionAnswer.FALSE;
            if(q.QuestionType == AnswerType.Undefined)
            {
                System.Windows.MessageBox.Show("From DB, question type error at stem: " + q.Stem);
            }
            return q;
        }

        private List<Question> DBSelectQuestions(MySqlConnection conn, string condition)
        {
            string query = DBConnect.mkQrySelect("sqz_question",
                "id,secid,quest_type,stem,ans0,ans1,ans2,ans3,akey", condition);
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

        private List<uint> DBSelectQuestionIds(MySqlConnection conn, string condition)
        {
            string eMsg;
            string query = DBConnect.mkQrySelect("sqz_question",
                "id", condition);
            List<uint> questionIds = new List<uint>();
            MySqlDataReader reader = DBConnect.exeQrySelect(conn, query, out eMsg);
            if (reader != null)
            {
                while (reader.Read())
                    questionIds.Add(reader.GetUInt32(0));
                reader.Close();
            }
            else
                System.Windows.MessageBox.Show(eMsg.ToString());
            return questionIds;
        }

        private List<uint> RandomSubset(List<uint> set, int count, Random random)
        {
            if(count > set.Count)
            {
                System.Windows.MessageBox.Show("RandomSubset " + count + " > " + set.Count);
                return null;
            }
            if (count == set.Count)
                return set;

            List<uint> newList = new List<uint>();
            while (0 < count)
            {
                int idx = random.Next() % set.Count;
                newList.Add(set.ElementAt(idx));
                set.RemoveAt(idx);
                count--;
            }

            return newList;
        }

        private List<Question> DBSelectQuestions(MySqlConnection conn, string condition, int count, Random random)
        {
            List<uint> questionIds = RandomSubset(DBSelectQuestionIds(conn, condition), count, random);

            if (questionIds == null)
                return null;

            questionIds.Sort();

            StringBuilder id_condition = new StringBuilder();
            foreach (uint id in questionIds)
                id_condition.Append(id.ToString() + ',');
            id_condition.Remove(id_condition.Length - 1, 1);

            condition = condition + " AND id IN (" + id_condition + ")";

            string eMsg;
            string query = DBConnect.mkQrySelect("sqz_question",
                "id,secid,quest_type,stem,ans0,ans1,ans2,ans3,akey", condition);
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
            MySqlConnection conn = DBConnect.OpenNewConnection();
            if (conn == null)
                return;

            if (!QSheetSection.GetMaxID_inDB(conn))
                return;

            StringBuilder questionInsVals = new StringBuilder();
            StringBuilder sectionInsVals = new StringBuilder();
            string eMsg;
            foreach (QSheetSection section in Sections)
            {
                //TODO: Updating doesn't know insert in sectionInsVals.
                section.AccquireMaxId(conn);

                if (section.ID < 0)
                {
                    section.AccquireGlobalMaxId();
                    sectionInsVals.Append("(" + section.ID + "," + section.GetSectionTypeID() +
                        ",'" + DBConnect.SafeSQL_Text(section.Requirements) + "',");
                    BasicPassageSection passageSection = section as BasicPassageSection;
                    if (passageSection != null)
                        sectionInsVals.Append("'" + DBConnect.SafeSQL_Text(passageSection.Passage) + "'),");
                    else
                        sectionInsVals.Append("NULL),"); //TODO: manual config later
                }
                else
                {
                    DBConnect.Update(conn, "sqz_section",
                        "req='" + DBConnect.SafeSQL_Text(section.Requirements) + "'", "id=" + section.ID, out eMsg);
                }

                foreach (Question q in section.Questions)
                    AppendQuestionInsertQuery(q, questionInsVals);
            }

            if (sectionInsVals.Length > 0)
            {
                sectionInsVals.Remove(sectionInsVals.Length - 1, 1);//remove the last comma
                if (DBConnect.Ins(conn, "sqz_section", "id,s_type,req,psg",
                    sectionInsVals.ToString(), out eMsg) < 0)
                    System.Windows.MessageBox.Show("Error inserting passages:\n" + eMsg);
            }
            if (questionInsVals.Length > 0)
            {
                DB_InsertSubject_ifNExists(conn, Subject);
                questionInsVals.Remove(questionInsVals.Length - 1, 1);//remove the last comma
                if (DBConnect.Ins(conn, "sqz_question", "subj_id,secid,deleted,quest_type,stem,ans0,ans1,ans2,ans3,akey",
                questionInsVals.ToString(), out eMsg) < 0)
                    System.Windows.MessageBox.Show("Error inserting questions:\n" + eMsg);
            }

            DBConnect.Close(ref conn);
        }

        public static void DB_InsertSubject_ifNExists(MySqlConnection conn, int subject)
        {
            string emsg;
            if (DBConnect.NExist(conn, "sqz_subject", "id=" + subject, out emsg))
                DBConnect.Ins(conn, "sqz_subject", "id", "(" + subject + ")", out emsg);
        }

        private void AppendQuestionInsertQuery(Question q, StringBuilder query)
        {
            query.Append("(" + Subject + ",");
            if (q.SectionID < 0)
                query.Append("NULL,0,");
            else
                query.Append(q.SectionID + ",0,");
            query.Append((int)q.QuestionType + ",'");
            query.Append(DBConnect.SafeSQL_Text(q.Stem) + "','");
            for (int i = 0; i < OptionSelectAnswer.OPTION_COUNT; ++i)
                query.Append(DBConnect.SafeSQL_Text(q.Options[i]) + "','");
            for (int i = 0; i < OptionSelectAnswer.OPTION_COUNT; ++i)
                query.Append((char)q.Answer[i]);
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
            SavedQuestOrderInDB = new QIdxComparer<Question>();
            while (reader.Read())
            {
                uint qid = reader.GetUInt32(0);
                questionIDs.Add(qid);
                options_sorts.Add(reader.GetString(1));
                SavedQuestOrderInDB.Add((int)qid, reader.GetInt32(2));
            }
            reader.Close();
            if (questionIDs.Count == 0)
                return true;
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
                var sorted_answers = new string[OptionSelectAnswer.OPTION_COUNT];
                var sorted_keys = new byte[4];
                for (int j = 0; j < 4; ++j)
                {
                    sorted_answers[j] = q.Options[options_sorts[i][j] - '0'];
                    sorted_keys[j] = q.Answer[options_sorts[i][j] - '0'];
                }
                q.Options = sorted_answers;
                q.Answer = sorted_keys;
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
            if (uid < -1 &&
                System.Windows.MessageBox.Show("Cannot get QuestSheet.GetMaxID_inDB. Choose Yes to continue and get risky!",
                    "Warning!", System.Windows.MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.No)
                    return false;

            globalMaxID = uid;

            return true;
        }
    }

    public class QIdxComparer<T> : Comparer<T>
    {
        public SortedList<int, int> vIdx;

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
                return CompareSections(x, y);
            Question qy = y as Question;
            if (qy == null)
                return CompareSections(x, y);
            return CompareQuestions(qx, qy);
        }

        private int CompareQuestions(Question qx, Question qy)
        {
            if (vIdx[qx.uId] < vIdx[qy.uId])
                return -1;
            else if (vIdx[qx.uId] == vIdx[qy.uId])
                return 0;
            return 1;
        }

        private int CompareSections(T x, T y)
        {
            QSheetSection sx = x as QSheetSection;
            if (sx == null)
                return 0;
            QSheetSection sy = y as QSheetSection;
            if (sy == null)
                return 0;
            if (sx.Questions == null || sy.Questions == null ||
                sx.Questions.Count == 0 || sy.Questions.Count == 0)
                return 0;
            return CompareQuestions(sx.Questions.First(), sy.Questions.First());
        }
    }

    public enum QuestDiff
    {
        Easy,
        Diff,
        Both
    }
}
