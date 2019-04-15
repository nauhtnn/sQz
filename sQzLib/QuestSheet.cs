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
        public ExamLv eLv;
        public static int guDBCurAId;
        public static int guDBCurBId;
        public int uId;
        public int LvId { get { return (eLv == ExamLv.A) ? uId : uId + ExamineeA.LV_CAP; } }
        public string tId { get { return eLv.ToString() + uId.ToString("d3"); } }
        public bool bAlt;
        List<MCItem> vQuest;
        public byte[] aQuest;
        public int Count { get { return vQuest.Count; } }
        public int CountD
        {
            get
            {
                int n = 0;
                foreach (MCItem q in vQuest)
                    if (q.IsDifficult)
                        ++n;
                return n;
            }
        }

        public QuestSheet()
        {
            eLv = ExamLv.A;
            vQuest = new List<MCItem>();
            aQuest = null;
            uId = ExamineeA.LV_CAP;
            bAlt = false;
        }

        public static IUx[] GetIUs(ExamLv lv)
        {
            IUx[] x;
            if (lv == ExamLv.A)
            {
                x = new IUx[6];
                x[0] = IUx._1;
                x[1] = IUx._2;
                x[2] = IUx._3;
                x[3] = IUx._4;
                x[4] = IUx._5;
                x[5] = IUx._6;
            }
            else
            {
                x = new IUx[3];
                x[0] = IUx._7;
                x[1] = IUx._8;
                x[2] = IUx._10;
            }
            return x;
        }

        public static bool ParseLvId(string s, out ExamLv lv, out int id)
        {
            if (s == null || s.Length != 4)
            {
                lv = ExamLv.A;
                id = ExamineeA.LV_CAP;
                return true;
            }
            s = s.ToUpper();
            if (!Enum.TryParse(s.Substring(0, 1), out lv))
            {
                id = ExamineeA.LV_CAP;
                return true;
            }
            if (!int.TryParse(s.Substring(1), out id))
                return true;
            if (id < 1 || ExamineeA.LV_CAP <= id)
                return true;
            return false;
        }

        public MCItem Q(int idx)
        {
            return vQuest[idx];
        }

        public void Add(MCItem q)
        {
            vQuest.Add(q);
        }

        public void Clear()
        {
            vQuest.Clear();
        }

        public List<int[]> GetNMod()
        {
            List<int[]> rv = new List<int[]>();
            IUx[] viu = GetIUs(eLv);
            int[] vnesydif = new int[viu.Length];
            int[] vndif = new int[viu.Length];
            foreach (MCItem q in vQuest)
            {
                int idx = (int)q.eIU;//mod on 0705
                if (idx == 6 || idx == 7)
                    idx = idx - 6;
                else if (idx == 9)
                    idx = 2;
                ++vnesydif[idx];
                if (q.IsDifficult)
                    ++vndif[idx];
            }
            rv.Add(vnesydif);
            rv.Add(vndif);
            return rv;
        }

        public static List<int[]> DBGetNMod(ExamLv lv)
        {
            List<int[]> rv = new List<int[]>();
            IUx[] viu = GetIUs(lv);
            int[] vn = new int[viu.Length];
            int[] vnd = new int[viu.Length];
            int j = -1;
            foreach(IUx i in viu)
            {
                int n = DBConnect.Count( "sqz_question", "id",
                    "moid=" + (int)i + " AND del=0");
                if (n < 0)
                    n = 0;
                vn[++j] = n;
                n = DBConnect.Count("sqz_question", "id",
                    "moid=" + (int)i + " AND diff=1 AND del=0");
                if (n < 0)
                    n = 0;
                vnd[j] = n;
            }
            rv.Add(vn);
            rv.Add(vnd);
            return rv;
        }

        public static int DBGetND(IUx iu)
        {
            int n = DBConnect.Count("sqz_question", "id",
                    "moid=" + (int)iu + " AND diff=1 AND del=0");
            if (n < 0)
                n = 0;
            return n;
        }

        //public void DBAppendQryIns(string prefx, StringBuilder vals)
        //{
        //    int idx = -1;
        //    foreach (MCItem q in vQuest)
        //    {
        //        vals.Append(prefx + "'" + eLv.ToString() + "'," +
        //            uId + "," + q.uId + ",'");
        //        foreach (int i in q.POptions)
        //            vals.Append(i.ToString());
        //        vals.Append("'," + ++idx + "),");
        //    }
        //}

        //only Operation0 uses this.
        public void ExtractKey(AnsSheet anssh)
        {
            anssh.uQSLvId = LvId;
            if (0 < vQuest.Count)
                anssh.aAns = new byte[vQuest.Count * MCItem.N_OPTIONS];
            else
                return;
            int i = -1;
            foreach (MCItem q in vQuest)
                foreach (bool x in q.Keys)
                    anssh.aAns[++i] = Convert.ToByte(x);
        }

        public List<byte[]> ToByte()
        {
            List<byte[]> l = new List<byte[]>();
            //l.Add(BitConverter.GetBytes((int)eLv));
            //l.Add(BitConverter.GetBytes(uId));
            //l.Add(BitConverter.GetBytes(vQuest.Count));
            //foreach (MCItem q in vQuest)
            //{
            //    //Stem
            //    byte[] b = Encoding.UTF8.GetBytes(q.GetStem);
            //    l.Add(BitConverter.GetBytes(b.Length));
            //    l.Add(b);
            //    //ans
            //    for (int j = 0; j < MCItem.N_OPTIONS; ++j)
            //    {
            //        //each ans
            //        b = Encoding.UTF8.GetBytes(q.vAns[j]);
            //        l.Add(BitConverter.GetBytes(b.Length));
            //        l.Add(b);
            //    }
            //}
            return l;
        }

        public bool ReadByte(byte[] buf, ref int offs)
        {
            //if (buf == null)
            //    return true;
            //int offs0 = offs;
            //int l = buf.Length - offs;
            ////
            //if (l < 12)
            //    return true;
            //int x;
            //if (Enum.IsDefined(typeof(ExamLv), x = BitConverter.ToInt32(buf, offs)))
            //    eLv = (ExamLv)x;
            //else
            //    return true;
            //offs += 4;
            //l -= 4;
            //uId = BitConverter.ToInt32(buf, offs);
            //offs += 4;
            //l -= 4;
            //int nq = BitConverter.ToInt32(buf, offs);
            //offs += 4;
            //l -= 4;
            ////
            //vQuest = new List<MCItem>();
            //while (0 < nq)
            //{
            //    MCItem q = new MCItem();
            //    //Stem
            //    if (l < 4)
            //        return true;
            //    int sz = BitConverter.ToInt32(buf, offs);
            //    l -= 4;
            //    offs += 4;
            //    if (l < sz)
            //        return true;
            //    q.Stem = Encoding.UTF8.GetString(buf, offs, sz);
            //    l -= sz;
            //    offs += sz;
            //    //ans
            //    q.vAns = new string[MCItem.N_OPTIONS];
            //    for (int j = 0; j < MCItem.N_OPTIONS; ++j)
            //    {
            //        //each ans
            //        if (l < 4)
            //            return true;
            //        sz = BitConverter.ToInt32(buf, offs);
            //        l -= 4;
            //        offs += 4;
            //        if (l < sz)
            //            return true;
            //        q.vAns[j] = Encoding.UTF8.GetString(buf, offs, sz);
            //        l -= sz;
            //        offs += sz;
            //    }
            //    --nq;
            //    vQuest.Add(q);
            //}
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
            return false;
        }

        public void ParseDocx(string path)
        {
            string[] rawData = Utils.ReadAllLines(path);
            ParseArray(rawData);
        }

        void ParseArray(string[] rawData)
        {
            vQuest.Clear();
            int stride = 1 + MCItem.N_OPTIONS;
            for (int i = 0; i + stride < rawData.Length; i += stride)
            {
                MCItem q = new MCItem();
                q.Parse(rawData, i);
                vQuest.Add(q);
            }
        }

        public void WriteTxt(string fpath)
        {
            System.IO.File.WriteAllText(fpath, ToString());
        }

        public IEnumerable<string> ToListOfStrings()
        {
            IEnumerable<string> s = new LinkedList<string>();
            foreach (MCItem q in vQuest)
                s = s.Concat(q.ToListOfStrings()) as IEnumerable<string>;
                
            return s;
        }

        public List<MCItem> ShallowCopy()
        {
            List<MCItem> l = new List<MCItem>();
            foreach (MCItem q in vQuest)
                l.Add(q);
            return l;
        }

        public QuestSheet DeepCopy()
        {
            QuestSheet qs = new QuestSheet();
            qs.eLv = eLv;
            qs.uId = uId;
            qs.bAlt = bAlt;
            foreach (MCItem qi in vQuest)
                qs.vQuest.Add(qi.DeepCopy());
            return qs;
        }

        public void Randomize(Random rand)
        {
            List<MCItem> qs = new List<MCItem>();
            int n = vQuest.Count;
            while (0 < n)
            {
                int sel = rand.Next() % n;
                qs.Add(vQuest[sel]);
                vQuest.RemoveAt(sel);
                --n;
            }
            vQuest = qs;
            foreach (MCItem q in vQuest)
                q.Randomize(rand);
        }

        public QuestSheet RandomizeDeepCopy(Random rand)
        {
            QuestSheet qs = new QuestSheet();
            qs.eLv = eLv;
            qs.uId = uId;
            qs.bAlt = bAlt;
            foreach (MCItem qi in vQuest)
                qs.vQuest.Add(qi.RandomizeDeepCopy(rand));
            //randomize
            List<MCItem> lq = new List<MCItem>();
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

        public int[] DBCount()
		{
            IUx[] ius = new IUx[GetIUs(ExamLv.A).Count() + GetIUs(ExamLv.B).Count()];
            int i = -1;
            foreach (IUx iu in GetIUs(ExamLv.A))
                ius[++i] = iu;
            foreach (IUx iu in GetIUs(ExamLv.B))
                ius[++i] = iu;
			int[] nn = new int[ius.Length];
			i = -1;
			StringBuilder emsg = new StringBuilder();
			foreach(IUx iu in ius)
			{
				nn[++i] = DBConnect.Count("sqz_question", "id",
					"moid=" + (int)iu + " AND del=0");
			}
			return nn;
		}

        //only Server0 uses this.
        public void DBSelect(IUx eIU, QuestDiff d)
        {
            //vQuest.Clear();
            //string cond = string.Empty;
            //if (d == QuestDiff.Easy)
            //    cond = " AND diff=0";
            //else if (d == QuestDiff.Diff)
            //    cond = " AND diff=1";
            //MySqlDataReader reader = DBConnect.exeQrySelect("sqz_question",
            //    "id,diff,Stem,ans0,ans1,ans2,ans3,`key`", "moid=" + (int)eIU + " AND del=0" + cond);
            //if (reader != null)
            //{
            //    while (reader.Read())
            //    {
            //        int itemID = reader.GetInt32(0);
            //        bool isDifficult = reader.GetInt16(1) != 0;
            //        string[] cleanData = new string[MCItem.N_OPTIONS + 1];
            //        cleanData[0] = reader.GetString(2);
            //        for (int i = 1; i <= MCItem.N_OPTIONS; ++i)
            //            cleanData[i] = reader.GetString(2 + i);
            //        string keyStr = reader.GetString(7);
            //        bool[] keys = new bool[4];
            //        for (int i = 0; i < 4; ++i)
            //            keys[i] = (keyStr[i] == MCItem.C1);
            //        vQuest.Add(new MCItem(itemID, cleanData, keys, isDifficult));
            //    }
            //    reader.Close();
            //}
            throw new NotImplementedException();
        }

        //only Server0 uses this.
        public bool DBSelect(Random rand, IUx iu, int n, QuestDiff d)
        {
            //randomize
            //string qry = "moid=" + (int)iu + " AND del=0";
            //if (d == QuestDiff.Easy)
            //    qry = qry + " AND diff=0";
            //else if (d == QuestDiff.Diff)
            //    qry = qry + " AND diff=1";
            //int nn = DBConnect.Count("sqz_question", "id", qry);
            //if (nn < n)
            //    return true;
            //List<int> vIds = new List<int>();
            //int i;
            //for (i = 0; i < nn; ++i)
            //    vIds.Add(i);
            //int[] vSel = new int[n];
            //i = n;
            //while (0 < i)
            //{
            //    --i;
            //    int idx = rand.Next() % nn;
            //    --nn;
            //    vSel[i] = vIds[idx];
            //    vIds.RemoveAt(idx);
            //}
            //Array.Sort(vSel);
            ////
            //string cond = string.Empty;
            //if (d == QuestDiff.Diff)
            //    cond = " AND diff=1";
            //else
            //    cond = " AND diff=0";
            //MySqlDataReader reader = DBConnect.exeQrySelect("sqz_question",
            //    "id,diff,Stem,ans0,ans1,ans2,ans3,`key`", "moid=" + (int)iu + " AND del=0" + cond);
            //i = 0;
            //int ii = -1;
            //if (reader != null)
            //{
            //    while (reader.Read() && i < n)
            //    {
            //        if (++ii != vSel[i])
            //            continue;
            //        ++i;

            //        int itemID = reader.GetInt32(0);
            //        bool isDifficult = reader.GetInt16(1) != 0;
            //        string[] cleanData = new string[MCItem.N_OPTIONS + 1];
            //        cleanData[0] = reader.GetString(2);
            //        for (int j = 1; j <= MCItem.N_OPTIONS; ++j)
            //            cleanData[j] = reader.GetString(2 + j);
            //        string keyStr = reader.GetString(7);
            //        bool[] keys = new bool[4];
            //        for (int j = 0; j < 4; ++j)
            //            keys[j] = (keyStr[j] == MCItem.C1);
            //        vQuest.Add(new MCItem(itemID, cleanData, keys, isDifficult));
            //    }
            //    reader.Close();
            //}
            throw new NotImplementedException();
        }

        public void DBIns(IUx eIU)
        {
            StringBuilder vals = new StringBuilder();
            foreach (MCItem q in vQuest)
            {
                vals.Append("(" + (int)eIU + ",0," + (q.IsDifficult ? 1 : 0) + ",'");
                vals.Append(q.Stem.Replace("'", "\\'") + "','");
                for (int i = 0; i < MCItem.N_OPTIONS; ++i)
                    vals.Append(q.Options[i].Replace("'", "\\'") + "','");
                for (int i = 0; i < MCItem.N_OPTIONS; ++i)
                    if (q.Keys[i])
                        vals.Append(MCItem.C1);
                    else
                        vals.Append(MCItem.C0);
                vals.Append("'),");
            }
            vals.Remove(vals.Length - 1, 1);//remove the last comma
            DBConnect.Ins("sqz_question", "moid,del,diff,Stem,ans0,ans1,ans2,ans3,`key`",
                vals.ToString());
        }

        public bool DBSelect(MySqlConnection conn, DateTime dt, ExamLv lv, int id, out string eMsg)
        {
            //vQuest.Clear();
            //uId = id;
            //eLv = lv;
            //string qry = DBConnect.mkQrySelect("sqz_qsheet_quest", "qid,asort,idx",
            //    "dt='" + dt.ToString(DT._) + "' AND lv='" + lv.ToString() +
            //    "' AND qsid=" + id);
            //MySqlDataReader reader = DBConnect.exeQrySelect(conn, qry, out eMsg);
            //if (reader == null)
            //    return true;
            //List<uint> qids = new List<uint>();
            //List<string> asorts = new List<string>();
            //QIdxComparer<MCItem> qComparer = new QIdxComparer<MCItem>();
            //while (reader.Read())
            //{
            //    uint qid = reader.GetUInt32(0);
            //    qids.Add(qid);
            //    asorts.Add(reader.GetString(1));
            //    qComparer.Add((int)qid, reader.GetInt16(2));
            //}
            //reader.Close();
            //int i = -1;
            //foreach(int qid in qids)
            //{
            //    ++i;
            //    qry = DBConnect.mkQrySelect("sqz_question",
            //        "diff,Stem,ans0,ans1,ans2,ans3,`key`,moid", "id=" + qid);
            //    reader = DBConnect.exeQrySelect(conn, qry, out eMsg);
            //    if (reader == null)
            //        return true;
            //    while (reader.Read())
            //    {
            //        MCItem q = new MCItem();
            //        q.uId = qid;
            //        q.IsDifficult = reader.GetInt16(0) != 0;
            //        q.GetStem = reader.GetString(1);
            //        string[] anss = new string[MCItem.N_OPTIONS];
            //        for (int j = 0; j < MCItem.N_OPTIONS; ++j)
            //            anss[j] = reader.GetString(2 + j);
            //        string x = reader.GetString(6);
            //        bool[] keys = new bool[MCItem.N_OPTIONS];
            //        for (int j = 0; j < MCItem.N_OPTIONS; ++j)
            //            keys[j] = (x[j] == MCItem.C1);
            //        q.vAns = new string[4];
            //        q.Keys = new bool[4];
            //        for(int j = 0; j < 4; ++j)
            //        {
            //            q.vAns[j] = anss[asorts[i][j] - MCItem.C0];
            //            q.Keys[j] = keys[asorts[i][j] - MCItem.C0];
            //        }
            //        int iu;
            //        if (Enum.IsDefined(typeof(IUx), iu = reader.GetInt32(7)))
            //            q.eIU = (IUx)iu;
            //        vQuest.Add(q);
            //    }
            //    reader.Close();
            //}
            //vQuest.Sort(qComparer);
            throw new NotImplementedException();
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
            int uid = DBConnect.MaxInt(conn, "sqz_qsheet", "id",
                    "dt='" + dt.ToString(DT._) + "' AND lv='" + ExamLv.A.ToString() + "'");
            if (uid < 0)
            {
                DBConnect.Close(ref conn);
                return true;
            }
            guDBCurAId = uid;

            uid = DBConnect.MaxInt(conn, "sqz_qsheet", "id",
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
            MCItem qx = x as MCItem;
            if (qx == null)
                return 0;
            MCItem qy = y as MCItem;
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
