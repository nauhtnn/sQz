using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace sQzLib
{
    public class ExamRoomS0: ExamRoomA
    {
        public string tPw;

        public ExamRoomS0()
        {
            tPw = null;
        }

        public int DBIns(MySqlConnection conn, out string eMsg)
        {
            if (Examinees.Count == 0)
            {
                eMsg = null;
                return 0;
            }
            string attbs = "dt,id,rid,name,birthdate,birthplace";
            StringBuilder vals = new StringBuilder();
            foreach (ExamineeA e in Examinees.Values)
            {
                vals.Append("('" + e.mDt.ToString(DT._) + "','");
                vals.Append(e.ID + "',");
                vals.Append(uId + ",");
                vals.Append("'" + e.Name + "',");
                vals.Append("'" + e.Birthdate + "',");
                vals.Append("'" + e.Birthplace + "'),");
            }
            vals.Remove(vals.Length - 1, 1);//remove the last comma
            int n = DBConnect.Ins(conn, "sqz_examinee",
                attbs, vals.ToString(), out eMsg);
            return n;
        }

        public void DBSelNee(MySqlConnection conn, DateTime dt)
        {
            Examinees.Clear();
            string qry = DBConnect.mkQrySelect("sqz_slot_room AS a,sqz_examinee AS b",
                "id,name,birthdate,birthplace", "a.rid=" + uId +
                " AND a.dt='" + dt.ToString(DT._) +
                "' AND a.dt=b.dt AND a.rid=b.rid");
            string emsg;
            MySqlDataReader reader = DBConnect.exeQrySelect(conn, qry, out emsg);
            if (reader == null)
                return;
            while (reader.Read())
            {
                ExamineeS0 e = new ExamineeS0();
                e.mDt = dt;
                e.ID = reader.GetString(0);
                e.Name = reader.GetString(1);
                e.Birthdate = reader.GetString(2);
                e.Birthplace = reader.GetString(3);
                Examinees.Add(e.ID, e);
            }
            reader.Close();

            bool showError = true;
            foreach (ExamineeA e in Examinees.Values)
            {
                qry = DBConnect.mkQrySelect("sqz_nee_qsheet",
                    "t1,t2,grade,comp", "dt='" + e.mDt.ToString(DT._) +
                    "' AND neeid='" + e.ID + "'");
                reader = DBConnect.exeQrySelect(conn, qry, out emsg);
                if (reader != null)
                {
                    if (reader.Read())
                    {
                        if (DT.Toh(reader.GetString(0), DT.hs, out e.dtTim1))
                            break;
                        if (DT.Toh(reader.GetString(1), DT.hs, out e.dtTim2))
                            break;
                        e.Grade = reader.GetInt16(2);
                        e.ComputerName = reader.GetString(3);
                        e.eStt = NeeStt.Finished;
                    }
                    reader.Close();
                }
                else if (showError)
                {
                    showError = false;
                    System.Windows.MessageBox.Show("DBSelNee error\n" + emsg.ToString());
                }
            }
        }

        public void DBUpdateRs(StringBuilder vals)
        {
            foreach (ExamineeS0 e in Examinees.Values)
                if (e.bToDB)
                {
                    e.bToDB = false;
                    vals.Append("('" + e.mDt.ToString(DT._) + "','" +
                        e.ID + "'," + (e.mAnsSh.uQSId)
                        + ",'" + e.dtTim1.ToString(DT.hh) +
                        "','" + e.dtTim2.ToString(DT.hh) + "'," + e.Grade + ",'" +
                        e.ComputerName + "','" + e.mAnsSh.tAns + "'),");
                }
        }

        public bool DBSelTimeAndPw(DateTime dt, out string eMsg)
        {
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
            {
                eMsg = Txt.s._((int)TxI.DB_NOK);
                return true;
            }
            string qry = DBConnect.mkQrySelect("sqz_slot_room", "pw,t1,t2",
                "dt='" + dt.ToString(DT._) + "' AND rid=" + uId + " LIMIT 1");
            MySqlDataReader reader = DBConnect.exeQrySelect(conn, qry, out eMsg);
            if (reader == null)
            {
                DBConnect.Close(ref conn);
                return true;
            }
            if (reader.Read())
            {
                tPw = reader.IsDBNull(0) ? null : reader.GetString(0);
                if (reader.IsDBNull(1) || DT.Toh(reader.GetString(1), DT.hs, out t1))
                    t1 = DT.INVALID;
                if (reader.IsDBNull(2) || DT.Toh(reader.GetString(2), DT.hs, out t2))
                    t2 = DT.INVALID;
            }
            reader.Close();
            DBConnect.Close(ref conn);
            return false;
        }

        public bool DBUpT1(MySqlConnection conn, DateTime dt, out string eMsg)
        {
            string cond = "dt='" + dt.ToString(DT._) + "' AND rid=" + uId;
            string val = "t1='" + t1.ToString(DT.hh) + "'";
            if (DBConnect.Update(conn, "sqz_slot_room", val, cond, out eMsg) < 1)
                return true;
            return false;
        }

        public bool DBUpT2(MySqlConnection conn, DateTime dt,
            out string eMsg)
        {
            string cond = "dt='" + dt.ToString(DT._) + "' AND rid=" + uId;
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
                eMsg = Txt.s._((int)TxI.DB_NOK);
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
            if (Examinees.Count == 0)
                return true;
            string vch = PwChars();
            Random r = new Random();
            ExamineeA x = Examinees.Values.First();
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return true;
            string otPw = tPw;
            tPw = GenPw(vch, r);
            string emsg;
            int n = DBConnect.Update(conn, "sqz_slot_room", "pw='" + tPw + "'",
                "dt='" + x.mDt.ToString(DT._) + "' AND rid=" + uId, out emsg);
            if (0 < n)
                return false;
            tPw = otPw;
            return true;
        }

        public static string PwChars()
        {
            StringBuilder sb = new StringBuilder();
            for (char i = '0'; i <= '9'; ++i)
                sb.Append(i);
            //for (char i = 'A'; i < 'I'; ++i)
            //    sb.Append(i);
            //for (char i = 'J'; i <= 'Z'; ++i)
            //    sb.Append(i);
            //for (char i = 'a'; i < 'l'; ++i)
            //    sb.Append(i);
            //for (char i = 'm'; i <= 'z'; ++i)
            //    sb.Append(i);
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

        public List<byte[]> GetBytes()
        {
            List<byte[]> l = new List<byte[]>();
            l.Add(BitConverter.GetBytes(uId));
            int n = 0;
            foreach (ExamineeS1 e in Examinees.Values)
                if (e.eStt == NeeStt.Finished && e.NRecd)
                {
                    ++n;
                    e.bFromC = false;
                    l.InsertRange(l.Count, e.ToByte());
                }
            l.Insert(1, BitConverter.GetBytes(n));
            return l;
        }
    }
}
