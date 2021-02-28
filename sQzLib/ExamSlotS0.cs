using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Windows;

namespace sQzLib
{
    public class ExamSlotS0: ExamSlotA
    {
        public Dictionary<int, ExamRoomS0> Rooms;

        public ExamSlotS0()
        {
            Rooms = new Dictionary<int, ExamRoomS0>();
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
            List<int> rids = ExamRoomS0.DBSel(out emsg);
            if (rids == null)
                return emsg;
            foreach (int i in rids)
            {
                ExamRoomS0 r = new ExamRoomS0();
                r.uId = i;
                r.DBSelTimeAndPw(mDt, out emsg);
                if (!Rooms.ContainsKey(i))
                    Rooms.Add(i, r);
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
                while (0 < n)
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
            foreach (ExamRoomS0 r in Rooms.Values)
                if (0 < r.Examinees.Count && r.t2.Hour == DT.INV)
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
            if (0 < n)
                return null;
            return emsg;
        }

        public string ReadF(string fp, ref ExamSlotS0 o)
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
                    foreach (ExamRoomS0 ro in Rooms.Values)
                        if (ro.Examinees.ContainsKey(e.ID))
                        {
                            dup.Append(e.ID + ", ");
                            bCont = true;
                        }
                    if (bCont)
                        continue;
                    foreach (ExamRoomS0 ro in o.Rooms.Values)
                        if (ro.Examinees.ContainsKey(e.ID))
                        {
                            dup.Append(e.ID + ", ");
                            bCont = true;
                        }
                    if (bCont)
                        continue;
                    int roomID = -1;
                    if (!int.TryParse(v[1], out roomID) || !Rooms.ContainsKey(roomID))
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
                    o.Rooms[roomID].Examinees.Add(e.ID, e);
                }
                else
                    errorLines.Append(i.ToString() + ", ");
            }
            StringBuilder r = new StringBuilder();
            if (0 < dup.Length)
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
            string vch = ExamRoomS0.PwChars();
            Random rand = new Random();
            int v = 1;
            StringBuilder sb = new StringBuilder();
            foreach (ExamRoomS0 r in Rooms.Values)
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
                        "'," + r.uId + ",'" + ExamRoomS0.GenPw(vch, rand) + "')", out eMsg);
                if (n < 0)
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
            foreach (ExamRoomS0 r in Rooms.Values)
                r.Examinees.Clear();
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
            foreach (ExamRoomS0 r in Rooms.Values)
                r.DBSelNee(conn, mDt);
            DBConnect.Close(ref conn);
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
            ExamRoomS0 r;
            if (Rooms.TryGetValue(rid, out r))
                r.DBUpdateRs(vals);
        }

        public bool DBUpT1(int rid,
            out string eMsg)
        {
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
            {
                eMsg = Txt.s._((int)TxI.DB_NOK);
                return true;
            }
            ExamRoomS0 r;
            if (Rooms.TryGetValue(rid, out r))
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
            ExamRoomS0 r;
            if (Rooms.TryGetValue(rid, out r))
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
            if (QuestSheet.GetMaxID_inDB(mDt) &&
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

        public byte[] GetBytes_QPacksWithDateTime(int rid)
        {
            List<byte[]> l = new List<byte[]>();
            l.Add(DT.GetBytes(mDt));
            l.InsertRange(l.Count, QuestionPack.ToByte());

            return Utils.ToArray_FromListOfBytes(l);
        }

        public int CountQSByRoom()
        {
            int n = 0;
            foreach (ExamRoomS0 r in Rooms.Values)
                if (n < r.Examinees.Count)
                    n = r.Examinees.Count;
            return n;
        }

        private int ReadBytes_RoomFromS1(byte[] buf, ref int offs)
        {
            if (buf.Length - offs < 4)
                return -1;

            if (buf.Length - offs < 4)
                return -1;
            int rid = BitConverter.ToInt32(buf, offs);
            offs += 4;
            ExamRoomS0 r;
            if (!Rooms.TryGetValue(rid, out r) ||
                r.ReadBytes_FromS1(buf, ref offs))
                return -1;
            return rid;
        }

        public byte[] GetBytes_KeysWithDateTime()
        {
            List<byte[]> l = mKeyPack.ToByte();
            l.Insert(0, DT.GetBytes(mDt));
            return Utils.ToArray_FromListOfBytes(l);
        }

        public int ReadBytes_FromS1(byte[] buf, ref int offs)
        {
            if (Dt != DT.ReadByte(buf, ref offs))
                return -1;
            int rid = ReadBytes_RoomFromS1(buf, ref offs);
            if (rid < 0)
                return -1;
            return rid;
        }

        public byte[] GetBytes_RoomSendingToS1(int rId)
        {
            List<byte[]> l = new List<byte[]>();
            l.Add(DT.GetBytes(Dt));
            ExamRoomS0 r;
            if (Rooms.TryGetValue(rId, out r))
                l.InsertRange(l.Count, r.GetBytes_SendingToS1());
            else
                l.Add(BitConverter.GetBytes(-1));//should raise error message box here

            return Utils.ToArray_FromListOfBytes(l);
        }
    }
}
