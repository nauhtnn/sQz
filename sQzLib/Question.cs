using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

/*
CREATE TABLE IF NOT EXISTS `quest1` (`idx` INT(4) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
`body` VARCHAR(9192) CHARACTER SET `utf32`, `ansKeys` CHAR(4) CHARACTER SET `ascii`);
CREATE TABLE IF NOT EXISTS `quest2` (`idx` INT(4) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
`body` VARCHAR(9192) CHARACTER SET `utf32`, `ansKeys` CHAR(4) CHARACTER SET `ascii`);
CREATE TABLE IF NOT EXISTS `quest3` (`idx` INT(4) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
`body` VARCHAR(9192) CHARACTER SET `utf32`, `ansKeys` CHAR(4) CHARACTER SET `ascii`);
CREATE TABLE IF NOT EXISTS `quest4` (`idx` INT(4) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
`body` VARCHAR(9192) CHARACTER SET `utf32`, `ansKeys` CHAR(4) CHARACTER SET `ascii`);
CREATE TABLE IF NOT EXISTS `quest5` (`idx` INT(4) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
`body` VARCHAR(9192) CHARACTER SET `utf32`, `ansKeys` CHAR(4) CHARACTER SET `ascii`);
CREATE TABLE IF NOT EXISTS `quest6` (`idx` INT(4) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
`body` VARCHAR(9192) CHARACTER SET `utf32`, `ansKeys` CHAR(4) CHARACTER SET `ascii`);
CREATE TABLE IF NOT EXISTS `quest7` (`idx` INT(4) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
`body` VARCHAR(9192) CHARACTER SET `utf32`, `ansKeys` CHAR(4) CHARACTER SET `ascii`);
CREATE TABLE IF NOT EXISTS `quest8` (`idx` INT(4) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
`body` VARCHAR(9192) CHARACTER SET `utf32`, `ansKeys` CHAR(4) CHARACTER SET `ascii`);
CREATE TABLE IF NOT EXISTS `quest9` (`idx` INT(4) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
`body` VARCHAR(9192) CHARACTER SET `utf32`, `ansKeys` CHAR(4) CHARACTER SET `ascii`);
CREATE TABLE IF NOT EXISTS `quest10` (`idx` INT(4) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
`body` VARCHAR(9192) CHARACTER SET `utf32`, `ansKeys` CHAR(4) CHARACTER SET `ascii`);
CREATE TABLE IF NOT EXISTS `quest11` (`idx` INT(4) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
`body` VARCHAR(9192) CHARACTER SET `utf32`, `ansKeys` CHAR(4) CHARACTER SET `ascii`);
CREATE TABLE IF NOT EXISTS `quest12` (`idx` INT(4) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
`body` VARCHAR(9192) CHARACTER SET `utf32`, `ansKeys` CHAR(4) CHARACTER SET `ascii`);
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

    public enum IUxx
    {
        IU00 = 0, IU01, IU02, IU03, IU04, IU05, IU06, IU07, IU08, IU09, IU10, IU11, IU12, IU13, IU14, IU15
    }

    public class Question
    {
		public static List<Question>[] svQuest = null;
        public string mStmt; //statement
        int nAns;
        public static IUxx sIU = IUxx.IU00;
        public IUxx mIU;
        public string[] vAns;
        public bool[] vKeys;
        bool bChoiceSort;
        QuestType qType;
        ContentType cType;
        static string[] svToken;
        static int siToken;
        //static Settings sSett;
        //static string[] qPattern = { "[a-zA-Z]+[0-9]+", "[0-9]+\\." };
		static string[] qPattern = { "[0-9]+\\." };
        int qSubs;
        static string[] aPattern = { "\\([a-zA-Z]\\)", "[a-zA-Z]\\." };
        List<int> aSubs;
        public static byte[] sbArrwKey = null;
        static bool sRdywKey = false;
        public static byte[] sbArr = null;
        static bool sRdy = false;

        public Question() {
            nAns = 0;
            vAns = null;
            mIU = sIU;
            bChoiceSort = true;
            qType = QuestType.Single;
            cType = ContentType.Raw;
            qSubs = -1;
            aSubs = new List<int>();
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
            for(int k = 0; k < nAns; ++k)
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
                    vAns[n++] =  Utils.CleanFront(svToken[siToken++], aSubs[np]);
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
            if (svQuest == null)
            {
                svQuest = new List<Question>[1];
                svQuest[0] = new List<Question>();
            }
            svQuest[0].Clear();
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
            mIU = sIU;
            return true;
        }
        public void write(System.IO.StreamWriter os, int idx, ref int col)
        {
            if (cType == ContentType.Image)
            {
                if (col == 1)
                    os.Write("<div class='cl'></div><div class='cl1'></div>");
                col = 2;//Program.MAX_COLUMN;
            }
            else
                ++col;
            if (cType == ContentType.Image)
                os.Write("<div class='cl2'");
            else
                os.Write("<div class='cl'");
            os.Write("><div class='qid'>" + idx +
                "</div><div class='q'><div class='stmt'>");
            if (qType == QuestType.Multiple)
                os.Write("<i>(Câu hỏi nhiều lựa chọn)</i><br>");
            os.Write(mStmt);
            os.Write("</div>\n");
            if (qType == QuestType.Single ||
                qType == QuestType.Multiple)
                wrtChoices(os, idx);
        }
        void wrtChoices(System.IO.StreamWriter os, int idx)
        {
            string header = "<div name='" + idx + "'class='c'><span class='cid'>(", middle;
            if (qType == QuestType.Single)
                middle = ")</span><input type='radio'";
            else //Multiple
                middle = ")</span><input type='checkbox'";
            middle = middle + " name='-" + idx + "' value='";
            char j = 'A';
            List<string> choices = new List<string>(vAns);
            List<bool> keys = new List<bool>(vKeys);
            Random r = new Random();
            while (0 < choices.Count)
            {
                int i = 0;
                if (bChoiceSort && 1 < choices.Count)
                    i = r.Next(choices.Count);
                os.Write(header + j + middle);
                if (keys[i])
                {
                    char k = (char)(j - 'A' + '0');
                    os.Write(k + "'>");
                }
                else
                    os.Write("#'>");
                os.WriteLine(choices[i] + "</div>");
                choices.RemoveAt(i);
                keys.RemoveAt(i);
                ++j;
            }
            os.Write("</div></div>");
        }

        public static void ReadTxt()
        {
            ReadTxt(Utils.ReadFile("qz1.txt"));
        }

        public static void ReadTxt(string buf)
        {
            if (buf == null)
                return;
            //else
            //"buf size = " + buf.Length + '\n';
            //sQzCS.Page pg = new sQzCS.Page();
            //StartRead(Utils.Split(buf, '\n'), pg.mSt);
            svQuest = new List<Question>[1];
            svQuest[0] = new List<Question>();
            StartRead(Utils.Split(buf, '\n'), null);
            Question q = new Question();
            while (q.Read())
            {
                svQuest[0].Add(q);
                q = new Question();
            }
            q = null;
            sRdy = sRdywKey = false;
            ToByteArr(true);
        }

        public override string ToString()
        {
            string s = mStmt + '\n';
            for (int i = 0; i < nAns; ++i)
                s += vAns[i] + '\n';
            return s;
        }

        public static void ToByteArr(bool woKey)
        {
            woKey = false;
            if (svQuest == null || svQuest.Length == 0)
                return;
            List<byte[]> l = new List<byte[]>();
            List<bool> lk = new List<bool>();
            l.Add(BitConverter.GetBytes(svQuest.Length));
            if(woKey)
                lk.Add(false);
            foreach (List<Question> ls in svQuest)
            {
                l.Add(BitConverter.GetBytes(ls.Count));
                foreach (Question q in ls)//todo: foreach
                {
                    //qType
                    l.Add(BitConverter.GetBytes((int)q.qType));
                    if (woKey)
                        lk.Add(false);
                    //stmt
                    byte[] b = System.Text.Encoding.UTF32.GetBytes(q.mStmt);
                    l.Add(BitConverter.GetBytes(b.Length));
                    l.Add(b);
                    if (woKey)
                    {
                        lk.Add(false);
                        lk.Add(false);
                    }
                    //ans
                    l.Add(BitConverter.GetBytes(q.nAns));
                    if (woKey)
                        lk.Add(false);
                    for (int j = 0; j < q.nAns; ++j)
                    {
                        //each ans
                        b = System.Text.Encoding.UTF32.GetBytes(q.vAns[j]);
                        l.Add(BitConverter.GetBytes(b.Length));
                        l.Add(b);
                        if (woKey)
                        {
                            lk.Add(false);
                            lk.Add(false);
                        }
                    }
                    //keys
                    for (int j = 0; j < q.nAns; ++j)
                    {
                        l.Add(BitConverter.GetBytes(q.vKeys[j]));
                        if (woKey)
                            lk.Add(true);
                    }
                }
            }
            //join
            int sz = 0;
            int szk = 0;
            for (int i = 0; i < l.Count; ++i)//foreach
            {
                sz += l[i].Length;
                if (woKey && lk[i])
                    szk += l[i].Length;
            }
            sbArrwKey = new byte[sz];
            if (woKey)
                sbArr = new byte[sz - szk];
            int offs = 0;
            for(int i = 0; i < l.Count; ++i)//foreach
            {
                Buffer.BlockCopy(l[i], 0, sbArrwKey, offs, l[i].Length);
                if(woKey && !lk[i])
                    Buffer.BlockCopy(l[i], 0, sbArr, offs, l[i].Length);
                offs += l[i].Length;
            }
            sRdywKey = true;
            if (woKey)
                sRdy = true;
        }

        public static void ReadByteArr(byte[] buf, ref int offs, int l, bool wKey)
        {
            wKey = true;
            if (buf == null)
                return;
            int offs0 = offs;
            if (l < 4)
                return;
            int nExSh = BitConverter.ToInt32(buf, offs);
            offs += 4;
            l -= 4;
            if (nExSh < 1)
                svQuest = null;
            else
                svQuest = new List<Question>[nExSh];
            int sz = 0;
            while (0 < nExSh)
            {
                if (l < 4)
                    return;
                int nQuest = BitConverter.ToInt32(buf, offs);
                offs += 4;
                l -= 4;
                --nExSh;
                svQuest[nExSh] = new List<Question>();
                while (0 < nQuest)
                {
                    Question q = new Question();
                    //qType
                    if (l < 4)
                        break;
                    q.qType = (QuestType)BitConverter.ToInt32(buf, offs);
                    l -= 4;
                    offs += 4;
                    //stmt
                    if (l < 4)
                        break;
                    sz = BitConverter.ToInt32(buf, offs);
                    l -= 4;
                    offs += 4;
                    if (l < sz)
                        break;
                    byte[] ar = new byte[sz];
                    Buffer.BlockCopy(buf, offs, ar, 0, sz);
                    l -= sz;
                    offs += sz;
                    q.mStmt = System.Text.Encoding.UTF32.GetString(ar);
                    //ans
                    if (l < 4)
                        break;
                    q.nAns = BitConverter.ToInt32(buf, offs);
                    l -= 4;
                    offs += 4;
                    q.vAns = new string[q.nAns];
                    bool brk = false;
                    for (int j = 0; j < q.nAns; ++j)
                    {
                        //each ans
                        if (l < 4)
                        {
                            brk = true;
                            break;
                        }
                        sz = BitConverter.ToInt32(buf, offs);
                        l -= 4;
                        offs += 4;
                        if (l < sz)
                        {
                            brk = true;
                            break;
                        }
                        ar = new byte[sz];
                        Buffer.BlockCopy(buf, offs, ar, 0, sz);
                        l -= sz;
                        offs += sz;
                        q.vAns[j] = System.Text.Encoding.UTF32.GetString(ar);
                    }
                    if (brk)
                        break;
                    //keys
                    if (wKey)
                    {
                        if (l < q.nAns)
                            break;
                        q.vKeys = new bool[q.nAns];
                        for (int j = 0; j < q.nAns; ++j)
                            q.vKeys[j] = BitConverter.ToBoolean(buf, offs++);
                        l -= q.nAns;
                    }
                    --nQuest;
                    svQuest[nExSh].Add(q);
                }
            }
            if(wKey && !Array.Equals(buf, sbArrwKey))
            {
                sz = offs - offs0;
                if (sz == buf.Length)
                    sbArrwKey = (byte[])buf.Clone();
                else
                {
                    sbArrwKey = new byte[sz];
                    Buffer.BlockCopy(buf, 0, sbArrwKey, 0, sz);
                }
                sRdy = false;
                sRdywKey = true;
            }
            if(!wKey && !Array.Equals(buf, sbArr))
            {
                sz = offs - offs0;
                if (sz == buf.Length)
                    sbArr = (byte[])buf.Clone();
                else
                {
                    sbArr = new byte[sz];
                    Buffer.BlockCopy(buf, 0, sbArrwKey, 0, sz);
                }
                sRdy = true;
                sRdywKey = false;
            }
        }

        public static void Clear()
        {
            foreach (List<Question> l in svQuest)
                l.Clear();
            siToken = 0;//safe to be 0
            sbArr = null;
            sbArrwKey = null;
            sRdy = sRdywKey = false;
        }
        public static void DBInsert()
        {
            if (sIU == IUxx.IU00)
                return;
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return;
            string[] attbs = new string[2];//hardcode
            attbs[0] = "body";
            attbs[1] = "ansKeys";
            foreach (Question q in svQuest[0])
            {
                string[] vals = new string[2];
                vals[0] = "'" + q.mStmt.Replace("'", "\\'") + '\n';
                for (int i = 0; i < q.nAns; ++i)
                    vals[0] += q.vAns[i].Replace("'", "\\'") + '\n';
                vals[0] += "'";
                vals[1] = "'";
                for (int i = 0; i < q.nAns; ++i)
                    if (q.vKeys[i])
                        vals[1] += '1';
                    else
                        vals[1] += '0';
                vals[1] += "'";
                string iu = sIU.ToString().Substring(2);//hardcode
                if (iu[0] == '0')
                    iu = iu.Substring(1);
                DBConnect.Ins(conn, "quest" + iu, attbs, vals);
            }
            DBConnect.Close(ref conn);
        }
        public static void DBSelect()
        {
            if (sIU == IUxx.IU00)
                return;
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return;
            string iu = sIU.ToString().Substring(2);//hardcode
            if (iu[0] == '0')
                iu = iu.Substring(1);
            string qry = DBConnect.mkQrySelect("quest" + iu, null, null, null, null);
            MySqlDataReader reader = DBConnect.exeQrySelect(conn, qry);
            svQuest = new List<Question>[1];
            svQuest[0] = new List<Question>();
            if (reader != null)
            {
                while (reader.Read())
                {
                    Question q = new Question();
                    string[] s = reader.GetString(1).Split('\n');//hardcode
                    q.mStmt = s[0];
                    q.nAns = 4;
                    q.vAns = new string[4];
                    for (int i = 0; i < 4; ++i)
                        q.vAns[i] = s[i + 1];
                    string x = reader.GetString(2);
                    q.vKeys = new bool[4];
                    for (int i = 0; i < 4; ++i)
                        q.vKeys[i] = (x[i] == '1');
                    q.mIU = sIU;
                    svQuest[0].Add(q);
                }
                reader.Close();
            }
            DBConnect.Close(ref conn);
            ToByteArr(true);
        }

        public static void DBSelect(IUxx iu, int n, ref List<Question> l)
        {
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return;
            string siu = iu.ToString().Substring(2);//hardcode
            if (siu[0] == '0')
                siu = siu.Substring(1);
            string qry = DBConnect.mkQrySelect("quest" + siu, null, null, null, null);
            MySqlDataReader reader = DBConnect.exeQrySelect(conn, qry);
            if (reader != null)
            {
                while (reader.Read() && 0 < n)
                {
                    Question q = new Question();
                    string[] s = reader.GetString(1).Split('\n');//hardcode
                    q.mStmt = "(" + siu + ')' + s[0];
                    q.nAns = 4;
                    q.vAns = new string[4];
                    for (int i = 0; i < 4; ++i)
                        q.vAns[i] = s[i + 1];
                    string x = reader.GetString(2);
                    q.vKeys = new bool[4];
                    for (int i = 0; i < 4; ++i)
                        q.vKeys[i] = (x[i] == '1');
                    q.mIU = sIU;
                    l.Add(q);
                    --n;
                }
                reader.Close();
            }
            DBConnect.Close(ref conn);
        }
    }
}
