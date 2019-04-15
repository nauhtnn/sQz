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
        public const string PW_CHAR_SET = "0123456789";
        public int uId;
        public SortedList<int, ExamineeA> vExaminee;
        public DateTime t1, t2;
        public Dictionary<ExamLv, int> N_ExamineeGroupByLv;
        public string tPw;
        public ExamRoom()
        {
            uId = ExamineeA.LV_CAP;
            vExaminee = new SortedList<int, ExamineeA>();
            N_ExamineeGroupByLv = new Dictionary<ExamLv, int>();
            tPw = null;
        }

        public int DBIns()
        {
            if(vExaminee.Count == 0)
                return 0;
            string attbs = "dt,lv,id,t,rid,name,birdate,birthplace";
            StringBuilder vals = new StringBuilder();
            foreach (ExamineeA e in vExaminee.Values)
            {
                vals.Append("('" + e.mDt.ToString(DT._) + "','");
                vals.Append(e.mLv.ToString() + "',");
                vals.Append(e.uId + ",");
                vals.Append("'" + e.mDt.ToString(DT.hh) + "',");
                vals.Append(uId + ",");
                vals.Append("'" + e.tName + "',");
                vals.Append("'" + DT.ToS(e.tBirdate, DT.RR) + "',");
                vals.Append("'" + e.tBirthplace + "'),");
            }
            vals.Remove(vals.Length - 1, 1);//remove the last comma
            return DBConnect.Ins("sqz_examinee", attbs, vals.ToString());
        }

        public void DBSelectExaminee(MySqlConnection conn, DateTime dt)
        {
            vExaminee.Clear();
            N_ExamineeGroupByLv[ExamLv.A] = N_ExamineeGroupByLv[ExamLv.B] = 0;
            MySqlDataReader reader = DBConnect.exeQrySelect("sqz_slot_room AS a,sqz_examinee AS b",
                "lv,id,name,birdate,birthplace", "a.rid=" + uId +
                " AND a.dt='" + dt.ToString(DT._) + "' AND a.t='" + dt.ToString(DT.hh) +
                "' AND a.dt=b.dt AND a.t=b.t AND a.rid=b.rid");
            while (reader.Read())
            {
                ExamineeS0 e = new ExamineeS0();
                e.mDt = dt;
                if (!Enum.TryParse(reader.GetString(0), out e.mLv))
                    continue;
                e.uId = reader.GetUInt16(1);
                e.tName = reader.GetString(2);
                e.tBirdate = reader.GetDateTime(3).ToString(DT.RR);
                e.tBirthplace = reader.GetString(4);
                vExaminee.Add(e.LvId, e);
                ++N_ExamineeGroupByLv[e.mLv];
            }
            reader.Close();

            foreach (ExamineeA e in vExaminee.Values)
            {
                reader = DBConnect.exeQrySelect("sqz_nee_qsheet",
                    "t1,t2,grade,comp", "dt='" + e.mDt.ToString(DT._) + "' AND lv='" +
                    e.mLv + "' AND neeid=" + e.uId);
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

        public void DBMakeInsResult(StringBuilder vals)
        {
            foreach (ExamineeS0 e in vExaminee.Values)
                if (e.bToDB)
                {
                    e.bToDB = false;
                    vals.Append("('" + e.mDt.ToString(DT._) + "','" + e.mLv.ToString() + "'," +
                        e.uId + "," + (e.mAnsSh.uQSId)
                        + ",'" + e.dtTim1.ToString(DT.hh) +
                        "','" + e.dtTim2.ToString(DT.hh) + "'," + e.uGrade + ",'" +
                        e.tComp + "','" + e.mAnsSh.tAns + "'),");
                }
        }

        public ExamineeA Signin(ExamineeA e)
        {
            ExamineeA o;
            if (vExaminee.TryGetValue(e.LvId, out o) && o.tBirdate == e.tBirdate)
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
                if (vExaminee.TryGetValue(e.LvId, out o))
                {
                    o.bFromC = false;
                    o.Merge(e);
                }
                else
                    vExaminee.Add(e.LvId, e);
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
                if (vExaminee.TryGetValue(e.LvId, out o))
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

        public bool DBSelTimeAndPw(DateTime dt)
        {
            MySqlDataReader reader = DBConnect.exeQrySelect("sqz_slot_room", "pw,t1,t2",
                "dt='" + dt.ToString(DT._) + "' AND t='" + dt.ToString(DT.hh) +
                "' AND rid=" + uId + " LIMIT 1");
            if(reader.Read())
            {
                tPw = reader.IsDBNull(0) ? null : reader.GetString(0);
                if (reader.IsDBNull(1) || DT.Toh(reader.GetString(1), DT.hs, out t1))
                    t1 = DT.INV_;
                if (reader.IsDBNull(2) || DT.Toh(reader.GetString(2), DT.hs, out t2))
                    t2 = DT.INV_;
            }
            reader.Close();
            return false;
        }

        public bool DBUpT1(DateTime dt)
        {
            string cond = "dt='" + dt.ToString(DT._) + "' AND t='" +
                dt.ToString(DT.hh) + "' AND rid=" + uId;
            string val = "t1='" + t1.ToString(DT.hh) + "'";
            if (DBConnect.Update("sqz_slot_room", val, cond) < 1)
                return true;
            return false;
        }

        public bool DBUpT2(MySqlConnection conn, DateTime dt)
        {
            string cond = "dt='" + dt.ToString(DT._) + "' AND t='" +
                dt.ToString(DT.hh) + "' AND rid=" + uId;
            string val = "t2='" + t2.ToString(DT.hh) + "'";
            if (DBConnect.Update("sqz_slot_room", val, cond) < 1)
                return true;
            return false;
        }

        public static List<int> DBSelectRoomIDs()
        {
            MySqlDataReader reader = DBConnect.exeQrySelect("sqz_room", null, null);
            List<int> r = new List<int>();
            while (reader.Read())
                r.Add(reader.GetInt32(0));
            reader.Close();
            return r;
        }

        public static string GeneratePw(Random r)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 8; ++i)
                sb.Append(PW_CHAR_SET[r.Next() % PW_CHAR_SET.Length]);
            return sb.ToString();
        }

        public bool RegeneratePw()
        {
            if (vExaminee.Count == 0)
                return true;
            Random r = new Random();
            ExamineeA x = vExaminee.Values.First();
            string otPw = tPw;
            tPw = GeneratePw(r);
            int n = DBConnect.Update("sqz_slot_room", "pw='" + tPw + "'",
                "dt='" + x.mDt.ToString(DT._) + "' AND t='" +
                x.mDt.ToString(DT.hh) + "' AND rid=" + uId);
            if(0 < n)
                return false;
            tPw = otPw;
            return true;
        }
    }
}
