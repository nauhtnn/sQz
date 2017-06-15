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
        public Dictionary<int, ExamineeA> vExaminee;
        public ExamRoom()
        {
            uId = ushort.MaxValue;
            vExaminee = new Dictionary<int, ExamineeA>();
        }

        public Dictionary<int, int>  DBSelectId(uint slId, out Dictionary<int, string> vAns)
        {
            vAns = new Dictionary<int, string>();
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return null;
            string qry = DBConnect.mkQrySelect("sqz_examinee", "lv,id,qId,anssh", "slId=" + slId);
            string emsg;
            MySqlDataReader reader = DBConnect.exeQrySelect(conn, qry, out emsg);
            Dictionary<int, int> r = new Dictionary<int, int>();
            if (reader != null)
            {
                while (reader.Read())
                {
                    int id = reader.GetInt32(0) + reader.GetInt32(1);
                    if(!reader.IsDBNull(2))
                        r.Add(id, reader.GetInt32(2));
                    if (!reader.IsDBNull(3))
                        vAns.Add(id, reader.GetString(3));
                }
                reader.Close();
            }
            DBConnect.Close(ref conn);
            return r;
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

        public void DBUpdateRs(StringBuilder vals)
        {
            foreach (ExamineeS0 e in vExaminee.Values)
                if (e.bToDB)
                {
                    vals.Append("('" + e.mDt.ToString(DT._) + "','" + e.eLv.ToString() + "'," +
                        e.uId + "," + (e.mAnsSh.uQSId - ((e.eLv == ExamLv.A) ? 0 : ExamineeA.LV_CAP))
                        + ",'" + e.dtTim1.ToString(DT.hh) +
                        "','" + e.dtTim2.ToString(DT.hh) + "'," + e.uGrade + ",'" +
                        e.tComp + "','" + e.mAnsSh.tAns + "'),");
                    e.bToDB = false;
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
            foreach (ExamineeA e in vExaminee.Values)
                if (e.eStt == ExamStt.Finished)
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
            return r;
        }
    }
}
