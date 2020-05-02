using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;

namespace sQzLib
{
    public class QuestSheet
    {
        public static int guDBCurAId;
        public static int guDBCurBId;
        public int uId;
        public Level Lv;
        public int LvId { get { return (Lv == Level.A) ? uId : uId + (int)Level.MAX_COUNT_EACH_LEVEL; } }
        public string tId { get { return Lv.ToString() + uId.ToString("d3"); } }
        public List<MultiChoiceItem> Questions { get; private set; }
        public byte[] ItemsInBytes;
        public int CountDifficult
        {
            get
            {
                int n = 0;
                foreach (MultiChoiceItem q in Questions)
                    if (q.IsDifficult)
                        ++n;
                return n;
            }
        }

        public QuestSheet()
        {
            Questions = new List<MultiChoiceItem>();
            ItemsInBytes = null;
            uId = (int)Level.MAX_COUNT_EACH_LEVEL;
            Lv = Level.A;
        }

        public static bool ParseLvId(string s, out Level lv, out int id)
        {
            if (s == null || s.Length != 4)
            {
                lv = Level.A;
                id = (int)Level.MAX_COUNT_EACH_LEVEL;
                return true;
            }
            s = s.ToUpper();
            if (!Enum.TryParse(s.Substring(0, 1), out lv))
            {
                id = (int)Level.MAX_COUNT_EACH_LEVEL;
                return true;
            }
            if (!int.TryParse(s.Substring(1), out id))
                return true;
            if (id < 1 || (int)Level.MAX_COUNT_EACH_LEVEL <= id)
                return true;
            return false;
        }

        public MultiChoiceItem Q(int idx)
        {
            return Questions[idx];
        }

        public void Add(MultiChoiceItem q)
        {
            Questions.Add(q);
        }

        public void Clear()
        {
            Questions.Clear();
        }

        public List<int[]> CountItemGroupByModule()
        {
            IUx[] IUs_by_Lv = MultiChoiceItem.GetIUs(Lv);
            int[] n_difficultItems = new int[IUs_by_Lv.Length];
            int[] n_allItems = new int[IUs_by_Lv.Length];
            foreach (MultiChoiceItem i in Questions)
            {
                int module;
                switch(i.mIU)
                {
                    case IUx._7:
                        module = 0;
                        break;
                    case IUx._8:
                        module = 1;
                        break;
                    case IUx._9:
                        module = 2;
                        break;
                    default:
                        module = (int)i.mIU;
                        break;
                }
                ++n_allItems[module];
                if (i.IsDifficult)
                    ++n_difficultItems[module];
            }
            List<int[]> n = new List<int[]>();
            n.Add(n_allItems);
            n.Add(n_difficultItems);
            return n;
        }

        public static List<int[]> DBCountItemGroupByModule(Level lv)
        {
            IUx[] IUs_by_Lv = MultiChoiceItem.GetIUs(lv);
            int[] n_difficultItems = new int[IUs_by_Lv.Length];
            int[] n_allItems = new int[IUs_by_Lv.Length];
            int j = -1;
            foreach(IUx i in IUs_by_Lv)
            {
                n_allItems[++j] = DBConnect.Count( "sqz_question", "id",
                    "moid=" + (int)i + " AND del=0");
                if (n_allItems[j] < 0)
                    n_allItems[++j] = 0;
                n_difficultItems[j] = DBConnect.Count("sqz_question", "id",
                    "moid=" + (int)i + " AND diff=1 AND del=0");
                if (n_difficultItems[j] < 0)
                    n_difficultItems[j] = 0;
            }
            List<int[]> n = new List<int[]>();
            n.Add(n_allItems);
            n.Add(n_difficultItems);
            return n;
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
        //    foreach (MultiChoiceItem q in items)
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
            if (0 < Questions.Count)
                anssh.aAns = new byte[Questions.Count * MultiChoiceItem.N_OPTIONS];
            else
                return;
            int i = -1;
            foreach (MultiChoiceItem q in Questions)
                foreach (bool x in q.Keys)
                    anssh.aAns[++i] = Convert.ToByte(x);
        }

        public List<byte[]> ToByte()
        {
            List<byte[]> l = new List<byte[]>();
            //l.Add(BitConverter.GetBytes((int)eLv));
            //l.Add(BitConverter.GetBytes(uId));
            //l.Add(BitConverter.GetBytes(items.Count));
            //foreach (MultiChoiceItem q in items)
            //{
            //    //Stem
            //    byte[] b = Encoding.UTF8.GetBytes(q.GetStem);
            //    l.Add(BitConverter.GetBytes(b.Length));
            //    l.Add(b);
            //    //ans
            //    for (int j = 0; j < MultiChoiceItem.N_OPTIONS; ++j)
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
            //if (Enum.IsDefined(typeof(Level), x = BitConverter.ToInt32(buf, offs)))
            //    eLv = (Level)x;
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
            //items = new List<MultiChoiceItem>();
            //while (0 < nq)
            //{
            //    MultiChoiceItem q = new MultiChoiceItem();
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
            //    q.vAns = new string[MultiChoiceItem.N_OPTIONS];
            //    for (int j = 0; j < MultiChoiceItem.N_OPTIONS; ++j)
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
            //    items.Add(q);
            //}
            //if (!Array.Equals(buf, ItemsInBytes))
            //{
            //    int sz = offs - offs0;
            //    if (sz == buf.Length)
            //        ItemsInBytes = buf.Clone() as byte[];
            //    else
            //    {
            //        ItemsInBytes = new byte[sz];
            //        Buffer.BlockCopy(buf, offs0, ItemsInBytes, 0, sz);
            //    }
            //}
            return false;
        }

        public void ParseDocx(string path)
        {
            Queue<NonnullRichTextBuilder> richTexts = Utils.ReadAllLines(path);
            ParseQueue(richTexts);
        }

        void ParseQueue(Queue<NonnullRichTextBuilder> richTexts)
        {
            Questions.Clear();
            int stride = 1 + MultiChoiceItem.N_OPTIONS;
            while(richTexts.Count > 0)
                Questions.Add(MultiChoiceItem.NewWith(richTexts));
        }

        public void WriteTxt(string fpath)
        {
            System.IO.File.WriteAllText(fpath, ToString());
        }

        public IEnumerable<string> ToListOfStrings()
        {
            IEnumerable<string> s = new LinkedList<string>();
            foreach (MultiChoiceItem q in Questions)
                s = s.Concat(q.ToListOfStrings()) as IEnumerable<string>;
                
            return s;
        }

        public List<MultiChoiceItem> ShallowCopy()
        {
            List<MultiChoiceItem> l = new List<MultiChoiceItem>();
            foreach (MultiChoiceItem q in Questions)
                l.Add(q);
            return l;
        }

        public QuestSheet DeepCopy()
        {
            QuestSheet qs = new QuestSheet();
            qs.Lv = Lv;
            qs.uId = uId;
            foreach (MultiChoiceItem qi in Questions)
                qs.Questions.Add(qi.DeepCopy());
            return qs;
        }

        public void Randomize(Random rand)
        {
            List<MultiChoiceItem> qs = new List<MultiChoiceItem>();
            int n = Questions.Count;
            while (0 < n)
            {
                int sel = rand.Next() % n;
                qs.Add(Questions[sel]);
                Questions.RemoveAt(sel);
                --n;
            }
            Questions = qs;
            foreach (MultiChoiceItem q in Questions)
                q.Randomize(rand);
        }

        public QuestSheet RandomizeDeepCopy(Random rand)
        {
            QuestSheet qs = new QuestSheet();
            qs.Lv = Lv;
            qs.uId = uId;
            foreach (MultiChoiceItem qi in Questions)
                qs.Questions.Add(qi.RandomizeDeepCopy(rand));
            //randomize
            List<MultiChoiceItem> lq = new List<MultiChoiceItem>();
            int n = qs.Questions.Count;
            while (0 < n)
            {
                int idx = rand.Next() % n;
                lq.Add(qs.Questions.ElementAt(idx));
                qs.Questions.RemoveAt(idx);
                --n;
            }
            qs.Questions = lq;

            return qs;
        }

  //      public int[] DBCount()
		//{
  //          IUx[] ius = new IUx[LvA_IUx.Length + LvB_IUx.Length];
  //          int i = -1;
  //          foreach (IUx iu in GetIUs(Level.A))
  //              ius[++i] = iu;
  //          foreach (IUx iu in GetIUs(Level.B))
  //              ius[++i] = iu;
		//	int[] nn = new int[ius.Length];
		//	i = -1;
		//	StringBuilder emsg = new StringBuilder();
		//	foreach(IUx iu in ius)
		//	{
		//		nn[++i] = DBConnect.Count("sqz_question", "id",
		//			"moid=" + (int)iu + " AND del=0");
		//	}
		//	return nn;
		//}

        //only Server0 uses this.
        public void DBSelect(IUx eIU, Difficulty d)
        {
            //items.Clear();
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
            //        string[] cleanData = new string[MultiChoiceItem.N_OPTIONS + 1];
            //        cleanData[0] = reader.GetString(2);
            //        for (int i = 1; i <= MultiChoiceItem.N_OPTIONS; ++i)
            //            cleanData[i] = reader.GetString(2 + i);
            //        string keyStr = reader.GetString(7);
            //        bool[] keys = new bool[4];
            //        for (int i = 0; i < 4; ++i)
            //            keys[i] = (keyStr[i] == MultiChoiceItem.C1);
            //        items.Add(new MultiChoiceItem(itemID, cleanData, keys, isDifficult));
            //    }
            //    reader.Close();
            //}
            throw new NotImplementedException();
        }

        //only Server0 uses this.
        public bool DBSelect(Random rand, IUx iu, int n, Difficulty d)
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
            //        string[] cleanData = new string[MultiChoiceItem.N_OPTIONS + 1];
            //        cleanData[0] = reader.GetString(2);
            //        for (int j = 1; j <= MultiChoiceItem.N_OPTIONS; ++j)
            //            cleanData[j] = reader.GetString(2 + j);
            //        string keyStr = reader.GetString(7);
            //        bool[] keys = new bool[4];
            //        for (int j = 0; j < 4; ++j)
            //            keys[j] = (keyStr[j] == MultiChoiceItem.C1);
            //        items.Add(new MultiChoiceItem(itemID, cleanData, keys, isDifficult));
            //    }
            //    reader.Close();
            //}
            throw new NotImplementedException();
        }

        public void DBIns(IUx eIU)
        {
            StringBuilder vals = new StringBuilder();
            foreach (MultiChoiceItem q in Questions)
            {
                vals.Append("(" + (int)eIU + ",0," + (q.IsDifficult ? 1 : 0) + ",'");
                vals.Append(q.Stem.Replace("'", "\\'") + "','");
                for (int i = 0; i < MultiChoiceItem.N_OPTIONS; ++i)
                    vals.Append(q.Options[i].Replace("'", "\\'") + "','");
                for (int i = 0; i < MultiChoiceItem.N_OPTIONS; ++i)
                    if (q.Keys[i])
                        vals.Append(MultiChoiceItem.C1);
                    else
                        vals.Append(MultiChoiceItem.C0);
                vals.Append("'),");
            }
            vals.Remove(vals.Length - 1, 1);//remove the last comma
            DBConnect.Ins("sqz_question", "moid,del,diff,Stem,ans0,ans1,ans2,ans3,`key`",
                vals.ToString());
        }

        public bool DBSelect(DateTime dt, Level lv, int id)
        {
            //items.Clear();
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
            //QIdxComparer<MultiChoiceItem> qComparer = new QIdxComparer<MultiChoiceItem>();
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
            //        MultiChoiceItem q = new MultiChoiceItem();
            //        q.uId = qid;
            //        q.IsDifficult = reader.GetInt16(0) != 0;
            //        q.GetStem = reader.GetString(1);
            //        string[] anss = new string[MultiChoiceItem.N_OPTIONS];
            //        for (int j = 0; j < MultiChoiceItem.N_OPTIONS; ++j)
            //            anss[j] = reader.GetString(2 + j);
            //        string x = reader.GetString(6);
            //        bool[] keys = new bool[MultiChoiceItem.N_OPTIONS];
            //        for (int j = 0; j < MultiChoiceItem.N_OPTIONS; ++j)
            //            keys[j] = (x[j] == MultiChoiceItem.C1);
            //        q.vAns = new string[4];
            //        q.Keys = new bool[4];
            //        for(int j = 0; j < 4; ++j)
            //        {
            //            q.vAns[j] = anss[asorts[i][j] - MultiChoiceItem.C0];
            //            q.Keys[j] = keys[asorts[i][j] - MultiChoiceItem.C0];
            //        }
            //        int iu;
            //        if (Enum.IsDefined(typeof(IUx), iu = reader.GetInt32(7)))
            //            q.eIU = (IUx)iu;
            //        items.Add(q);
            //    }
            //    reader.Close();
            //}
            //items.Sort(qComparer);
            throw new NotImplementedException();
        }

        public bool UpdateCurQSId()
        {
            if (Lv == Level.A && -1 < guDBCurAId)
            {
                uId = ++guDBCurAId;
                return false;
            }
            if (Lv == Level.B && -1 < guDBCurBId)
            {
                uId = ++guDBCurBId;
                return false;
            }
            return true;
        }

        public static bool DBUpdateCurQSId(DateTime dt)
        {
            //MySqlConnection conn = DBConnect.Init();
            //if (conn == null)
            //    return true;
            //int uid = DBConnect.MaxInt(conn, "sqz_qsheet", "id",
            //        "dt='" + dt.ToString(DT._) + "' AND lv='" + Level.A.ToString() + "'");
            //if (uid < 0)
            //{
            //    DBConnect.Close(ref conn);
            //    return true;
            //}
            //guDBCurAId = uid;

            //uid = DBConnect.MaxInt(conn, "sqz_qsheet", "id",
            //        "dt='" + dt.ToString(DT._) + "' AND lv='" + Level.B.ToString() + "'");
            //if (uid < 0)
            //{
            //    DBConnect.Close(ref conn);
            //    return true;
            //}
            //guDBCurBId = uid;

            //return false;
            throw new NotImplementedException();
        }
    }

    //public class QIdxComparer<T> : Comparer<T>
    //{
    //    SortedList<int, int> vIdx;

    //    public QIdxComparer()
    //    {
    //        vIdx = new SortedList<int, int>();
    //    }

    //    public void Add(int qid, int idx)
    //    {
    //        vIdx.Add(qid, idx);
    //    }

    //    public override int Compare(T x, T y)
    //    {
    //        MultiChoiceItem qx = x as MultiChoiceItem;
    //        if (qx == null)
    //            return 0;
    //        MultiChoiceItem qy = y as MultiChoiceItem;
    //        if (qy == null)
    //            return 0;
    //        if (vIdx[qx.uId] < vIdx[qy.uId])
    //            return -1;
    //        else if (vIdx[qx.uId] == vIdx[qy.uId])
    //            return 0;
    //        return 1;

    //    }
    //}
}
