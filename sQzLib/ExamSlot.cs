using System;
using System.Collections.Generic;
using System.Windows.Controls;
using MySql.Data.MySqlClient;
using System.Windows.Media;
using System.Windows;
using System.Text;

/*
CREATE TABLE IF NOT EXISTS `slot` (`dt` DATE, `t` TIME, `open` TINYINT DEFAULT 0,
PRIMARY KEY(`dt`, `t`), FOREIGN KEY(`dt`) REFERENCES `board`(`dt`));
*/

namespace sQzLib
{
    public class ExamSlot
    {
        public DateTime mDt;
        public bool bOpen;
        public Dictionary<ExamLv, QuestPack> vQPack;

        public AnsPack mKeyPack;

        public Dictionary<int, ExamRoom> vRoom;

        public ExamSlot()
        {
            mDt = DtFmt.INV_;

            vRoom = new Dictionary<int, ExamRoom>();
            for (int i = 1; i < 7; ++i)//todo: read from db
            {
                ExamRoom r = new ExamRoom();
                r.uId = i;
                vRoom.Add(i, r);
            }
            vQPack = new Dictionary<ExamLv, QuestPack>();
            QuestPack p = new QuestPack();
            p.eLv = ExamLv.A;
            vQPack.Add(p.eLv, p);
            p = new QuestPack();
            p.eLv = ExamLv.B;
            vQPack.Add(p.eLv, p);

            mKeyPack = new AnsPack();
        }

        public static Dictionary<uint, Tuple<DateTime, bool>> DBSelect()
        {
            Dictionary<uint, Tuple<DateTime, bool>> r =
                new Dictionary<uint, Tuple<DateTime, bool>>();
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return r;
            string qry = DBConnect.mkQrySelect("slot", null, null, null);
            MySqlDataReader reader = null;//todo DBConnect.exeQrySelect(conn, qry);
            while (reader.Read())
                r.Add(reader.GetUInt32(0),
                    Tuple.Create(reader.GetDateTime(1), reader.GetBoolean(2)));
            reader.Close();
            DBConnect.Close(ref conn);
            return r;
        }

        public int GetByteCountDt()
        {
            return 20;
        }

        public static void ToByteDt(byte[] buf, ref int offs, DateTime dt)
        {
            Array.Copy(BitConverter.GetBytes(dt.Year), 0, buf, offs, 4);
            offs += 4;
            Array.Copy(BitConverter.GetBytes(dt.Month), 0, buf, offs, 4);
            offs += 4;
            Array.Copy(BitConverter.GetBytes(dt.Day), 0, buf, offs, 4);
            offs += 4;
            Array.Copy(BitConverter.GetBytes(dt.Hour), 0, buf, offs, 4);
            offs += 4;
            Array.Copy(BitConverter.GetBytes(dt.Minute), 0, buf, offs, 4);
            offs += 4;
        }

        public static bool ReadByteDt(byte[] buf, ref int offs, out DateTime dt)
        {
            if (buf.Length - offs < 20)
            {
                dt = DtFmt.INV_;
                return true;
            }
            int y = BitConverter.ToInt32(buf, offs);
            offs += 4;
            int M = BitConverter.ToInt32(buf, offs);
            offs += 4;
            int d = BitConverter.ToInt32(buf, offs);
            offs += 4;
            int H = BitConverter.ToInt32(buf, offs);
            offs += 4;
            int m = BitConverter.ToInt32(buf, offs);
            offs += 4;
            if (DtFmt.ToDt(y.ToString("d4") + '/' + M.ToString("d2") + '/' + d.ToString("d2") +
                ' ' + H.ToString("d2") + ':' + m.ToString("d2"), DtFmt.H, out dt))
                return true;
            return false;
        }

        public List<byte[]> ToByteR1(int rId)
        {
            List<byte[]> l = new List<byte[]>();
            ExamRoom r;
            if (rId == 0)
                foreach (ExamRoom i in vRoom.Values)
                {
                    byte[] a = i.ToByteS1();
                    if (3 < a.Length)
                    {
                        l.Add(BitConverter.GetBytes(i.uId));//todo: optmz duplication
                        l.Add(a);
                    }
                }
            else if (vRoom.TryGetValue(rId, out r))
            {
                l.Add(BitConverter.GetBytes(rId));//todo: optmz duplication
                l.Add(r.ToByteS1());
            }
            return l;
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
                    ExamLv x;
                    if(Enum.TryParse(v[0].Substring(0, 1), out x))
                        e.eLv = x;
                    else
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
                    e.uId = e.uId + (int)e.eLv;
                    if(vRoom[urid].vExaminee.ContainsKey(e.uId) || o.vRoom[urid].vExaminee.ContainsKey(e.uId))
                    {
                        dup.Append(e.eLv.ToString() + (e.uId - (int)e.eLv) + ", ");
                        continue;
                    }
                    e.mDt = mDt;
                    e.tName = v[2].Trim();
                    DateTime dt;
                    if(!DtFmt.ToDt(v[3], DtFmt._, out dt))
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
                r.Append(dup.ToString() + '.');
            }
            if (r.Length == 0)
                return null;
            else
                return Txt.s._[(int)TxI.NEE_FERR] + r.ToString();
        }

        public int DBInsNee(out string eMsg)
        {
            int v = 1;
            StringBuilder sb = new StringBuilder();
            foreach (ExamRoom r in vRoom.Values)
            {
                int n = r.DBIns(out eMsg);
                if(n < 0)
                {
                    string[] p = new string[2];
                    p[0] = r.uId.ToString();
                    if (n == -1062)
                        p[1] = Txt.s._[(int)TxI.NEE_EXIST];
                    else
                        p[1] = eMsg;
                    sb.AppendFormat(Txt.s._[(int)TxI.ROOM_DB_NOK] + '\n', p);
                    v = 0;
                }
            }
            eMsg = sb.ToString();
            return v;
        }

        public void DelNee()
        {
            StringBuilder sb = new StringBuilder();
            foreach (ExamRoom r in vRoom.Values)
                r.vExaminee.Clear();
        }

        public string DBDelNee()
        {
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return Txt.s._[(int)TxI.DB_NOK];
            StringBuilder sb = new StringBuilder();
            foreach (ExamRoom r in vRoom.Values)
            {
                int n = DBConnect.Count(conn, ExamineeS0.tDBtbl + r.uId,
                    "dt", "dt='" + mDt.ToString(DtFmt._) +
                    "' AND t='" + mDt.ToString(DtFmt.hh) +
                    "' AND grd IS NOT NULL");
                sb.AppendFormat(Txt.s._[(int)TxI.ROOM_DEL], r.uId);
                if (0 < n)
                    sb.Append(Txt.s._[(int)TxI.ROOM_DEL_GRD] + '\n');
                else if (n < 0)
                    sb.Append(Txt.s._[(int)TxI.ROOM_DEL_ECPT] + '\n');
                else
                {
                    n = DBConnect.Delete(conn, ExamineeS0.tDBtbl + r.uId,
                        "dt='" + mDt.ToString(DtFmt._) + "' AND t='" + mDt.ToString(DtFmt.hh) + "'");
                    if (n < 0)
                        sb.Append(Txt.s._[(int)TxI.ROOM_DEL_ECPT] + '\n');
                    else
                        sb.AppendFormat(Txt.s._[(int)TxI.ROOM_DEL_N] + '\n', n.ToString());
                }
            }
            DBConnect.Close(ref conn);
            return sb.ToString();
        }

        public void DBSelNee()
        {
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return;
            foreach (ExamRoom r in vRoom.Values)
            {
                r.vExaminee.Clear();
                string qry = DBConnect.mkQrySelect(ExamineeS0.tDBtbl + r.uId,
                    "dt,t,id,name,birdate,birthplace,t1,t2,grd,comp,qId,anssh",
                    "dt='" + mDt.ToString(DtFmt._) + "' AND t='" + mDt.ToString(DtFmt.hh) + "'", null);
                string emsg;
                MySqlDataReader reader = DBConnect.exeQrySelect(conn, qry, out emsg);
                if (reader != null)
                {
                    while (reader.Read())
                    {
                        ExamineeS0 e = new ExamineeS0();
                        DateTime dt = reader.GetDateTime(0);
                        string t = reader.GetString(1);
                        DtFmt.ToDt(dt.ToString(DtFmt.__) + ' ' + t, DtFmt.HS, out e.mDt);
                        e.uId = (int) reader.GetUInt32(2);//todo no coerce
                        if (e.uId < (int)ExamLv.B)
                            e.eLv = ExamLv.A;
                        else
                            e.eLv = ExamLv.B;
                        e.tName = reader.GetString(3);
                        e.tBirdate = reader.GetDateTime(4).ToString(DtFmt.RR);
                        e.tBirthplace = reader.GetString(5);
                        e.dtTim1 = (reader.IsDBNull(6)) ? DtFmt.INV_ :
                            DateTime.Parse(reader.GetString(6));
                        e.dtTim2 = (reader.IsDBNull(7)) ? DtFmt.INV_ :
                            DateTime.Parse(reader.GetString(7));
                        if (!reader.IsDBNull(8))
                        {
                            e.eStt = ExamStt.Finished;
                            e.uGrade = reader.GetInt32(8);
                        }
                        else
                            e.eStt = ExamStt.Info;
                        if (!reader.IsDBNull(9))
                            e.tComp = reader.GetString(9);
                        else
                            e.tComp = "unknown";//todo
                        r.vExaminee.Add(e.uId, e);
                    }
                    reader.Close();
                }
            }
            DBConnect.Close(ref conn);
        }

        public void ReadByteR0(byte[] buf, ref int offs)
        {
            List<ExamineeA> v = new List<ExamineeA>();
            List<ExamineeA> l = new List<ExamineeA>();
            while (true)
            {
                if (buf.Length - offs < 4)
                    break;
                int rId = BitConverter.ToInt32(buf, offs);
                offs += 4;
                ExamRoom r;
                if (!vRoom.TryGetValue(rId, out r))
                    break;
                if (r.ReadByteS0(buf, ref offs, ref v))
                    break;
                foreach (ExamineeS0 e in v)
                {
                    ExamineeA o;
                    bool unfound = true;
                    foreach (ExamRoom i in vRoom.Values)
                        if (i.uId != rId && i.vExaminee.TryGetValue(e.mLv + e.uId, out o))
                        {
                            unfound = false;
                            //o.bFromC = false;
                            o.Merge(e);
                            break;
                        }
                    if (unfound)
                        l.Add(e);
                }
                v.Clear();
            }
        }

        public void ReadByteR1(byte[] buf, ref int offs)
        {
            while (3 < buf.Length - offs)
            {
                int rId = BitConverter.ToInt32(buf, offs);
                offs += 4;
                ExamRoom r;
                if (vRoom.TryGetValue(rId, out r))
                    r.ReadByteS1(buf, ref offs);
                else
                {
                    r = new ExamRoom();
                    r.uId = rId;
                    r.ReadByteS1(buf, ref offs);
                    vRoom.Add(rId, r);
                }
            }
        }

        public void DBUpdateRs()
        {
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return;
            foreach (ExamRoom r in vRoom.Values)
                r.DBUpdateRs(conn);
            DBConnect.Close(ref conn);
        }

        public bool GenQPack(int n, ExamLv lv, int[] vn)
        {
            List<QuestSheet> l = vQPack[lv].GenQPack(n, vn);
            mKeyPack.ExtractKey(l);
            return false;
        }

        public byte[] ToByteQPack()
        {
            List<byte[]> l = new List<byte[]>();
            foreach (QuestPack p in vQPack.Values)
            {
                l.Add(BitConverter.GetBytes((int)p.eLv));
                l.Add(p.ToByte());
            }
            int sz = 0;
            foreach (byte[] x in l)
                sz += x.Length;
            byte[] buf = new byte[sz];
            int offs = 0;
            foreach(byte[] x in l)
            {
                Array.Copy(x, 0, buf, offs, x.Length);
                offs += x.Length;
            }
            return buf;
        }

        public bool ReadByteQPack(byte[] buf, ref int offs)
        {
            while(0 < buf.Length - offs)
            {
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
            return false;
        }

        public bool ReadByteQPack1(ExamLv lv, byte[] buf, ref int offs)
        {
            if (vQPack[lv].ReadByte1(buf, ref offs))
                return true;
            return false;
        }

        public byte[] ToByteKey()
        {
            return mKeyPack.ToByte();
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
    }
}
