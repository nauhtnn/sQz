using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Text;

namespace sQzLib
{
    public class Question
    {
        public const int N_ANS = 4;
        public const char C0 = '0';
        public const char C1 = '1';
        public int uId;
        public int PassageID;
        public string Stem;
        public string[] vAns;
        public bool[] vKeys;
        public int[] vAnsSort;

        public Question() {
            vAns = null;
            PassageID = -1;
            vAnsSort = new int[N_ANS];
            for (int i = 0; i < N_ANS; ++i)
                vAnsSort[i] = i;
        }

        TokenType classify(string s) {
            return TokenType.Both;
        }

        public IEnumerable<string> ToListOfStrings()
        {
            LinkedList<string> s = new LinkedList<string>();
            s.AddLast(Stem);
            foreach (string i in vAns)
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
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return;
            string eMsg;
            DBConnect.Update(conn, "sqz_question", "deleted=1", ids, out eMsg);
        }

        public Question DeepCopy()
        {
            Question q = new Question();
            q.uId = uId;
            q.Stem = Stem;
            q.PassageID = PassageID;
            q.vAns = new string[N_ANS];
            for (int i = 0; i < N_ANS; ++i)
                q.vAns[i] = vAns[i];
            q.vKeys = new bool[N_ANS];
            for (int i = 0; i < N_ANS; ++i)
                q.vKeys[i] = vKeys[i];
            q.vAnsSort = new int[N_ANS];
            for (int i = 0; i < N_ANS; ++i)
                q.vAnsSort[i] = vAnsSort[i];
            return q;
        }

        public void Randomize(Random rand)
        {
            string[] anss = new string[N_ANS];
            bool[] keys = new bool[N_ANS];
            int[] asort = new int[N_ANS];
            List<int> l = new List<int>();
            int n = N_ANS;
            for (int i = 0; i < n; ++i)
                l.Add(i);
            while (0 < n)
            {
                int lidx = rand.Next() % n;
                int idx = l[lidx];
                l.RemoveAt(lidx);
                --n;
                anss[n] = vAns[idx];
                keys[n] = vKeys[idx];
                asort[n] = idx;
            }
            vAns = anss;
            vKeys = keys;
            vAnsSort = asort;
        }

        public Question RandomizeDeepCopy(Random rand)
        {
            Question q = new Question();
            q.uId = uId;
            q.Stem = Stem;
            q.PassageID = PassageID;
            //randomize
            q.vAns = new string[N_ANS];
            q.vKeys = new bool[N_ANS];
            List<int> l = new List<int>();
            for (int i = 0; i < N_ANS; ++i)
                l.Add(i);
            int n = N_ANS;
            while (0 < n)
            {
                int lidx = rand.Next() % n;
                int idx = l[lidx];
                l.RemoveAt(lidx);
                --n;
                q.vAns[n] = vAns[idx];
                q.vKeys[n] = vKeys[idx];
                q.vAnsSort[n] = idx;
            }
            return q;
        }
    }

    public enum QuestType
    {
        Single = 1,
        Multiple = 2,
        Insertion = 4,
        Selection = 8,
        Matching = 16
    }

    public enum ContentType
    {
        Raw = 1,
        Image = 2,
        Audio = 4,
        Video = 8
    }

    public enum TokenType
    {
        Requirement = 0,
        Stem = 1,
        Ans = 2,
        Both = 3
    }

    public enum IUx
    {
        _1 = 0, _2, _3, _4, _5, _6, _7, _8, _9, _10, _11, _12, _13, _14, _15, _0
    }
}
