using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace sQzLib
{
    public enum ExamLvl
    {
        Basis = -1,
        Advance = 1,
    }
    public class QuestSheet
    {
        ExamLvl eLvl;
        public uint mId;
        public List<Question> vQuest;
        public byte[] aQuest;

        static int sSeed = 1;

        public QuestSheet()
        {
            eLvl = ExamLvl.Basis;
            vQuest = new List<Question>();
            aQuest = null;
            mId = 0;
        }

        public static int[] GetBasicIU()
        {
            int[] x = new int[6];
            x[0] = 1;
            x[1] = 2;
            x[2] = 3;
            x[3] = 4;
            x[4] = 5;
            x[5] = 6;
            return x;
        }

        public static int[] GetAdvanceIU()
        {
            int[] x = new int[3];
            x[0] = 7;
            x[1] = 8;
            x[2] = 10;
            return x;
        }

        //only Optimization0 uses this.
        //optimization: return List<byte[]> instead of byte[]
        public List<byte[]> ToByte(bool woKey)
        {
            woKey = false;
            List<byte[]> l = new List<byte[]>();
            //List<bool> lk = new List<bool>();
            l.Add(BitConverter.GetBytes(mId));
            l.Add(BitConverter.GetBytes(vQuest.Count));
            foreach (Question q in vQuest)
            {
                //qType
                //l.Add(BitConverter.GetBytes((int)q.qType));
                //if (woKey)
                //    lk.Add(false);
                //stmt
                byte[] b = Encoding.UTF8.GetBytes(q.mStmt);
                l.Add(BitConverter.GetBytes(b.Length));
                l.Add(b);
                //if (woKey)
                //{
                //    lk.Add(false);
                //    lk.Add(false);
                //}
                //ans
                //l.Add(BitConverter.GetBytes(q.nAns));
                //if (woKey)
                //    lk.Add(false);
                for (int j = 0; j < q.nAns; ++j)
                {
                    //each ans
                    b = Encoding.UTF8.GetBytes(q.vAns[j]);
                    l.Add(BitConverter.GetBytes(b.Length));
                    l.Add(b);
                    //if (woKey)
                    //{
                    //    lk.Add(false);
                    //    lk.Add(false);
                    //}
                }
                //keys
                for (int j = 0; j < q.nAns; ++j)
                {
                    l.Add(BitConverter.GetBytes(q.vKeys[j]));
                    //if (woKey)
                    //    lk.Add(true);
                }
            }
            return l;
        }

        public bool ReadByte(byte[] buf, ref int offs, bool wKey)
        {
            wKey = true;
            if (buf == null)
                return true;
            int offs0 = offs;
            int l = buf.Length - offs;
            if (l < 4)
                return true;
            mId = BitConverter.ToUInt32(buf, offs);
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
                --nq;
                vQuest.Add(q);
            }
            if (wKey && !Array.Equals(buf, aQuest))
            {
                int sz = offs - offs0;
                if (sz == buf.Length)
                    aQuest = (byte[])buf.Clone();
                else
                {
                    aQuest = new byte[sz];
                    Buffer.BlockCopy(buf, offs0, aQuest, 0, sz);
                }
                //sRdy = false;
                //sRdywKey = true;
            }
            //if (!wKey && !Array.Equals(buf, sbArr))
            //{
            //    sz = offs - offs0;
            //    if (sz == buf.Length)
            //        sbArr = (byte[])buf.Clone();
            //    else
            //    {
            //        sbArr = new byte[sz];
            //        Buffer.BlockCopy(buf, 0, sbArrwKey, 0, sz);
            //    }
            //    sRdy = true;
            //    sRdywKey = false;
            //}
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
            //sRdy = sRdywKey = false;
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
            string qry = DBConnect.mkQrySelect("quest" + iu, null, null, null, null);
            MySqlDataReader reader = DBConnect.exeQrySelect(conn, qry);
            if (reader != null)
            {
                while (reader.Read())
                {
                    Question q = new Question();
                    q.mId = reader.GetUInt32(0);//hardcode
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
            string qry = DBConnect.mkQrySelect("quest" + iu, null, null, null, null);
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
                    q.mId = reader.GetUInt32(0);//hardcode
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
    }
}
