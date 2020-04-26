using System;
using System.Collections.Generic;
using System.Windows.Controls;
using MySql.Data.MySqlClient;
using System.Windows.Media;
using System.Windows;
using System.Text;

namespace sQzLib
{
    public enum ExamStt
    {
        Prep = 0,
        Doing,
        Archive
    }

    public class ExamSlot
    {
        public DateTime mDt;
        public Dictionary<Level, QuestPack> vQPack;

        public AnsPack mKeyPack;

        public Dictionary<int, ExamRoom> Rooms;
        public Dictionary<int, DateTime> vT1;
        public Dictionary<int, DateTime> vT2;
        public ExamStt eStt;

        public ExamSlot()
        {
            mDt = DT.INV_;
            Rooms = new Dictionary<int, ExamRoom>();
            eStt = ExamStt.Prep;
            vQPack = new Dictionary<Level, QuestPack>();
            QuestPack p = new QuestPack(false);
            p.eLv = Level.A;
            vQPack.Add(p.eLv, p);
            p = new QuestPack(false);
            p.eLv = Level.B;
            vQPack.Add(p.eLv, p);

            mKeyPack = new AnsPack();
        }

        public void DBSelectRoomID()
        {
            List<int> rids = ExamRoom.DBSelectRoomIDs();
            foreach (int i in rids)
            {
                ExamRoom r = new ExamRoom();
                r.ID = i;
                r.DBSelTimeAndPw(mDt);
                if(!Rooms.ContainsKey(i))
                    Rooms.Add(i, r);
            }
        }

        public static List<bool> IsDoing(List<DateTime> l)
        {
            throw new NotImplementedException();
            //List<bool> v = new List<bool>();
            //MySqlConnection conn = DBConnect.Init();
            //if (conn == null)
            //{
            //    int n = l.Count;
            //    while(0 < n)
            //    {
            //        --n;
            //        v.Add(false);
            //    }
            //    return v;
            //}
            //foreach (DateTime dt in l)
            //{
            //    string qry = DBConnect.mkQrySelect("sqz_slot", "stt",
            //        "dt='" + dt.ToString(DT._) + "' AND t='" + dt.ToString(DT.hh) + "'");
            //    string eMsg;
            //    MySqlDataReader reader = DBConnect.exeQrySelect(conn, qry, out eMsg);
            //    if (reader == null)
            //    {
            //        v.Add(false);
            //        continue;
            //    }
            //    int x;
            //    if (reader.Read())
            //        if (Enum.IsDefined(typeof(ExamStt), x = reader.GetInt16(0)))
            //            v.Add((ExamStt)x == ExamStt.Doing);
            //    reader.Close();
            //}
            //DBConnect.Close(ref conn);
            //return v;
        }

        public string DBSelStt()
        {
            throw new NotImplementedException();
            //MySqlConnection conn = DBConnect.Init();
            //if (conn == null)
            //    return Txt.s._[(int)TxI.DB_NOK];
            //string qry = DBConnect.mkQrySelect("sqz_slot", "stt",
            //    "dt='" + mDt.ToString(DT._) + "' AND t='" + mDt.ToString(DT.hh) + "'");
            //string eMsg;
            //MySqlDataReader reader = DBConnect.exeQrySelect(conn, qry, out eMsg);
            //if (reader == null)
            //{
            //    DBConnect.Close(ref conn);
            //    return eMsg;
            //}
            //int x;
            //if (reader.Read())
            //    if (Enum.IsDefined(typeof(ExamStt), x = reader.GetInt16(0)))
            //        eStt = (ExamStt)x;
            //reader.Close();
            //DBConnect.Close(ref conn);
            //return null;
        }

        public string DBUpStt()
        {
            throw new NotImplementedException();
            //MySqlConnection conn = DBConnect.Init();
            //if (conn == null)
            //    return Txt.s._[(int)TxI.DB_NOK];
            //string emsg;
            //int n = DBConnect.Update(conn, "sqz_slot", "stt=" + (int)eStt,
            //    "dt='" + mDt.ToString(DT._) + "' AND t='" + mDt.ToString(DT.hh) + "'",
            //    out emsg);
            //if(0 < n)
            //    return null;
            //return emsg;
        }

        public DateTime Dt {
            get { return mDt; }
            set {
                mDt = value;
                foreach (QuestPack p in vQPack.Values)
                    p.mDt = value;
            }
        }

        public string ReadF(string fp, ref ExamSlot o)
        {
            throw new NotImplementedException();
            //string buf = Utils.ReadFile(fp);
            //if (buf == null)
            //    return null;
            //string[] vs = buf.Split('\n');
            //StringBuilder eline = new StringBuilder();
            //StringBuilder dup = new StringBuilder();
            //int i = 0;
            //foreach (string s in vs)
            //{
            //    ++i;
            //    ExamineeS0 e = new ExamineeS0();
            //    string[] v = s.Split('\t');
            //    if (v.Length == 5)
            //    {
            //        if (v[0].Length < 2)
            //        {
            //            eline.Append(i.ToString() + ", ");
            //            continue;
            //        }
            //        v[0] = v[0].ToUpper();
            //        if(!Enum.TryParse(v[0].Substring(0, 1), out e.eLv))
            //        {
            //            eline.Append(i.ToString() + ", ");
            //            continue;
            //        }
            //        int urid;
            //        if (!int.TryParse(v[0].Substring(1), out e.uId)
            //            || !int.TryParse(v[1], out urid) || !Rooms.ContainsKey(urid))
            //        {
            //            eline.Append(i.ToString() + ", ");
            //            continue;
            //        }
            //        bool bCont = false;
            //        foreach(ExamRoom ro in Rooms.Values)
            //            if(ro.vExaminee.ContainsKey(e.LvId))
            //            {
            //                dup.Append(e.eLv.ToString() + e.uId + ", ");
            //                bCont = true;
            //            }
            //        if (bCont)
            //            continue;
            //        foreach (ExamRoom ro in o.Rooms.Values)
            //            if (ro.vExaminee.ContainsKey(e.LvId))
            //            {
            //                dup.Append(e.eLv.ToString() + e.uId + ", ");
            //                bCont = true;
            //            }
            //        if (bCont)
            //            continue;
            //        e.mDt = mDt;
            //        e.tName = v[2].Trim();
            //        DateTime dt;
            //        if(DT.To_(v[3], DT.RR, out dt))
            //        {
            //            eline.Append(i.ToString() + ", ");
            //            continue;
            //        }
            //        e.tBirdate = v[3];
            //        e.tBirthplace = v[4].Trim();
            //        if (e.tName.Length == 0 || e.tBirdate.Length == 0 || e.tBirthplace.Length == 0)
            //        {
            //            eline.Append(i.ToString() + ", ");
            //            continue;
            //        }
            //        o.Rooms[urid].vExaminee.Add(e.LvId, e);
            //    }
            //    else
            //        eline.Append(i.ToString() + ", ");
            //}
            //StringBuilder r = new StringBuilder();
            //if(0 < dup.Length)
            //{
            //    dup.Remove(dup.Length - 2, 2);//remove the last comma
            //    r.Append("\n" + Txt.s._[(int)TxI.NEE_ID_EXIST]);
            //    r.Append(dup.ToString() + '.');
            //}
            //if (0 < eline.Length)
            //{
            //    eline.Remove(eline.Length - 2, 2);//remove the last comma
            //    r.Append("\n" + Txt.s._[(int)TxI.NEE_ELINE]);
            //    r.Append(eline.ToString() + '.');
            //}
            //if (r.Length == 0)
            //    return null;
            //else
            //    return Txt.s._[(int)TxI.NEE_FERR] + r.ToString();
        }

        public int DBInsRoom_and_Examinee()
        {
            Random rand = new Random();
            int v = 1;
            StringBuilder sb = new StringBuilder();
            foreach (ExamRoom r in Rooms.Values)
            {
                int n = 0;
                bool isExist = DBConnect.IsExist("sqz_slot_room",
                    "dt='" + mDt.ToString(DT._) + "' AND t='" + mDt.ToString(DT.hh) +
                    "' AND rid=" + r.ID);
                if (!isExist)
                    DBConnect.Ins("sqz_slot_room",
                        "dt,t,rid,pw,qpkalt", "('" + mDt.ToString(DT._) + "','" + mDt.ToString(DT.hh) +
                        "'," + r.ID + ",'" + ExamRoom.GeneratePw(rand) + "',0)");
                n = r.SaveExaminees();
                if (n < 0)
                {
                    sb.AppendFormat(Txt.s._[(int)TxI.ROOM_DB_NOK] + '\n', r.ID + 1,
                        Txt.s._[(int)TxI.NEE_EXIST]);
                    v = 0;
                }
            }
            return v;
        }

        public void DelNee()
        {
            foreach (ExamRoom r in Rooms.Values)
            {
                r.vExaminee.Clear();
                r.N_ExamineeGroupByLv[Level.A] = r.N_ExamineeGroupByLv[Level.B] = 0;
            }
        }

        public string DBDeleleUnfinishedExaminee()
        {
            StringBuilder sb = new StringBuilder();
            int n = DBConnect.Count("sqz_nee_qsheet AS a,sqz_examinee AS b",
                "a.dt", "a.dt='" + mDt.ToString(DT._) +
                "' AND t='" + mDt.ToString(DT.hh) + "' AND a.dt=b.dt AND a.neeid=b.id");
            sb.AppendFormat(Txt.s._[(int)TxI.SLOT], mDt.ToString(DT.hh));
            if (0 < n)
            {
                sb.Append(Txt.s._[(int)TxI.SLOT_DEL_GRD] + '\n');
                return sb.ToString();
            }
            else if (n < 0)
            {
                sb.Append(Txt.s._[(int)TxI.SLOT_DEL_ECPT]);
                return sb.ToString();
            }
            n = DBConnect.Delete("sqz_examinee",
                "dt='" + mDt.ToString(DT._) + "' AND t='" + mDt.ToString(DT.hh) + "'");
            if (n < 0)
                sb.Append(Txt.s._[(int)TxI.SLOT_DEL_ECPT]);
            else
                sb.AppendFormat(Txt.s._[(int)TxI.SLOT_DEL_N], n.ToString());
            return sb.ToString();
        }

        public void DBSelectExaminee()
        {
            foreach (ExamRoom r in Rooms.Values)
                r.LoadExaminees(mDt);
        }

        public int CountQSByRoom(Level lv)
        {
            int n = 0;
            foreach (ExamRoom r in Rooms.Values)
                if (n < r.N_ExamineeGroupByLv[lv])
                    n = r.N_ExamineeGroupByLv[lv];
            return n;
        }

        public int ReadByteR0(byte[] buf, ref int offs)
        {
            if (buf.Length - offs < 4)
                return -1;
            int rid = BitConverter.ToInt32(buf, offs);
            offs += 4;
            ExamRoom r;
            if (!Rooms.TryGetValue(rid, out r) ||
                r.ReadByte0(buf, ref offs))
                return -1;
            return rid;
        }

        public List<byte[]> ToByteR1(int rId)
        {
            List<byte[]> l = new List<byte[]>();
            l.Add(DT.ToByteh(mDt));
            ExamRoom r;
            if (Rooms.TryGetValue(rId, out r))
                l.InsertRange(l.Count, r.ToByte1());
            else
                l.Add(BitConverter.GetBytes(-1));
            return l;
        }

        public bool ReadByteR1(byte[] buf, ref int offs)
        {
            if (buf.Length - offs < 4)
                return true;
            int rId;
            if ((rId = BitConverter.ToInt32(buf, offs)) < 0)
                return true;
            offs += 4;
            ExamRoom r;
            if (Rooms.TryGetValue(rId, out r))
            {
                if (r.ReadByte1(buf, ref offs))
                    return true;
            }
            else
            {
                r = new ExamRoom();
                r.ID = rId;
                if (r.ReadByte1(buf, ref offs))
                    return true;
                Rooms.Add(rId, r);
            }
            return false;
        }

        public void DBUpdateRs(int rid, StringBuilder vals)
        {
            ExamRoom r;
            if(Rooms.TryGetValue(rid, out r))
                r.DBMakeInsResult(vals);
        }

        public bool SaveRetrieveTime(int rid)
        {
            ExamRoom r;
            if (Rooms.TryGetValue(rid, out r))
            {
                r.t1 = DateTime.Now;
                bool rv = r.SaveRetrieveTime(mDt);
                return rv;
            }
            return true;
        }

        public bool SaveCommitTime(int rid)
        {
            ExamRoom r;
            if (Rooms.TryGetValue(rid, out r))
            {
                r.t2 = DateTime.Now;
                return r.SaveCommitTime(mDt);
            }
            return true;
        }

        public bool GenQ(int n, Level lv, int[] vn, int[] vndiff)
        {
            string emsg;
            if (vQPack[lv].DBDelete(out emsg))
                WPopup.s.ShowDialog(emsg);
            vQPack[lv].Sheets.Clear();
            //if (vQPackAlt[lv].DBDelete(out emsg))
            //    WPopup.s.ShowDialog(emsg);
            //vQPackAlt[lv].vSheet.Clear();
            QuestSheet.DBUpdateCurQSId(mDt);
            foreach (QuestSheet qs in vQPack[lv].Sheets.Values)
                mKeyPack.vSheet.Remove(qs.LvId);
            List<QuestSheet> l;
            if(System.IO.File.Exists("Randomize.txt"))
            {
                l = vQPack[lv].GenQPack2(n, vn, vndiff);
                //l.InsertRange(l.Count, vQPackAlt[lv].GenQPack2(n, vn, vndiff));
            }
            else
            {
                l = vQPack[lv].GenQPack3(n, vn, vndiff);
                //l.InsertRange(l.Count, vQPackAlt[lv].GenQPack3(n, vn, vndiff));
            }
            mKeyPack.ExtractKey(l);
            return false;
        }

        public bool DBSelArchieve(out string eMsg)
        {
            if (vQPack[Level.A].DBSelectQS(mDt, out eMsg))
                return true;
            if (vQPack[Level.B].DBSelectQS(mDt, out eMsg))
                return true;
            if (vQPackAlt[Level.A].DBSelectQS(mDt, out eMsg))
                return true;
            if (vQPackAlt[Level.B].DBSelectQS(mDt, out eMsg))
                return true;
            foreach (QuestPack p in vQPack.Values)
                foreach (QuestSheet qs in p.Sheets.Values)
                    mKeyPack.ExtractKey(qs);
            foreach (QuestPack p in vQPackAlt.Values)
                foreach (QuestSheet qs in p.Sheets.Values)
                    mKeyPack.ExtractKey(qs);
            return false;
        }

        public List<byte[]> ToByteQPack(int rid)
        {
            List<byte[]> l = new List<byte[]>();
            l.Add(DT.ToByteh(mDt));
            if (vbQPkAlt.ContainsKey(rid))
                l.Add(BitConverter.GetBytes(vbQPkAlt[rid]));
            else
                l.Add(BitConverter.GetBytes(false));
            l.Add(BitConverter.GetBytes(vQPack.Count));
            foreach (QuestPack p in vQPack.Values)
                l.InsertRange(l.Count, p.ToByte());
            l.Add(BitConverter.GetBytes(vQPackAlt.Count));
            foreach (QuestPack p in vQPackAlt.Values)
                l.InsertRange(l.Count, p.ToByte());
            return l;
        }

        public bool ReadByteQPack(byte[] buf, ref int offs)
        {
            if (buf.Length - offs < 5)
                return true;
            bQPkAlt = BitConverter.ToBoolean(buf, offs);
            ++offs;
            int n = BitConverter.ToInt32(buf, offs);
            offs += 4;
            int l = buf.Length - offs;
            while (0 < n)
            {
                if (l < 4)
                    return true;
                --n;
                int x;
                Level lv;
                if (Enum.IsDefined(typeof(Level), x = BitConverter.ToInt32(buf, offs)))
                    lv = (Level)x;
                else
                    return true;
                l -= 4;
                offs += 4;
                if (vQPack[lv].ReadByte(buf, ref offs))
                    return true;
            }
            if (n != 0)
                return true;
            //
            if (buf.Length - offs < 4)
                return true;
            n = BitConverter.ToInt32(buf, offs);
            offs += 4;
            l = buf.Length - offs;
            while (0 < n)
            {
                if (l < 4)
                    return true;
                --n;
                int x;
                Level lv;
                if (Enum.IsDefined(typeof(Level), x = BitConverter.ToInt32(buf, offs)))
                    lv = (Level)x;
                else
                    return true;
                offs += 4;
                if (vQPackAlt[lv].ReadByte(buf, ref offs))
                    return true;
            }
            if (n != 0)
                return true;
            return false;
        }

        public byte[] ToByteNextQS(Level lv)
        {
            if (bQPkAlt)
                return vQPackAlt[lv].ToByteNextQS();
            return vQPack[lv].ToByteNextQS();
        }

        public List<byte[]> ToByteKey()
        {
            List<byte[]> l = mKeyPack.ToByte();
            l.Insert(0, DT.ToByteh(mDt));
            return l;
        }

        public bool ReadByteKey(byte[] buf, ref int offs)
        {
            return mKeyPack.ReadByte(buf, ref offs);
        }

        public ExamineeA Signin(ExamineeA e)
        {
            ExamineeA o;
            foreach (ExamRoom r in Rooms.Values)
                if ((o = r.Signin(e)) != null)
                    return o;
            return null;
        }

        public ExamineeA Find(int lvid)
        {
            ExamineeA o;
            foreach (ExamRoom r in Rooms.Values)
                if (r.vExaminee.TryGetValue(lvid, out o))
                    return o;
            return null;
        }

        public List<byte[]> ToByteR0()
        {
            List<byte[]> l = new List<byte[]>();
            l.Add(DT.ToByteh(mDt));
            if(Rooms.Values.Count == 1)//either 0 or 1
            {
                foreach(ExamRoom r in Rooms.Values)
                    l.InsertRange(l.Count, r.ToByte0());
            }
            return l;
        }

        public void WriteTxt()
        {
            string folderToStore = mDt.ToString(DT.__);
            if (!System.IO.Directory.Exists(folderToStore))
                System.IO.Directory.CreateDirectory(folderToStore);
            System.IO.Directory.SetCurrentDirectory(System.IO.Directory.GetCurrentDirectory() + "\\" +
                folderToStore);
            foreach (QuestPack p in vQPack.Values)
                p.WriteDocx();
            System.IO.Directory.SetCurrentDirectory(System.IO.Directory.GetParent(
                System.IO.Directory.GetCurrentDirectory()).ToString());
        }
    }
}
