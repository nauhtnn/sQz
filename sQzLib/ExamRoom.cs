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

        public void DBUpdateRs(MySqlConnection conn)
        {
            //foreach (ExamineeA e in vExaminee.Values)
            //{
            //    StringBuilder qry = new StringBuilder("UPDATE exnee" + uId + " SET ");
            //    if (e.dtTim1.Hour != 0)
            //    {
            //        qry.Append("t1='" + e.dtTim1.ToString("HH:mm") + "'");
            //        if (e.dtTim2.Hour != 0)
            //        {
            //            qry.Append(",t2='" + e.dtTim2.ToString("HH:mm") + "'");
            //            if (e.uGrade != short.MaxValue)
            //                qry.Append(",grd=" + e.uGrade);
            //            if (e.tComp != null)
            //            {
            //                if (32 < e.tComp.Length)
            //                    e.tComp = e.tComp.Substring(0, 32);//todo: more responsive
            //                qry.Append(",comp='" + e.tComp + "'");
            //            }
            //            if (e.mAnsSh != null)
            //            {
            //                qry.Append(",qId=" + e.mAnsSh.uQSId);
            //                if (e.mAnsSh.aAns != null)
            //                {
            //                    qry.Append(",anssh='");
            //                    foreach (byte i in e.mAnsSh.aAns)
            //                        qry.Append(i);
            //                    qry.Append("'");
            //                }
            //            }
            //        }
            //        qry.Append(" WHERE slId=" + e.uSlId +
            //            " AND lv=" + (int)e.mLv + " AND id=" + e.uId);
            //        DBConnect.Update(conn, qry.ToString());
            //    }
            //}
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

        public List<byte[]> ToByteS1()
        {
            List<byte[]> l = new List<byte[]>();
            l.Add(BitConverter.GetBytes(uId));
            l.Add(BitConverter.GetBytes(vExaminee.Count));
            foreach (ExamineeS0 e in vExaminee.Values)
                l.InsertRange(l.Count - 1, e.ToByte());
            return l;
        }

        public bool ReadByteS1(byte[] buf, ref int offs, ref List<ExamineeA> v)
        {
            if (buf == null)
                return true;
            int l = buf.Length - offs;
            if (l < 4)
                return true;
            int n = BitConverter.ToInt32(buf, offs);
            l -= 4;
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
                    //o.bFromC = false;
                    o.Merge(e);
                }
                else
                    v.Add(e);
            }
            if (n == 0)
                return false;
            else
                return true;
        }

        public List<byte[]> ToByteS0()
        {
            List<byte[]> l = new List<byte[]>();
            l.Add(BitConverter.GetBytes(uId));
            int n = 0;
            foreach (ExamineeA e in vExaminee.Values)
                if (e.eStt == ExamStt.Finished)
                {
                    ++n;
                    foreach (byte[] b in e.ToByte())
                        l.Add(b);
                }
            if (1 < l.Count)
                l.Insert(1, BitConverter.GetBytes(n));
            else
                l.Add(BitConverter.GetBytes(n));
            return l;
        }

        public bool ReadByte1(byte[] buf, ref int offs, ref List<ExamineeA> v)
        {
            if (buf == null)
                return true;
            int l = buf.Length - offs;
            if (l < 4)
                return true;
            int n = BitConverter.ToInt32(buf, offs);
            l -= 4;
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
                    //o.bFromC = false;
                    o.Merge(e);
                }
                else
                    v.Add(e);
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
