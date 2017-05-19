using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

/*
CREATE TABLE IF NOT EXISTS `questsh` (`slId` INT(4) UNSIGNED, `lv` SMALLINT,
 `id` SMALLINT UNSIGNED, `vquest` VARCHAR(1024),
 PRIMARY KEY(`slId`,`lv`,`id`));
*/

namespace sQzLib
{
    public class QuestSheet
    {
        public ExamLv eLv;
        public int uId;
        public List<Question> vQuest;
        public byte[] aQuest;

        static int sSeed = 1;

        public QuestSheet()
        {
            eLv = ExamLv.A;
            vQuest = new List<Question>();
            aQuest = null;
            uId = 0;
        }

        static int[] aB = null;
        static int[] aA = null;
        public static int[] GetIUId(int lv)
        {
            if(lv == -1 || lv == 0)
            {
                if (aB == null)
                {
                    aB = new int[6];
                    aB[0] = 1;
                    aB[1] = 2;
                    aB[2] = 3;
                    aB[3] = 4;
                    aB[4] = 5;
                    aB[5] = 6;
                }
                return aB;
            }
            else
            {
                if (aA == null)
                {
                    aA = new int[3];
                    aA[0] = 7;
                    aA[1] = 8;
                    aA[2] = 10;
                }
                return aA;
            }
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
            if (l < 4)
                return true;
            uId = BitConverter.ToInt32(buf, offs);
            offs += 4;
            l -= 4;
            if (l < 4)
                return true;
            int nq = BitConverter.ToInt32(buf, offs);
            offs += 4;
            l -= 4;
            vQuest = new List<Question>();
            bool err = false;
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
                {
                    err = true;
                    break;
                }
                int sz = BitConverter.ToInt32(buf, offs);
                l -= 4;
                offs += 4;
                if (l < sz) {
                    err = true;
                    break;
                }
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
                    {
                        err = true;
                        break;
                    }
                    sz = BitConverter.ToInt32(buf, offs);
                    l -= 4;
                    offs += 4;
                    if (l < sz)
                    {
                        err = true;
                        break;
                    }
                    q.vAns[j] = Encoding.UTF8.GetString(buf, offs, sz);
                    l -= sz;
                    offs += sz;
                }
                if (err)
                    break;
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
            return err;
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
        public void DBSelect(IUxx eIU)
        {
            vQuest.Clear();
            if (eIU == IUxx.IU00)
                return;
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return;
            string iu = eIU.ToString().Substring(2);//hardcode
            if (iu[0] == '0')
                iu = iu.Substring(1);
            string qry = DBConnect.mkQrySelect("quest" + iu, null, null, null);
            MySqlDataReader reader = DBConnect.exeQrySelect(conn, qry);
            if (reader != null)
            {
                while (reader.Read())
                {
                    Question q = new Question();
                    q.uId = reader.GetUInt32(0);//hardcode
                    string[] s = reader.GetString(1).Split('\n');
                    q.mStmt = s[0];
                    q.nAns = 4;
                    q.vAns = new string[4];
                    for (int i = 0; i < 4; ++i)
                        q.vAns[i] = s[i + 1];
                    string x = reader.GetString(2);
                    q.vKeys = new bool[4];
                    for (int i = 0; i < 4; ++i)
                        q.vKeys[i] = (x[i] == '1');
                    q.mIU = eIU;
                    vQuest.Add(q);
                }
                reader.Close();
            }
            DBConnect.Close(ref conn);
        }

        //only Server0 uses this.
        public void DBSelect(IUxx eIU, int n)
        {
            if (eIU == IUxx.IU00)
                return;
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return;
            string iu = eIU.ToString().Substring(2);//hardcode
            if (iu[0] == '0')
                iu = iu.Substring(1);
            //randomize
            int nn = DBConnect.Count(conn, "quest" + iu);
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
            string qry = DBConnect.mkQrySelect("quest" + iu, null, null, null);
            MySqlDataReader reader = DBConnect.exeQrySelect(conn, qry);
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
                    q.uId = reader.GetUInt32(0);//hardcode
                    string[] s = reader.GetString(1).Split('\n');
                    q.mStmt = "(" + iu + ')' + s[0];
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

        public void DBInsert(IUxx eIU)
        {
            if (eIU == IUxx.IU00)
                return;
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return;
            StringBuilder vals = new StringBuilder();
            foreach (Question q in vQuest)
            {
                vals.Append("('" + q.mStmt.Replace("'", "\\'") + '\n');
                for (int i = 0; i < q.nAns; ++i)
                    vals.Append(q.vAns[i].Replace("'", "\\'") + '\n');
                vals.Append("','");
                for (int i = 0; i < q.nAns; ++i)
                    if (q.vKeys[i])
                        vals.Append('1');
                    else
                        vals.Append('0');
                vals.Append("'),");
            }
            vals.Remove(vals.Length - 1, 1);//remove the last comma
            string iu = eIU.ToString().Substring(2);//hardcode
            if (iu[0] == '0')
                iu = iu.Substring(1);
            DBConnect.Ins(conn, "quest" + iu, "body,ansKeys", vals.ToString());
            DBConnect.Close(ref conn);
        }

        public void DBAppendInsQry(uint slId, ref StringBuilder vals)
        {
            vals.Append("(" + slId + "," + (int)eLv + "," + uId + ",'");
            foreach(Question q in vQuest)
                vals.Append(((short)q.mIU).ToString() + '_' + q.uId + '-');
            vals.Remove(vals.Length - 1, 1);//remove the last '-'
            vals.Append("'),");
        }

        public bool DBSelect(uint slId, short lv, ushort idx)
        {
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return false;
            StringBuilder cond = new StringBuilder();
            cond.Append("slId=" + slId);
            cond.Append(" AND lv=" + lv);
            cond.Append(" AND id=" + idx);
            string qry = DBConnect.mkQrySelect("questsh", "vquest", cond.ToString(), null);
            MySqlDataReader reader = DBConnect.exeQrySelect(conn, qry);
            string qIds = null;
            if (reader != null && reader.Read())
            {
                qIds = reader.GetString(0);
                reader.Close();
            }
            if (qIds == null)
            {
                DBConnect.Close(ref conn);
                return false;
            }
            uId = idx;
            vQuest.Clear();
            string[] vQId = qIds.Split('-');
            foreach(string qid in vQId)
            {
                string[] iuid = qid.Split('_');
                if(iuid.Length == 2)
                {
                    qry = DBConnect.mkQrySelect("quest" + iuid[0], null, "idx=" + iuid[1], null);
                    reader = DBConnect.exeQrySelect(conn, qry);
                    if (reader != null)
                    {
                        if(reader.Read())
                        {
                            Question q = new Question();
                            q.uId = reader.GetUInt32(0);//hardcode
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
                            q.mIU = (IUxx)int.Parse(iuid[0]);
                            vQuest.Add(q);
                        }
                        reader.Close();
                    }
                }
            }
            return true;
        }
    }
}
