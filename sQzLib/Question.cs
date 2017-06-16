using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Text;

/*
CREATE TABLE IF NOT EXISTS `q0` (`id` INT(4) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
`body` VARCHAR(9192) CHARACTER SET `utf8`, `ansKeys` CHAR(4) CHARACTER SET `ascii`);
CREATE TABLE IF NOT EXISTS `q1` (`id` INT(4) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
`body` VARCHAR(9192) CHARACTER SET `utf8`, `ansKeys` CHAR(4) CHARACTER SET `ascii`);
CREATE TABLE IF NOT EXISTS `q2` (`id` INT(4) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
`body` VARCHAR(9192) CHARACTER SET `utf8`, `ansKeys` CHAR(4) CHARACTER SET `ascii`);
CREATE TABLE IF NOT EXISTS `q3` (`id` INT(4) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
`body` VARCHAR(9192) CHARACTER SET `utf8`, `ansKeys` CHAR(4) CHARACTER SET `ascii`);
CREATE TABLE IF NOT EXISTS `q4` (`id` INT(4) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
`body` VARCHAR(9192) CHARACTER SET `utf8`, `ansKeys` CHAR(4) CHARACTER SET `ascii`);
CREATE TABLE IF NOT EXISTS `q5` (`id` INT(4) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
`body` VARCHAR(9192) CHARACTER SET `utf8`, `ansKeys` CHAR(4) CHARACTER SET `ascii`);
CREATE TABLE IF NOT EXISTS `q6` (`id` INT(4) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
`body` VARCHAR(9192) CHARACTER SET `utf8`, `ansKeys` CHAR(4) CHARACTER SET `ascii`);
CREATE TABLE IF NOT EXISTS `q7` (`id` INT(4) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
`body` VARCHAR(9192) CHARACTER SET `utf8`, `ansKeys` CHAR(4) CHARACTER SET `ascii`);
CREATE TABLE IF NOT EXISTS `q8` (`id` INT(4) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
`body` VARCHAR(9192) CHARACTER SET `utf8`, `ansKeys` CHAR(4) CHARACTER SET `ascii`);
CREATE TABLE IF NOT EXISTS `q9` (`id` INT(4) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
`body` VARCHAR(9192) CHARACTER SET `utf8`, `ansKeys` CHAR(4) CHARACTER SET `ascii`);
CREATE TABLE IF NOT EXISTS `q10` (`id` INT(4) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
`body` VARCHAR(9192) CHARACTER SET `utf8`, `ansKeys` CHAR(4) CHARACTER SET `ascii`);
CREATE TABLE IF NOT EXISTS `q11` (`id` INT(4) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
`body` VARCHAR(9192) CHARACTER SET `utf8`, `ansKeys` CHAR(4) CHARACTER SET `ascii`);
CREATE TABLE IF NOT EXISTS `q12` (`id` INT(4) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
`body` VARCHAR(9192) CHARACTER SET `utf8`, `ansKeys` CHAR(4) CHARACTER SET `ascii`);
CREATE TABLE IF NOT EXISTS `q13` (`id` INT(4) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
`body` VARCHAR(9192) CHARACTER SET `utf8`, `ansKeys` CHAR(4) CHARACTER SET `ascii`);
CREATE TABLE IF NOT EXISTS `q14` (`id` INT(4) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
`body` VARCHAR(9192) CHARACTER SET `utf8`, `ansKeys` CHAR(4) CHARACTER SET `ascii`);
*/

namespace sQzLib
{
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

    public enum TokenType {
        Requirement = 0,
        Stmt = 1,
        Ans = 2,
        Both = 3
    }

    public enum IUx
    {
        _1 = 0, _2, _3, _4, _5, _6, _7, _8, _9, _10, _11, _12, _13, _14, _15, _0
    }

    public class Question
    {
        public int uId;
        public string mStmt; //statement
        public int nAns;
        public IUx mIU;
        public string[] vAns;
        public bool[] vKeys;
        public int[] vAnsSort;
        bool bChoiceSort;
        public QuestType qType;
        ContentType cType;
        static string[] svToken;
        static int siToken;
        //static Settings sSett;
        //static string[] qPattern = { "[a-zA-Z]+[0-9]+", "[0-9]+\\." };
        static string[] qPattern = { "[0-9]+\\." };
        int qSubs;
        static string[] aPattern = { "\\([a-zA-Z]\\)", "[a-zA-Z]\\." };
        List<int> aSubs;

        public Question() {
            nAns = 0;
            vAns = null;
            bChoiceSort = true;
            qType = QuestType.Single;
            cType = ContentType.Raw;
            qSubs = -1;
            aSubs = new List<int>();
            vAnsSort = new int[4];//hardcode, todo
            for (int i = 0; i < 4; ++i)
                vAnsSort[i] = i;
        }

        TokenType classify(string s) {
            if (s.StartsWith("!"))
                return TokenType.Requirement;
            int x = s.IndexOf(' '), y = s.IndexOf('\t');
            if (-1 < x && -1 < y)
                x = System.Math.Min(x, y);
            else
                x = System.Math.Max(x, y);
            if (-1 == x)
                return TokenType.Ans;
            else
                s = s.Substring(0, x);//first word
            for (int i = 0; i < qPattern.Length; ++i)
            {
                System.Text.RegularExpressions.Match m =
                    System.Text.RegularExpressions.Regex.Match(s, qPattern[i]);
                if (m.Success && m.Index == 0 && m.Length == s.Length)
                {
                    qSubs = x;
                    return TokenType.Stmt;
                }
            }
            for (int i = 0; i < aPattern.Length; ++i)
            {
                System.Text.RegularExpressions.Match m =
                    System.Text.RegularExpressions.Regex.Match(s, aPattern[i]);
                if (m.Success && m.Index == 0 && m.Length == s.Length)
                {
                    aSubs.Add(System.Math.Min(x, m.Index + m.Length));
                    return TokenType.Ans;
                }
            }
            return TokenType.Both;
        }

        void readStmt()
        {
            if (svToken.Length <= siToken)
                return;
            TokenType t = classify(svToken[siToken]);
            if (t == TokenType.Stmt || t == TokenType.Both)
            {
                if (-1 < qSubs)
                    mStmt = Utils.CleanFront(svToken[siToken++], qSubs);
                else
                    mStmt = svToken[siToken++];
            }

            //else: error
            //TODO: detect qType
        }

        void searchAns()
        {
            List<int> v = new List<int>(3 * vAns.Length);
            for (int k = 0; k < nAns; ++k)
                for (int i = 0; i < aPattern.Length; ++i)
                {
                    System.Text.RegularExpressions.MatchCollection m =
                        System.Text.RegularExpressions.Regex.Matches(vAns[k], aPattern[i]);
                    for (int j = 0; j < m.Count; ++j)
                    {
                        v.Add(m[j].Index);
                        v.Add(k);
                        v.Add(m[j].Index + m[j].Length);
                    }
                }
            if (v.Count / 3 == vAns.Length - aSubs.Count)
            {
                List<string> vA = new List<string>(vAns.Length);
                bool add0 = true;
                while (0 < v.Count)
                {
                    if (add0)
                    {
                        vA.Add(vAns[v[1]].Substring(0, v[0]));
                        add0 = false;
                    }
                    if (4 < v.Count && v[1] == v[4]) //middle of line
                        vA.Add(Utils.CleanFrontBack(vAns[v[1]], v[2], v[3] - 1));
                    else //end of line
                    {
                        vA.Add(Utils.CleanFront(vAns[v[1]], v[2]));
                        add0 = true;
                    }
                    v.RemoveRange(0, 3);
                }
                vAns = vA.ToArray();
                nAns = vAns.Length;
            }
        }

        void readAns()
        {
            if (svToken.Length <= siToken)
                return;
            vAns = new string[nAns];
            vKeys = new bool[nAns];
            int np = aSubs.Count;
            TokenType t = classify(svToken[siToken]);
            int n = 0;
            while (n < nAns && (t == TokenType.Ans || t == TokenType.Both)) {
                if (np < aSubs.Count) {
                    vAns[n++] = Utils.CleanFront(svToken[siToken++], aSubs[np]);
                    np = aSubs.Count;
                }
                else
                    vAns[n++] = svToken[siToken++];
                if (svToken.Length <= siToken)
                    break;
                t = classify(svToken[siToken]);
            }
            if (n < nAns)
            {
                nAns = n;
                searchAns();
            }
        }

        public static void StartRead(string[] v, Settings s) {
            svToken = v;
            siToken = 0;
        }

        public bool Read()
        {
            if (svToken.Length <= siToken)
                return false;
            int s = 0;
            System.Text.RegularExpressions.Match m =
                    System.Text.RegularExpressions.Regex.Match(svToken[siToken], "\\\\[0-9]+ ");
            if (m.Success)
            {
                int nc = int.Parse(svToken[siToken].Substring(m.Index + 1, m.Length - 1));
                if (1 < nc)
                    nAns = nc;
                else
                    nAns = 4;// sSett.nAns;
                s = m.Index + m.Length;
            }
            else
                nAns = 4;// sSett.nAns;
            int x = svToken[siToken].IndexOf("\\c");
            if (-1 < x)
            {
                bChoiceSort = false;
                x += 2;
                s = System.Math.Max(s, x);
            }
            x = svToken[siToken].IndexOf("\\C");
            if (-1 < x)
            {
                bChoiceSort = false;
                x += 2;
                s = System.Math.Max(s, x);
            }
            svToken[siToken] = svToken[siToken].Substring(s);
            readStmt();
            if (svToken.Length <= siToken)
                return false;
            if (qType == QuestType.Single || qType == QuestType.Multiple)
                readAns();
            mStmt = Utils.HTML(mStmt, ref cType);
            vKeys = new bool[nAns];
            for (int ki = 0; ki < nAns; ++ki)
                vKeys[ki] = false;
            int ci = 0, keyC = 0;
            for (; ci < nAns; ++ci)
            {
                vAns[ci] = Utils.HTML(vAns[ci], ref cType);
                if (vAns[ci][0] == '\\')
                {
                    vKeys[ci] = true;
                    ++keyC;
                    vAns[ci] = vAns[ci].Substring(1);
                }
            }
            if (ci < nAns)
            {
                for (int cj = ci; cj < nAns; ++cj)
                    vAns[cj] = null;
                nAns = ci;
            }
            if (1 < keyC && qType == QuestType.Single)
                qType = QuestType.Multiple;
            return true;
        }
        //public void write(System.IO.StreamWriter os, int idx, ref int col)
        //{
        //    if (cType == ContentType.Image)
        //    {
        //        if (col == 1)
        //            os.Write("<div class='cl'></div><div class='cl1'></div>");
        //        col = 2;//Program.MAX_COLUMN;
        //    }
        //    else
        //        ++col;
        //    if (cType == ContentType.Image)
        //        os.Write("<div class='cl2'");
        //    else
        //        os.Write("<div class='cl'");
        //    os.Write("><div class='qid'>" + idx +
        //        "</div><div class='q'><div class='stmt'>");
        //    if (qType == QuestType.Multiple)
        //        os.Write("<i>(Câu hỏi nhiều lựa chọn)</i><br>");
        //    os.Write(mStmt);
        //    os.Write("</div>\n");
        //    if (qType == QuestType.Single ||
        //        qType == QuestType.Multiple)
        //        wrtChoices(os, idx);
        //}
        //void wrtChoices(System.IO.StreamWriter os, int idx)
        //{
        //    string header = "<div name='" + idx + "'class='c'><span class='cid'>(", middle;
        //    if (qType == QuestType.Single)
        //        middle = ")</span><input type='radio'";
        //    else //Multiple
        //        middle = ")</span><input type='checkbox'";
        //    middle = middle + " name='-" + idx + "' value='";
        //    char j = 'A';
        //    List<string> choices = new List<string>(vAns);
        //    List<bool> keys = new List<bool>(vKeys);
        //    Random r = new Random();
        //    while (0 < choices.Count)
        //    {
        //        int i = 0;
        //        if (bChoiceSort && 1 < choices.Count)
        //            i = r.Next(choices.Count);
        //        os.Write(header + j + middle);
        //        if (keys[i])
        //        {
        //            char k = (char)(j - 'A' + '0');
        //            os.Write(k + "'>");
        //        }
        //        else
        //            os.Write("#'>");
        //        os.WriteLine(choices[i] + "</div>");
        //        choices.RemoveAt(i);
        //        keys.RemoveAt(i);
        //        ++j;
        //    }
        //    os.Write("</div></div>");
        //}

        public override string ToString()
        {
            StringBuilder s = new StringBuilder();
            s.Append('(' + (int)mIU +") " + mStmt + '\n');
            for (int i = 0; i < nAns; ++i)
                s.Append(vAns[i] + '\n');
            return s.ToString();
        }

        public static void Clear()
        {
            siToken = 0;//safe to be 0
        }

        public static void DBDelete(IUx eIU, string ids) {
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return;
            string eMsg;
            DBConnect.Update(conn, "sqz_question", "del=1", ids, out eMsg);
        }

        public Question DeepCopy()
        {
            Question q = new Question();
            q.uId = uId;
            q.mStmt = mStmt;
            q.nAns = nAns;
            q.mIU = mIU;
            q.vAns = new string[nAns];
            for (int i = 0; i < nAns; ++i)
                q.vAns[i] = vAns[i];
            q.vKeys = new bool[nAns];
            for (int i = 0; i < nAns; ++i)
                q.vKeys[i] = vKeys[i];
            //q.vAnsSort
            return q;
        }

        public void Randomize(Random rand)
        {
            string[] anss = new string[nAns];
            bool[] keys = new bool[nAns];
            int[] asort = new int[nAns];
            List<int> l = new List<int>();
            int n = nAns;
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
            q.mStmt = mStmt;
            q.nAns = nAns;
            q.mIU = mIU;
            //randomize
            q.vAns = new string[nAns];
            q.vKeys = new bool[nAns];
            List<int> l = new List<int>();
            int n = nAns;
            for (int i = 0; i < n; ++i)
                l.Add(i);
            while(0 < n)
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
}
