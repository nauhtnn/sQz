using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace sQzLib
{
    public class ExamRoom
    {
        public int uId;
        public SortedList<int, ExamineeA> vExaminee;
        public DateTime t1, t2;
        public Dictionary<ExamLv, int> nLv;
        public string tPw;
        public ExamRoom()
        {
            uId = ExamineeA.LV_CAP;
            vExaminee = new SortedList<int, ExamineeA>();
            nLv = new Dictionary<ExamLv, int>();
            tPw = null;
        }

        public int DBIns(MySqlConnection conn, out string eMsg)
        {
            if(vExaminee.Count == 0)
            {
                eMsg = null;
                return 0;
            }
            string attbs = "dt,lv,id,t,rid,name,birdate,birthplace";
            StringBuilder vals = new StringBuilder();
            foreach (ExamineeA e in vExaminee.Values)
            {
                vals.Append("('" + e.mDt.ToString(DT._) + "','");
                vals.Append(e.eLv.ToString() + "',");
                vals.Append(e.uId + ",");
                vals.Append("'" + e.mDt.ToString(DT.hh) + "',");
                vals.Append(uId + ",");
                vals.Append("'" + e.tName + "',");
                vals.Append("'" + DT.ToS(e.tBirdate, DT.RR) + "',");
                vals.Append("'" + e.tBirthplace + "'),");
            }
            vals.Remove(vals.Length - 1, 1);//remove the last comma
            int n = DBConnect.Ins(conn, "sqz_examinee",
                attbs, vals.ToString(), out eMsg);
            return n;
        }

        public void DBSelNee(MySqlConnection conn, DateTime dt)
        {
            vExaminee.Clear();
            nLv[ExamLv.A] = nLv[ExamLv.B] = 0;
            string qry = DBConnect.mkQrySelect("sqz_slot_room AS a,sqz_examinee AS b",
                "lv,id,name,birdate,birthplace", "a.rid=" + uId +
                " AND a.dt='" + dt.ToString(DT._) + "' AND a.t='" + dt.ToString(DT.hh) +
                "' AND a.dt=b.dt AND a.t=b.t AND a.rid=b.rid");
            string emsg;
            MySqlDataReader reader = DBConnect.exeQrySelect(conn, qry, out emsg);
            if (reader == null)
                return;
            while (reader.Read())
            {
                ExamineeS0 e = new ExamineeS0();
                e.mDt = dt;
                if (!Enum.TryParse(reader.GetString(0), out e.eLv))
                    continue;
                e.uId = reader.GetUInt16(1);
                e.tName = reader.GetString(2);
                e.tBirdate = reader.GetDateTime(3).ToString(DT.RR);
                e.tBirthplace = reader.GetString(4);
                vExaminee.Add(e.uId, e);
                ++nLv[e.eLv];
            }
            reader.Close();

            foreach (ExamineeA e in vExaminee.Values)
            {
                qry = DBConnect.mkQrySelect("sqz_nee_qsheet",
                    "t1,t2,grade,comp", "dt='" + e.mDt.ToString(DT._) + "' AND lv='" +
                    e.eLv + "' AND neeid=" + e.uId);
                reader = DBConnect.exeQrySelect(conn, qry, out emsg);
                if (reader != null)
                {
                    if (reader.Read())
                    {
                        if (DT.Toh(reader.GetString(0), DT.hs, out e.dtTim1))
                            break;
                        if (DT.Toh(reader.GetString(1), DT.hs, out e.dtTim2))
                            break;
                        e.uGrade = reader.GetInt16(2);
                        e.tComp = reader.GetString(3);
                        e.eStt = NeeStt.Finished;
                    }
                    reader.Close();
                }
            }
        }

        public void DBUpdateRs(StringBuilder vals)
        {
            foreach (ExamineeS0 e in vExaminee.Values)
                if (e.bToDB)
                {
                    e.bToDB = false;
                    vals.Append("('" + e.mDt.ToString(DT._) + "','" + e.eLv.ToString() + "'," +
                        e.uId + "," + (e.mAnsSh.uQSId)
                        + ",'" + e.dtTim1.ToString(DT.hh) +
                        "','" + e.dtTim2.ToString(DT.hh) + "'," + e.uGrade + ",'" +
                        e.tComp + "','" + e.mAnsSh.tAns + "'),");
                }
        }

        public ExamineeA Signin(ExamineeA e)
        {
            ExamineeA o;
            if (vExaminee.TryGetValue(e.uId, out o) && o.tBirdate == e.tBirdate)
            {
                o.bFromC = true;
                o.Merge(e);
                return o;
            }
            return null;
        }

        public List<byte[]> ToByte1()
        {
            List<byte[]> l = new List<byte[]>();
            l.Add(BitConverter.GetBytes(uId));
            l.Add(BitConverter.GetBytes(vExaminee.Count));
            foreach (ExamineeS0 e in vExaminee.Values)
                l.InsertRange(l.Count, e.ToByte());
            return l;
        }

        public bool ReadByte1(byte[] buf, ref int offs)
        {
            if (buf == null)
                return true;
            if (buf.Length - offs < 0)
                return true;
            int n = BitConverter.ToInt32(buf, offs);
            offs += 4;
            while (0 < n)
            {
                --n;
                ExamineeS1 e = new ExamineeS1();
                //e.bFromC = false;
                if (e.ReadByte(buf, ref offs))
                    return true;
                ExamineeA o;
                if (vExaminee.TryGetValue(e.uId, out o))
                {
                    o.bFromC = false;
                    o.Merge(e);
                }
                else
                    vExaminee.Add(e.uId, e);
            }
            if (n == 0)
                return false;
            else
                return true;
        }

        public List<byte[]> ToByte0()
        {
            List<byte[]> l = new List<byte[]>();
            l.Add(BitConverter.GetBytes(uId));
            int n = 0;
            foreach (ExamineeS1 e in vExaminee.Values)
                if (e.eStt == NeeStt.Finished && e.NRecd)
                {
                    ++n;
                    e.bFromC = false;
                    l.InsertRange(l.Count, e.ToByte());
                }
            l.Insert(1, BitConverter.GetBytes(n));
            return l;
        }

        public bool ReadByte0(byte[] buf, ref int offs)
        {
            if (buf == null)
                return true;
            if (buf.Length - offs < 4)
                return true;
            int n = BitConverter.ToInt32(buf, offs);
            offs += 4;
            while (0 < n)
            {
                --n;
                ExamineeS0 e = new ExamineeS0();
                //e.bFromC = false;
                if (e.ReadByte(buf, ref offs))
                    return true;
                ExamineeA o;
                if (vExaminee.TryGetValue(e.uId, out o))
                {
                    o.bFromC = false;
                    o.Merge(e);
                }
            }
            if (n == 0)
                return false;
            else
                return true;
        }

        public bool DBSelTimeAndPw(DateTime dt, out string eMsg)
        {
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
            {
                eMsg = Txt.s._[(int)TxI.DB_NOK];
                return true;
            }
            string qry = DBConnect.mkQrySelect("sqz_slot_room", "pw,t1,t2",
                "dt='" + dt.ToString(DT._) + "' AND t='" + dt.ToString(DT.hh) +
                "' AND rid=" + uId + " LIMIT 1");
            MySqlDataReader reader = DBConnect.exeQrySelect(conn, qry, out eMsg);
            if (reader == null)
            {
                DBConnect.Close(ref conn);
                return true;
            }
            if(reader.Read())
            {
                tPw = reader.IsDBNull(0) ? null : reader.GetString(0);
                if (reader.IsDBNull(1) || DT.Toh(reader.GetString(1), DT.hs, out t1))
                    t1 = DT.INV_;
                if (reader.IsDBNull(2) || DT.Toh(reader.GetString(2), DT.hs, out t2))
                    t2 = DT.INV_;
            }
            reader.Close();
            DBConnect.Close(ref conn);
            return false;
        }

        public bool DBUpT1(MySqlConnection conn, DateTime dt, out string eMsg)
        {
            string cond = "dt='" + dt.ToString(DT._) + "' AND t='" +
                dt.ToString(DT.hh) + "' AND rid=" + uId;
            string val = "t1='" + t1.ToString(DT.hh) + "'";
            if (DBConnect.Update(conn, "sqz_slot_room", val, cond, out eMsg) < 1)
                return true;
            return false;
        }

        public bool DBUpT2(MySqlConnection conn, DateTime dt,
            out string eMsg)
        {
            string cond = "dt='" + dt.ToString(DT._) + "' AND t='" +
                dt.ToString(DT.hh) + "' AND rid=" + uId;
            string val = "t2='" + t2.ToString(DT.hh) + "'";
            if (DBConnect.Update(conn, "sqz_slot_room", val, cond, out eMsg) < 1)
                return true;
            return false;
        }

        public static List<int> DBSel(out string eMsg)
        {
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
            {
                eMsg = Txt.s._[(int)TxI.DB_NOK];
                return null;
            }
            string qry = DBConnect.mkQrySelect("sqz_room", null, null);
            MySqlDataReader reader = DBConnect.exeQrySelect(conn, qry, out eMsg);
            if (reader == null)
            {
                DBConnect.Close(ref conn);
                return null;
            }
            List<int> r = new List<int>();
            while (reader.Read())
                r.Add(reader.GetInt32(0));
            reader.Close();
            DBConnect.Close(ref conn);
            return r;
        }

        public static string PwChars()
        {
            StringBuilder sb = new StringBuilder();
            for (char i = '0'; i <= '9'; ++i)
                sb.Append(i);
            for (char i = 'A'; i <= 'Z'; ++i)
                sb.Append(i);
            for (char i = 'a'; i <= 'z'; ++i)
                sb.Append(i);
            //sb.Append('!');
            //sb.Append('@');
            //sb.Append('#');
            //sb.Append('$');
            //sb.Append('%');
            //sb.Append('^');
            //sb.Append('&');
            //sb.Append('*');
            //sb.Append('(');
            //sb.Append(')');
            //sb.Append('-');
            //sb.Append('_');
            return sb.ToString();
        }

        public static string GenPw(string vch, Random r)
        {
            int n = vch.Length;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 8; ++i)
                sb.Append(vch[r.Next() % n]);
            return sb.ToString();
        }

        public bool RegenPw()
        {
            if (vExaminee.Count == 0)
                return true;
            string vch = PwChars();
            Random r = new Random();
            ExamineeA x = vExaminee.Values.First();
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return true;
            string otPw = tPw;
            tPw = GenPw(vch, r);
            string emsg;
            int n = DBConnect.Update(conn, "sqz_slot_room", "pw='" + tPw + "'",
                "dt='" + x.mDt.ToString(DT._) + "' AND t='" +
                x.mDt.ToString(DT.hh) + "' AND rid=" + uId, out emsg);
            if(0 < n)
                return false;
            tPw = otPw;
            return true;
        }
    }
}
