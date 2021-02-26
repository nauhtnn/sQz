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
        Oper,
        Arch
    }

    public class ExamSlot
    {
        public DateTime mDt;
        public QuestPack QuestionPack;

        public AnsPack mKeyPack;

        public Dictionary<int, ExamRoom> vRoom;
        public Dictionary<int, DateTime> vT1;
        public Dictionary<int, DateTime> vT2;
        public ExamStt eStt;

        public ExamSlot()
        {
            mDt = DT.INV_;
            vRoom = new Dictionary<int, ExamRoom>();
            eStt = ExamStt.Prep;
            QuestionPack = new QuestPack();

            mKeyPack = new AnsPack();
        }

        public int InsertSlot(out string eMsg)
        {
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
            {
                eMsg = Txt.s._((int)TxI.DB_NOK);
                return 0;
            }
            string v = "('" + mDt.ToString(DT._) + "'," + (int)ExamStt.Prep + ")";
            int n = DBConnect.Ins(conn, "sqz_slot", "dt,status", v, out eMsg);
            DBConnect.Close(ref conn);
            if (n == -1062)
                eMsg = Txt.s._((int)TxI.DB_EXCPT) + Txt.s._((int)TxI.SLOT_EXIST);
            return n;
        }

        public static List<DateTime> DBSelectSlotIDs(bool arch, out string eMsg)
        {
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
            {
                eMsg = Txt.s._((int)TxI.DB_NOK);
                return null;
            }
            string qry;
            if (arch)
                qry = "status=" + (int)ExamStt.Arch;
            else
                qry = "status!=" + (int)ExamStt.Arch;
            qry = DBConnect.mkQrySelect("sqz_slot", "dt", qry);
            MySqlDataReader reader = DBConnect.exeQrySelect(conn, qry, out eMsg);
            if (reader == null)
            {
                DBConnect.Close(ref conn);
                return null;
            }
            List<DateTime> r = new List<DateTime>();
            while (reader.Read())
            {
                string s = reader.GetString(0);
                DateTime dt;
                DT.To_(s, DT.SYSTEM_DT_FMT, out dt);
                r.Add(dt);
            }
            reader.Close();
            DBConnect.Close(ref conn);
            return r;
        }

        public string DBSelRoomId()
        {
            string emsg;
            List<int> rids = ExamRoom.DBSel(out emsg);
            if (rids == null)
                return emsg;
            foreach (int i in rids)
            {
                ExamRoom r = new ExamRoom();
                r.uId = i;
                r.DBSelTimeAndPw(mDt, out emsg);
                if(!vRoom.ContainsKey(i))
                    vRoom.Add(i, r);
            }
            return null;
        }

        public static List<bool> IsSttOper(List<DateTime> l)
        {
            List<bool> v = new List<bool>();
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
            {
                int n = l.Count;
                while(0 < n)
                {
                    --n;
                    v.Add(false);
                }
                return v;
            }
            foreach (DateTime dt in l)
            {
                string qry = DBConnect.mkQrySelect("sqz_slot", "status",
                    "dt='" + dt.ToString(DT._) + "'");
                string eMsg;
                MySqlDataReader reader = DBConnect.exeQrySelect(conn, qry, out eMsg);
                if (reader == null)
                {
                    v.Add(false);
                    continue;
                }
                int x;
                if (reader.Read())
                    if (Enum.IsDefined(typeof(ExamStt), x = reader.GetInt16(0)))
                        v.Add((ExamStt)x == ExamStt.Oper);
                reader.Close();
            }
            DBConnect.Close(ref conn);
            return v;
        }

        public string DBSelStt()
        {
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return Txt.s._((int)TxI.DB_NOK);
            string qry = DBConnect.mkQrySelect("sqz_slot", "status",
                "dt='" + mDt.ToString(DT._) + "'");
            string eMsg;
            MySqlDataReader reader = DBConnect.exeQrySelect(conn, qry, out eMsg);
            if (reader == null)
            {
                DBConnect.Close(ref conn);
                System.Windows.MessageBox.Show("DBSelStt error\n" + eMsg.ToString());
                return eMsg;
            }
            int x;
            if (reader.Read())
                if (Enum.IsDefined(typeof(ExamStt), x = reader.GetInt16(0)))
                    eStt = (ExamStt)x;
            reader.Close();
            DBConnect.Close(ref conn);
            return null;
        }

        public void DBSafeUpdateArchiveStatus()
        {
            bool bArch = true;
            foreach (ExamRoom r in vRoom.Values)
                if (0 < r.vExaminee.Count && r.t2.Hour == DT.INV)
                {
                    bArch = false;
                    break;
                }
            if (bArch)
            {
                eStt = ExamStt.Arch;
                DBUpStt();
            }
        }

        public string DBUpStt()
        {
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return Txt.s._((int)TxI.DB_NOK);
            string emsg;
            int n = DBConnect.Update(conn, "sqz_slot", "status=" + (int)eStt,
                "dt='" + mDt.ToString(DT._) + "'",
                out emsg);
            if(0 < n)
                return null;
            return emsg;
        }

        public DateTime Dt {
            get { return mDt; }
            set {
                mDt = value;
                QuestionPack.mDt = value;
            }
        }

        public string ReadF(string fp, ref ExamSlot o)
        {
            string[] vs = System.IO.File.ReadAllLines(fp);
            StringBuilder errorLines = new StringBuilder();
            StringBuilder dup = new StringBuilder();
            int i = 0;
            foreach (string s in vs)
            {
                ++i;
                ExamineeS0 e = new ExamineeS0();
                string[] v = s.Split('\t');
                if (v.Length == 5)
                {
                    if (v[0].Length < 1)
                    {
                        errorLines.Append(i.ToString() + ", ");
                        continue;
                    }
                    e.ID = v[0].Trim();
                    bool bCont = false;
                    foreach (ExamRoom ro in vRoom.Values)
                        if (ro.vExaminee.ContainsKey(e.ID))
                        {
                            dup.Append(e.ID + ", ");
                            bCont = true;
                        }
                    if (bCont)
                        continue;
                    foreach (ExamRoom ro in o.vRoom.Values)
                        if (ro.vExaminee.ContainsKey(e.ID))
                        {
                            dup.Append(e.ID + ", ");
                            bCont = true;
                        }
                    if (bCont)
                        continue;
                    int roomID = -1;
                    if (!int.TryParse(v[1], out roomID) || !vRoom.ContainsKey(roomID))
                    {
                        errorLines.Append(i.ToString() + ", ");
                        continue;
                    }
                    e.mDt = mDt;
                    e.Name = v[2].Trim();
                    e.Birthdate = v[3].Trim();
                    e.Birthplace = v[4].Trim();
                    if (e.Name.Length == 0 || e.Birthdate.Length == 0 || e.Birthplace.Length == 0)
                    {
                        errorLines.Append(i.ToString() + ", ");
                        continue;
                    }
                    o.vRoom[roomID].vExaminee.Add(e.ID, e);
                }
                else
                    errorLines.Append(i.ToString() + ", ");
            }
            StringBuilder r = new StringBuilder();
            if(0 < dup.Length)
            {
                dup.Remove(dup.Length - 2, 2);//remove the last comma
                r.Append("\n" + Txt.s._((int)TxI.NEE_ID_EXIST));
                r.Append(dup.ToString() + '.');
            }
            if (0 < errorLines.Length)
            {
                errorLines.Remove(errorLines.Length - 2, 2);//remove the last comma
                r.Append("\n" + Txt.s._((int)TxI.NEE_ELINE));
                r.Append(errorLines.ToString() + '.');
            }
            if (r.Length == 0)
                return null;
            else
                return Txt.s._((int)TxI.NEE_FERR) + r.ToString();
        }

        public int DBInsNee(out string eMsg)
        {
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
            {
                eMsg = Txt.s._((int)TxI.DB_NOK);
                return -1;
            }
            string vch = ExamRoom.PwChars();
            Random rand = new Random();
            int v = 1;
            StringBuilder sb = new StringBuilder();
            foreach (ExamRoom r in vRoom.Values)
            {
                int n = 0;
                bool bNExist = DBConnect.NExist(conn, "sqz_slot_room",
                    "dt='" + mDt.ToString(DT._) +
                    "' AND rid=" + r.uId, out eMsg);
                if (eMsg != null)
                {
                    DBConnect.Close(ref conn);
                    return -1;
                }
                else if (bNExist)
                    n = DBConnect.Ins(conn, "sqz_slot_room",
                        "dt,rid,pw", "('" + mDt.ToString(DT._) +
                        "'," + r.uId + ",'" + ExamRoom.GenPw(vch, rand) + "')", out eMsg);
                if(n < 0)
                {
                    DBConnect.Close(ref conn);
                    return n;
                }
                n = r.DBIns(conn, out eMsg);
                if (n < 0)
                {
                    sb.AppendFormat(Txt.s._((int)TxI.ROOM_DB_NOK) + '\n', r.uId,
                        Txt.s._((int)TxI.NEE_EXIST));
                    v = 0;
                }
            }
            eMsg = sb.ToString();
            DBConnect.Close(ref conn);
            return v;
        }

        public void DelNee()
        {
            foreach (ExamRoom r in vRoom.Values)
                r.vExaminee.Clear();
        }

        public string DBDelNee()
        {
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return Txt.s._((int)TxI.DB_NOK);
            StringBuilder sb = new StringBuilder();
            string eMsg;
            int n = DBConnect.Count(conn, "sqz_nee_qsheet AS a,sqz_examinee AS b",
                "a.dt", "a.dt='" + mDt.ToString(DT._) +
                "' AND a.dt=b.dt AND a.neeid=b.id",
                out eMsg);
            sb.AppendFormat(Txt.s._((int)TxI.SLOT), mDt.ToString(DT._));// DT.hh
            if (0 < n)
            {
                sb.Append(Txt.s._((int)TxI.SLOT_DEL_GRD) + '\n');
                DBConnect.Close(ref conn);
                return sb.ToString();
            }
            else if (n < 0)
            {
                sb.Append(Txt.s._((int)TxI.SLOT_DEL_ECPT) + eMsg);
                DBConnect.Close(ref conn);
                return sb.ToString();
            }
            n = DBConnect.Delete(conn, "sqz_examinee",
                "dt='" + mDt.ToString(DT._) + "'", out eMsg);
            if (n < 0)
                sb.Append(Txt.s._((int)TxI.SLOT_DEL_ECPT) + eMsg);
            else
                sb.AppendFormat(Txt.s._((int)TxI.SLOT_DEL_N), n.ToString());
            DBConnect.Close(ref conn);
            return sb.ToString();
        }

        public void DBSelNee()
        {
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return;
            foreach (ExamRoom r in vRoom.Values)
                r.DBSelNee(conn, mDt);
            DBConnect.Close(ref conn);
        }

        public int CountQSByRoom()
        {
            int n = 0;
            foreach (ExamRoom r in vRoom.Values)
                if (n < r.vExaminee.Count)
                    n = r.vExaminee.Count;
            return n;
        }

        public int ReadByteR0(byte[] buf, ref int offs)
        {
            if (buf.Length - offs < 4)
                return -1;

            DateTime dt;
            if (DT.ReadByte(buf, ref offs, out dt) || dt != mDt)
                return -1;

            if (buf.Length - offs < 4)
                return -1;
            int rid = BitConverter.ToInt32(buf, offs);
            offs += 4;
            ExamRoom r;
            if (!vRoom.TryGetValue(rid, out r) ||
                r.ReadByte0(buf, ref offs))
                return -1;
            return rid;
        }

        public int ReadByteSl0(byte[] buf, ref int offs)
        {
            DateTime dt;
            if (DT.ReadByte(buf, ref offs, out dt) || dt != mDt)
                return -1;
            int rid = ReadByteR0(buf, ref offs);
            if (rid < 0)
                return -1;
            return rid;
        }

        public byte[] GetBytes_S0SendingToS1(int rId)
        {
            List<byte[]> l = new List<byte[]>();
            ExamRoom r;
            if (vRoom.TryGetValue(rId, out r))
                l.InsertRange(l.Count, r.GetBytes_S0SendingToS1());
            else
                l.Add(BitConverter.GetBytes(-1));//should raise error message box here

            int sz = sizeof(long);
            foreach (byte[] x in l)
                sz += x.Length;
            byte[] buf = new byte[sz];
            sz = 0;
            DT.CopyBytesToBuffer(buf, ref sz, mDt);
            foreach (byte[] x in l)
            {
                Buffer.BlockCopy(x, 0, buf, sz, x.Length);
                sz += x.Length;
            }
            return buf;
        }

        public bool ReadBytes_S1RecevingFromS0(byte[] buf, ref int offs)
        {
            if (DT.ReadByte(buf, ref offs, out mDt))
                return true;
            if (buf.Length - offs < 4)
                return true;
            int rId;
            if ((rId = BitConverter.ToInt32(buf, offs)) < 0)
                return true;
            offs += 4;
            ExamRoom r;
            if (vRoom.TryGetValue(rId, out r))
            {
                if (r.ReadBytes_S1ReceivingFromS0(buf, ref offs))
                    return true;
            }
            else
            {
                r = new ExamRoom();
                r.uId = rId;
                if (r.ReadBytes_S1ReceivingFromS0(buf, ref offs))
                    return true;
                vRoom.Add(rId, r);
            }
            return false;
        }

        public bool DBUpdateRs(int rid, out string eMsg)
        {
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
            {
                eMsg = Txt.s._((int)TxI.DB_NOK);
                return true;
            }
            StringBuilder vals = new StringBuilder();

            DBUpdateRs(rid, vals);

            bool rval;
            if (0 < vals.Length)
            {
                vals.Remove(vals.Length - 1, 1);//remove the last comma
                int rs;
                rval = (rs = DBConnect.Ins(conn, "sqz_nee_qsheet",
                    "dt,lv,neeid,qsid,t1,t2,grade,comp,ans", vals.ToString(), out eMsg)) < 0;
                if (rs == DBConnect.PRI_KEY_EXISTS)
                    eMsg = Txt.s._((int)TxI.RS_UP_EXISTS);
            }
            else
            {
                rval = true;
                eMsg = Txt.s._((int)TxI.RS_UP_NOTHING);
            }
            if (!rval)
            {
                DBUpT2(conn, rid, out eMsg);
            }
            DBConnect.Close(ref conn);
            return rval;
        }

        public void DBUpdateRs(int rid, StringBuilder vals)
        {
            ExamRoom r;
            if(vRoom.TryGetValue(rid, out r))
                r.DBUpdateRs(vals);
        }

        public bool DBUpT1(int rid,
            out string eMsg)
        {
            MySqlConnection conn = DBConnect.Init();
            if(conn == null)
            {
                eMsg = Txt.s._((int)TxI.DB_NOK);
                return true;
            }
            ExamRoom r;
            if (vRoom.TryGetValue(rid, out r))
            {
                r.t1 = DateTime.Now;
                bool rv = r.DBUpT1(conn, mDt, out eMsg);
                DBConnect.Close(ref conn);
                return rv;
            }
            eMsg = "Room id " + rid + " is not found";
            return true;
        }

        public bool DBUpT2(MySqlConnection conn, int rid,
            out string eMsg)
        {
            ExamRoom r;
            if (vRoom.TryGetValue(rid, out r))
            {
                r.t2 = DateTime.Now;
                return r.DBUpT2(conn, mDt, out eMsg);
            }
            eMsg = "Room id " + rid + " is not found";
            return true;
        }

        public bool GenQ(int n)
        {
            string emsg;
            if (QuestionPack.DBDelete(out emsg))
                WPopup.s.ShowDialog(emsg);
            QuestionPack.vSheet.Clear();
            if(QuestSheet.GetMaxID_inDB(mDt) &&
                MessageBox.Show("Cannot get QuestSheet.GetMaxID_inDB. Choose Yes to continue and get risky.", "Warning!", MessageBoxButton.YesNo) == MessageBoxResult.No)
                return true;
            foreach (QuestSheet qs in QuestionPack.vSheet.Values)
                mKeyPack.vSheet.Remove(qs.ID);
            List<QuestSheet> sheets;
            sheets = QuestionPack.GenQPack3(n);
            mKeyPack.ExtractKey(sheets);
            return false;
        }

        public bool DBSelArchieve(out string eMsg)
        {
            if (QuestionPack.DBSelectQS(mDt, out eMsg))
                return true;
            foreach (QuestSheet qs in QuestionPack.vSheet.Values)
                mKeyPack.ExtractKey(qs);
            return false;
        }

        public byte[] ToByteQPack(int rid)
        {
            List<byte[]> l = new List<byte[]>();
            l.Add(DT.GetBytes(mDt));
            l.InsertRange(l.Count, QuestionPack.ToByte());

            return ListOfBytes_ToArray(l);
        }

        private byte[] ListOfBytes_ToArray(List<byte[]> l)
        {
            int sz = 0;
            foreach (byte[] x in l)
                sz += x.Length;
            byte[] buf = new byte[sz];
            sz = 0;
            foreach (byte[] x in l)
            {
                Buffer.BlockCopy(x, 0, buf, sz, x.Length);
                sz += x.Length;
            }
            return buf;
        }

        public bool ReadByteQPack(byte[] buf, ref int offs)
        {
            if (QuestionPack.ReadByte(buf, ref offs))
                return true;
            return false;
        }

        public byte[] ToByteNextQS()
        {
            return QuestionPack.ToByteNextQS();
        }

        public byte[] ToByteKey()
        {
            List<byte[]> l = mKeyPack.ToByte();
            l.Insert(0, DT.GetBytes(mDt));
            return ListOfBytes_ToArray(l);
        }

        public bool ReadByteKey(byte[] buf, ref int offs)
        {
            return mKeyPack.ReadByte(buf, ref offs);
        }

        public ExamineeA Signin(ExamineeA e)
        {
            ExamineeA o;
            foreach (ExamRoom r in vRoom.Values)
                if ((o = r.Signin(e)) != null)
                    return o;
            return null;
        }

        public ExamineeA Find(string neeID)
        {
            ExamineeA o;
            foreach (ExamRoom r in vRoom.Values)
                if (r.vExaminee.TryGetValue(neeID, out o))
                    return o;
            return null;
        }

        public List<byte[]> ToByteR0()
        {
            List<byte[]> l = new List<byte[]>();
            l.Add(DT.GetBytes(mDt));
            if(vRoom.Values.Count == 1)//either 0 or 1
            {
                foreach(ExamRoom r in vRoom.Values)
                    l.InsertRange(l.Count, r.ToByte0());
            }
            return l;
        }

        public void WriteTxt()
        {
            //string folderToStore = mDt.ToString(DT._);
            //if (!System.IO.Directory.Exists(folderToStore))
            //    System.IO.Directory.CreateDirectory(folderToStore);
            //System.IO.Directory.SetCurrentDirectory(System.IO.Directory.GetCurrentDirectory() + "\\" +
            //    folderToStore);
            //foreach (QuestPack p in vQPack.Values)
            //    p.WriteDocx();
            //System.IO.Directory.SetCurrentDirectory(System.IO.Directory.GetParent(
            //    System.IO.Directory.GetCurrentDirectory()).ToString());
        }
    }
}
