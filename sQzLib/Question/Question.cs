using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Text;

namespace sQzLib
{
    public class Question
    {
        public int uId;
        public int SectionID;
        public AnswerType QuestionType;
        public string Stem;
        public string[] Options;
        public byte[] Answer;
        public int[] OptionShuffle;

        public Question() {
            QuestionType = AnswerType.Undefined;
            Options = null;
            SectionID = -1;
            OptionShuffle = new int[OptionSelectAnswer.OPTION_COUNT];
            for (int i = 0; i < OptionSelectAnswer.OPTION_COUNT; ++i)
                OptionShuffle[i] = i;
        }

        TokenType classify(string s) {
            return TokenType.Both;
        }

        public IEnumerable<string> ToListOfStrings()
        {
            LinkedList<string> s = new LinkedList<string>();
            s.AddLast(Stem);
            foreach (string i in Options)
                s.AddLast(i);
            return s;
        }

        //public bool Ans(int idx, out string ans)
        //{
        //    if (0 < idx && idx < N_ANS)
        //    {
        //        ans = vAns[idx];
        //        return vKeys[idx];
        //    }
        //    else
        //    {
        //        ans = string.Empty;
        //        return false;
        //    }
        //}

        public static void DBDelete(string ids) {
            MySqlConnection conn = DBConnect.OpenNewConnection();
            if (conn == null)
                return;
            string eMsg;
            DBConnect.Update(conn, "sqz_question", "deleted=1", ids, out eMsg);
            DBConnect.Close(ref conn);
        }

        //That RandomizeDeepCopy having its own DeepCopy codes created a bug by not setting question type
        public Question DeepCopy()
        {
            Question newQuestion = new Question();
            newQuestion.uId = uId;
            newQuestion.SectionID = SectionID;
            newQuestion.QuestionType = QuestionType;
            newQuestion.Stem = Stem;
            newQuestion.Options = new string[OptionSelectAnswer.OPTION_COUNT];
            newQuestion.OptionShuffle = new int[OptionSelectAnswer.OPTION_COUNT];
            newQuestion.Answer = new byte[OptionSelectAnswer.OPTION_COUNT];
            for (int i = 0; i < OptionSelectAnswer.OPTION_COUNT; ++i)
            {
                newQuestion.Options[i] = Options[i];
                newQuestion.OptionShuffle[i] = OptionShuffle[i];
                newQuestion.Answer[i] = Answer[i];
            }
                
            return newQuestion;
        }

        //public void Randomize(Random rand)
        //{
        //    string[] newOptions = new string[OptionSelectAnswer.OPTION_COUNT];
        //    int[] optionShuffle = new int[OptionSelectAnswer.OPTION_COUNT];
        //    List<int> l = new List<int>();
        //    int n = OptionSelectAnswer.OPTION_COUNT;
        //    for (int i = 0; i < n; ++i)
        //        l.Add(i);
        //    while (0 < n)
        //    {
        //        int lidx = rand.Next() % n;
        //        int idx = l[lidx];
        //        l.RemoveAt(lidx);
        //        --n;
        //        newOptions[n] = Options[idx];
        //        optionShuffle[n] = idx;
        //    }
        //    Options = newOptions;
        //    Answer = newAnswerKey;
        //    OptionShuffle = optionShuffle;
        //}

        //That RandomizeDeepCopy having its own DeepCopy codes created a bug by not setting question type
        public Question RandomizeDeepCopy(Random rand)
        {
            Question newQuestion = new Question();
            newQuestion.uId = uId;
            newQuestion.SectionID = SectionID;
            newQuestion.QuestionType = QuestionType;
            newQuestion.Stem = Stem;
            //randomize
            newQuestion.Options = new string[OptionSelectAnswer.OPTION_COUNT];
            newQuestion.Answer = new byte[OptionSelectAnswer.OPTION_COUNT];
            List<int> idxPool = new List<int>();
            for (int i = 0; i < OptionSelectAnswer.OPTION_COUNT; ++i)
                idxPool.Add(i);
            int poolCount = OptionSelectAnswer.OPTION_COUNT;
            while (0 < poolCount)
            {
                int pickedLocation = rand.Next() % poolCount;
                int pickedIdx = idxPool[pickedLocation];
                idxPool.RemoveAt(pickedLocation);
                --poolCount;
                newQuestion.OptionShuffle[poolCount] = pickedIdx;
                newQuestion.Options[poolCount] = Options[pickedIdx];
                newQuestion.Answer[poolCount] = Answer[pickedIdx];
            }

            return newQuestion;
        }
    }
}
