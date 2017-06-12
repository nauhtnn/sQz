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
        public int mDiff;
        public List<Question> vQuest;
        public byte[] aQuest;

        static int sSeed = 1;

        public QuestSheet()
        {
            eLv = ExamLv.A;
            vQuest = new List<Question>();
            aQuest = null;
            uId = 0;
            mDiff = 0;
        }

        static IUx[] gaA = null;
        static IUx[] gaB = null;
        public static IUx[] GetIUs(ExamLv lv)
        {
            if(lv == ExamLv.A)
            {
                if (gaA == null)
                {
                    gaA = new IUx[6];
                    gaA[0] = IUx._1;
                    gaA[1] = IUx._2;
                    gaA[2] = IUx._3;
                    gaA[3] = IUx._4;
                    gaA[4] = IUx._5;
                    gaA[5] = IUx._6;
                }
                return gaA;
            }
            else
            {
                if (gaB == null)
                {
                    gaB = new IUx[3];
                    gaB[0] = IUx._7;
                    gaB[1] = IUx._8;
                    gaB[2] = IUx._10;
                }
                return gaB;
            }
        }

        static IUx[] gaAll = null;
        public static IUx[] GetAllIUs()
        {
            if(gaAll == null)
            {
                gaAll = new IUx[15];
                gaAll[0] = IUx._1;
                gaAll[1] = IUx._2;
                gaAll[2] = IUx._3;
                gaAll[3] = IUx._4;
                gaAll[4] = IUx._5;
                gaAll[5] = IUx._6;
                gaAll[6] = IUx._7;
                gaAll[7] = IUx._8;
                gaAll[8] = IUx._9;
                gaAll[9] = IUx._10;
                gaAll[10] = IUx._11;
                gaAll[11] = IUx._12;
                gaAll[12] = IUx._13;
                gaAll[13] = IUx._14;
                gaAll[14] = IUx._15;
            }
            return gaAll;
        }

        //only Optimization0 uses this.
        //optimization: return List<byte[]> instead of byte[]
        public List<byte[]> ToByte()
        {
            List<byte[]> l = new List<byte[]>();
            l.Add(BitConverter.GetBytes(uId));
            //optmz//l.Add(BitConverter.GetBytes((int)eLv));
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
            if (l < 4)
                return true;
            uId = BitConverter.ToInt32(buf, offs);
            offs += 4;
            l -= 4;
            /*optmz
             * if (l < 4)
             *  return true;
             * int x;
             * if(Enum.IsDefined(typeof(ExamLv), x = BitConverter.ToInt32(buf, offs)))
             *  eLv = (ExamLv)x;
             * else
             *  return true;
             * offs += 4;
             * l -= 4;*/
            if (l < 4)
                return true;
            int nq = BitConverter.ToInt32(buf, offs);
            offs += 4;
            l -= 4;
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

        //only Server0 uses this.
        public void DBSelect(IUx eIU)
        {
            vQuest.Clear();
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return;
            string qry = DBConnect.mkQrySelect("sqz_question",
                "id,stmt,ans0,ans1,ans2,ans3,`key`", "mid=" + (int)eIU + " AND del=0");
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
        public void DBSelect(IUx eIU, int n)
        {
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return;
            string tbl = "q" + (int)eIU;
            //randomize
            string eMsg;
            int nn = DBConnect.Count(conn, tbl, "id", null, out eMsg);
            if (nn < 1 || nn < n)
                return;
            int[] vSel = new int[n];
            int i;
            for (i = 0; i < n; ++i)
                vSel[i] = -1;
            ++sSeed;
            if (sSeed == int.MaxValue)
                sSeed = 1;
            Random r = new Random(sSeed);
            i = n;
            while (0 < i)
            {
                int sel = r.Next() % nn;
                int idx = Array.IndexOf(vSel, sel);
                bool fw = sel % 2 == 0;
                while (-1 < idx)
                {
                    if (fw)
                        ++sel;
                    else
                        --sel;
                    if (sel < 0 || sel == nn)
                    {
                        fw = sel < 0;
                        continue;
                    }
                    idx = Array.IndexOf(vSel, sel);
                }
                --i;
                vSel[i] = sel;
            }
            Array.Sort(vSel);
            //
            string qry = DBConnect.mkQrySelect(tbl, null, null);
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
                    string[] s = reader.GetString(1).Split('\n');
                    q.mStmt = "(" + tbl + ')' + s[0];
                    q.nAns = 4;
                    q.vAns = new string[4];
                    for (int k = 0; k < 4; ++k)
                        q.vAns[k] = s[k + 1];
                    string x = reader.GetString(2);
                    q.vKeys = new bool[4];
                    for (int k = 0; k < 4; ++k)
                        q.vKeys[k] = (x[k] == '1');
                    q.mIU = eIU;
                    vQuest.Add(q);
                }
                reader.Close();
            }
            DBConnect.Close(ref conn);
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
            DBConnect.Ins(conn, "sqz_question", "mid,del,diff,stmt,ans0,ans1,ans2,ans3,`key`",
                vals.ToString(), out eMsg);
            DBConnect.Close(ref conn);
        }

        public void DBAppendInsQry(DateTime dt, ref StringBuilder vals)
        {
            vals.Append("('" + dt.ToString(DT._) + "','" + eLv.ToString() + "'," + uId + ",'");
            foreach(Question q in vQuest)
                vals.Append(((int)q.mIU).ToString() + '_' + q.uId + '-');
            vals.Remove(vals.Length - 1, 1);//remove the last '-'
            vals.Append("'),");
        }

        public bool DBSelect(DateTime dt, ExamLv lv, int id)
        {
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return true;
            StringBuilder cond = new StringBuilder();
            cond.Append("dt='" + dt.ToString(DT._) + "'");
            cond.Append(" AND lv=" + lv.ToString("d"));
            cond.Append(" AND id=" + id);
            string qry = DBConnect.mkQrySelect("qs", "vquest", cond.ToString());
            string eMsg;
            MySqlDataReader reader = DBConnect.exeQrySelect(conn, qry, out eMsg);
            string qIds = null;
            if (reader != null && reader.Read())
            {
                qIds = reader.GetString(0);
                reader.Close();
            }
            if (qIds == null)
            {
                DBConnect.Close(ref conn);
                return true;
            }
            uId = id;
            vQuest.Clear();
            string[] vQId = qIds.Split('-');
            foreach(string qid in vQId)
            {
                string[] iuid = qid.Split('_');
                if(iuid.Length == 2)//todo handle error
                {
                    qry = DBConnect.mkQrySelect("q" + iuid[0], null, "id=" + iuid[1]);
                    reader = null;//todo DBConnect.exeQrySelect(conn, qry);
                    if (reader != null)//todo handle error
                    {
                        if(reader.Read())//todo handle error
                        {
                            Question q = new Question();
                            q.uId = reader.GetInt32(0);
                            string[] s = reader.GetString(1).Split('\n');
                            q.mStmt = "(" + iuid[0] + ')' + s[0];
                            q.nAns = 4;
                            q.vAns = new string[4];
                            for (int k = 0; k < 4; ++k)
                                q.vAns[k] = s[k + 1];
                            string x = reader.GetString(2);
                            q.vKeys = new bool[4];
                            for (int k = 0; k < 4; ++k)
                                q.vKeys[k] = (x[k] == '1');
                            q.mIU = (IUx)int.Parse(iuid[0]);
                            vQuest.Add(q);
                        }
                        reader.Close();
                    }
                }
            }
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
            int uid = DBConnect.MaxInt(conn, "qs", "id",
                    "dt='" + dt.ToString(DT._) + "' AND lv='" + ExamLv.A.ToString() + "'");
            if (uid < 0)
            {
                DBConnect.Close(ref conn);
                return true;
            }
            guDBCurAId = uid;

            uid = DBConnect.MaxInt(conn, "qs", "id",
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
}
