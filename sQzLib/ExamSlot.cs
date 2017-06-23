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
        public bool bOpen;
        public Dictionary<ExamLv, QuestPack> vQPack;

        public AnsPack mKeyPack;

        public Dictionary<int, ExamRoom> vRoom;

        public ExamSlot()
        {
            mDt = DT.INV_;
            vRoom = new Dictionary<int, ExamRoom>();
            vQPack = new Dictionary<ExamLv, QuestPack>();
            QuestPack p = new QuestPack();
            p.eLv = ExamLv.A;
            vQPack.Add(p.eLv, p);
            p = new QuestPack();
            p.eLv = ExamLv.B;
            vQPack.Add(p.eLv, p);

            mKeyPack = new AnsPack();
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
                if(!vRoom.ContainsKey(i))
                    vRoom.Add(i, r);
            }
            return null;
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
            string buf = Utils.ReadFile(fp);
            if (buf == null)
                return null;
            string[] vs = buf.Split('\n');
            StringBuilder eline = new StringBuilder();
            StringBuilder dup = new StringBuilder();
            int i = 0;
            foreach (string s in vs)
            {
                ++i;
                ExamineeS0 e = new ExamineeS0();
                string[] v = s.Split('\t');
                if (v.Length == 5)
                {
                    if (v[0].Length < 2)
                    {
                        eline.Append(i.ToString() + ", ");
                        continue;
                    }
                    v[0] = v[0].ToUpper();
                    if(!Enum.TryParse(v[0].Substring(0, 1), out e.eLv))
                    {
                        eline.Append(i.ToString() + ", ");
                        continue;
                    }
                    int urid;
                    if (!int.TryParse(v[0].Substring(1), out e.uId)
                        || !int.TryParse(v[1], out urid) || !vRoom.ContainsKey(urid))
                    {
                        eline.Append(i.ToString() + ", ");
                        continue;
                    }
                    if(vRoom[urid].vExaminee.ContainsKey(e.uId) || o.vRoom[urid].vExaminee.ContainsKey(e.uId))
                    {
                        dup.Append(e.eLv.ToString() + e.uId + ", ");
                        continue;
                    }
                    e.mDt = mDt;
                    e.tName = v[2].Trim();
                    DateTime dt;
                    if(!DT.To_(v[3], DT._, out dt))
                    {
                        eline.Append(i.ToString() + ", ");
                        continue;
                    }
                    e.tBirdate = v[3];
                    e.tBirthplace = v[4].Trim();
                    if (e.tName.Length == 0 || e.tBirdate.Length == 0 || e.tBirthplace.Length == 0)
                    {
                        eline.Append(i.ToString() + ", ");
                        continue;
                    }
                    o.vRoom[urid].vExaminee.Add(e.uId, e);
                }
                else
                    eline.Append(i.ToString() + ", ");
            }
            StringBuilder r = new StringBuilder();
            if(0 < dup.Length)
            {
                dup.Remove(dup.Length - 2, 2);//remove the last comma
                r.Append("\n" + Txt.s._[(int)TxI.NEE_ID_EXIST]);
                r.Append(dup.ToString() + '.');
            }
            if (0 < eline.Length)
            {
                eline.Remove(eline.Length - 2, 2);//remove the last comma
                r.Append("\n" + Txt.s._[(int)TxI.NEE_ELINE]);
                r.Append(eline.ToString() + '.');
            }
            if (r.Length == 0)
                return null;
            else
                return Txt.s._[(int)TxI.NEE_FERR] + r.ToString();
        }

        public int DBInsNee(out string eMsg)
        {
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
            {
                eMsg = Txt.s._[(int)TxI.DB_NOK];
                return -1;
            }
            int v = 1;
            StringBuilder sb = new StringBuilder();
            foreach (ExamRoom r in vRoom.Values)
            {
                int n = DBConnect.Count(conn, "sqz_slot_room", "dt",
                    "dt='" + mDt.ToString(DT._) + "' AND t='" + mDt.ToString(DT.hh) +
                    "' AND rid=" + r.uId, out eMsg);
                if (n < 0)
                {
                    DBConnect.Close(ref conn);
                    return n;
                }
                else if (n == 0)
                    n = DBConnect.Ins(conn, "sqz_slot_room",
                        "dt,t,rid", "('" + mDt.ToString(DT._) + "','" + mDt.ToString(DT.hh) +
                        "'," + r.uId + ")", out eMsg);
                if(n < 0)
                {
                    DBConnect.Close(ref conn);
                    return n;
                }
                n = r.DBIns(conn, out eMsg);
                if (n < 0)
                {
                    sb.AppendFormat(Txt.s._[(int)TxI.ROOM_DB_NOK] + '\n', r.uId + 1,
                        Txt.s._[(int)TxI.NEE_EXIST]);
                    v = 0;
                }
            }
            eMsg = sb.ToString();
            DBConnect.Close(ref conn);
            return v;
        }

        public void DelNee()
        {
            StringBuilder sb = new StringBuilder();
            foreach (ExamRoom r in vRoom.Values)
            {
                r.vExaminee.Clear();
                r.nLv[ExamLv.A] = r.nLv[ExamLv.B] = 0;
            }
        }

        public string DBDelNee()
        {
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return Txt.s._[(int)TxI.DB_NOK];
            StringBuilder sb = new StringBuilder();
            string eMsg;
            int n = DBConnect.Count(conn, "sqz_nee_qsheet AS a,sqz_examinee AS b",
                "a.dt", "a.dt='" + mDt.ToString(DT._) +
                "' AND t='" + mDt.ToString(DT.hh) + "' AND a.dt=b.dt AND a.neeid=b.id",
                out eMsg);
            sb.AppendFormat(Txt.s._[(int)TxI.SLOT], mDt.ToString(DT.hh));
            if (0 < n)
            {
                sb.Append(Txt.s._[(int)TxI.SLOT_DEL_GRD] + '\n');
                DBConnect.Close(ref conn);
                return sb.ToString();
            }
            else if (n < 0)
            {
                sb.Append(Txt.s._[(int)TxI.SLOT_DEL_ECPT] + eMsg);
                DBConnect.Close(ref conn);
                return sb.ToString();
            }
            n = DBConnect.Delete(conn, "sqz_examinee",
                "dt='" + mDt.ToString(DT._) + "' AND t='" + mDt.ToString(DT.hh) + "'", out eMsg);
            if (n < 0)
                sb.Append(Txt.s._[(int)TxI.SLOT_DEL_ECPT] + eMsg);
            else
                sb.AppendFormat(Txt.s._[(int)TxI.SLOT_DEL_N], n.ToString());
            DBConnect.Close(ref conn);
            return sb.ToString();
        }

        public void DBSelNee()
        {
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return;
            foreach(ExamRoom r in vRoom.Values)
            {
                r.vExaminee.Clear();
                r.nLv[ExamLv.A] = r.nLv[ExamLv.B] = 0;
                string qry = DBConnect.mkQrySelect("sqz_slot_room AS a,sqz_examinee AS b",
                    "lv,id,name,birdate,birthplace", "a.rid=" + r.uId +
                    " AND a.dt='" + mDt.ToString(DT._) + "' AND a.t='" + mDt.ToString(DT.hh) +
                    "' AND a.dt=b.dt AND a.t=b.t AND a.rid=b.rid");
                string emsg;
                MySqlDataReader reader = DBConnect.exeQrySelect(conn, qry, out emsg);
                if (reader != null)
                {
                    while (reader.Read())
                    {
                        ExamineeS0 e = new ExamineeS0();
                        e.mDt = Dt;
                        if (!Enum.TryParse(reader.GetString(0), out e.eLv))
                            continue;
                        e.uId = reader.GetUInt16(1);
                        e.tName = reader.GetString(2);
                        e.tBirdate = reader.GetDateTime(3).ToString(DT.RR);
                        e.tBirthplace = reader.GetString(4);
                        r.vExaminee.Add(e.uId, e);
                        ++r.nLv[e.eLv];
                    }
                    reader.Close();
                }
            }
            foreach(ExamRoom r in vRoom.Values)
            {
                foreach(ExamineeA e in r.vExaminee.Values)
                {
                    string qry = DBConnect.mkQrySelect("sqz_nee_qsheet",
                        "t1,t2,grade,comp", "dt='" + mDt.ToString(DT._) + "' AND lv='" +
                        e.eLv + "' AND neeid=" + e.uId);
                    string emsg;
                    MySqlDataReader reader = DBConnect.exeQrySelect(conn, qry, out emsg);
                    if (reader != null)
                    {
                        if (reader.Read())
                        {
                            if (DT.Toh(reader.GetString(0), DT.hs, out e.dtTim1))
                                break;
                            if (DT.Toh(reader.GetString(1), DT.hs, out e.dtTim1))
                                break;
                            e.uGrade = reader.GetInt16(2);
                            e.tComp = reader.GetString(3);
                        }
                        reader.Close();
                    }
                }
            }
            DBConnect.Close(ref conn);
        }

        public int CountQSByRoom(ExamLv lv)
        {
            int n = 0;
            foreach (ExamRoom r in vRoom.Values)
                if (n < r.nLv[lv])
                    n = r.nLv[lv];
            return n + (n / 10) + 1;
        }

        public bool ReadByteR0(byte[] buf, ref int offs)
        {
            if (buf.Length - offs < 4)
                return true;
            int n = BitConverter.ToInt32(buf, offs);
            offs += 4;
            while (0 < n)
            {
                --n;
                int rId = BitConverter.ToInt32(buf, offs);
                offs += 4;
                ExamRoom r;
                if (vRoom.TryGetValue(rId, out r) &&
                    r.ReadByte0(buf, ref offs))
                        return true;
            }
            if (n == 0)
                return false;
            else
                return true;
        }

        public List<byte[]> ToByteR1(int rId)
        {
            List<byte[]> l = new List<byte[]>();
            l.Add(DT.ToByteh(mDt));
            ExamRoom r;
            if (rId == 0)
            {
                l.Add(BitConverter.GetBytes(vRoom.Count));
                foreach (ExamRoom i in vRoom.Values)
                    l.InsertRange(l.Count, i.ToByte1());
            }
            else if (vRoom.TryGetValue(rId, out r))
            {
                l.Add(BitConverter.GetBytes(1));
                l.InsertRange(l.Count, r.ToByte1());
            }
            else
                l.Add(BitConverter.GetBytes(0));
            return l;
        }

        public bool ReadByteR1(byte[] buf, ref int offs)
        {
            if (buf.Length - offs < 4)
                return true;
            int n = BitConverter.ToInt32(buf, offs);
            offs += 4;
            while (0 < n)
            {
                --n;
                int rId = BitConverter.ToInt32(buf, offs);
                offs += 4;
                ExamRoom r;
                if (vRoom.TryGetValue(rId, out r))
                {
                    if (r.ReadByte1(buf, ref offs))
                        return true;
                }
                else
                {
                    r = new ExamRoom();
                    r.uId = rId;
                    if (r.ReadByte1(buf, ref offs))
                        return true;
                    vRoom.Add(rId, r);
                }
            }
            if (n == 0)
                return false;
            else
                return true;
        }

        public void DBUpdateRs(StringBuilder vals)
        {
            foreach (ExamRoom r in vRoom.Values)
                r.DBUpdateRs(vals);
        }

        public bool GenQ(int n, ExamLv lv, int[] vn, int[] vndiff)
        {
            string emsg;
            if (vQPack[lv].DBDelete(out emsg))//todo: only mark del, not del from db
                WPopup.s.ShowDialog(emsg);
            foreach (QuestSheet qs in vQPack[lv].vSheet.Values)
                mKeyPack.vSheet.Remove(qs.LvId);
            vQPack[lv].vSheet.Clear();
            List<QuestSheet> l;
            if(System.IO.File.Exists("Randomize.txt"))
                l = vQPack[lv].GenQPack2(n, vn, vndiff);
            else
                l = vQPack[lv].GenQPack3(n, vn, vndiff);
            mKeyPack.ExtractKey(l);
            return false;
        }

        public bool DBSelArchieve(out string eMsg)
        {
            if (vQPack[ExamLv.A].DBSelectQS(mDt, out eMsg))
                return true;
            if (vQPack[ExamLv.B].DBSelectQS(mDt, out eMsg))
                return true;
            List<QuestSheet> qss = new List<QuestSheet>();
            foreach (QuestPack p in vQPack.Values)
                foreach (QuestSheet qs in p.vSheet.Values)
                    mKeyPack.ExtractKey(qs);
            return false;
        }

        public List<byte[]> ToByteQPack()
        {
            List<byte[]> l = new List<byte[]>();
            l.Add(DT.ToByteh(mDt));
            l.Add(BitConverter.GetBytes(vQPack.Count));
            foreach (QuestPack p in vQPack.Values)
            {
                l.Add(BitConverter.GetBytes((int)p.eLv));
                l.Add(p.ToByte());
            }
            return l;
        }

        public bool ReadByteQPack(byte[] buf, ref int offs)
        {
            if (buf.Length - offs < 4)
                return true;
            int n = BitConverter.ToInt32(buf, offs);
            offs += 4;
            while (0 < n)
            {
                --n;
                int x;
                ExamLv lv;
                if (Enum.IsDefined(typeof(ExamLv), x = BitConverter.ToInt32(buf, offs)))
                    lv = (ExamLv)x;
                else
                    return true;
                offs += 4;
                if (vQPack[lv].ReadByte(buf, ref offs))
                    return true;
            }
            if (n == 0)
                return false;
            else
                return true;
        }

        public bool ReadByteQPack1(ExamLv lv, byte[] buf, ref int offs)
        {
            if (vQPack[lv].ReadByte1(buf, ref offs))
                return true;
            return false;
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
            foreach (ExamRoom r in vRoom.Values)
                if ((o = r.Signin(e)) != null)
                    return o;
            return null;
        }

        public ExamineeA Find(int lvid)
        {
            ExamineeA o;
            foreach (ExamRoom r in vRoom.Values)
                if (r.vExaminee.TryGetValue(lvid, out o))
                    return o;
            return null;
        }

        public List<byte[]> ToByteR0()
        {
            List<byte[]> l = new List<byte[]>();
            l.Add(DT.ToByteh(mDt));
            l.Add(BitConverter.GetBytes(vRoom.Count));
            foreach (ExamRoom r in vRoom.Values)
                l.InsertRange(l.Count, r.ToByte0());
            return l;
        }
    }
}
