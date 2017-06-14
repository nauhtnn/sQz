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

        public byte[] ToByteS1()
        {
            List<byte[]> l = new List<byte[]>();
            byte[] b = BitConverter.GetBytes(vExaminee.Count);
            l.Add(b);
            foreach (ExamineeA e in vExaminee.Values)
                foreach (byte[] i in e.ToByte())
                    l.Add(i);
            //join
            int sz = 0;
            foreach (byte[] i in l)
                sz += i.Length;
            b = new byte[sz];
            int offs = 0;
            foreach (byte[] i in l)
            {
                Buffer.BlockCopy(i, 0, b, offs, i.Length);
                offs += i.Length;
            }
            return b;
        }
        public void ReadByteS1(byte[] buf, ref int offs)
        {
            vExaminee.Clear();
            if (buf == null)
                return;
            int l = buf.Length - offs;
            if (l < 4)
                return;
            int n = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            for (int i = 0; i < n; ++i)
            {
                ExamineeS1 e = new ExamineeS1();
                //e.bFromC = false;
                if (!e.ReadByte(buf, ref offs))
                {
                    if (e.eStt == ExamStt.Info)
                    {
                        if (!vExaminee.ContainsKey(e.uId))
                            vExaminee.Add(e.uId, e);
                    }
                    else
                    {
                        ExamineeA o;
                        if (vExaminee.TryGetValue(e.uId, out o))
                        {
                            o.bFromC = false;
                            o.Merge(e);
                        }
                        else
                            vExaminee.Add(e.uId, e);
                    }
                }
            }
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

        public void ToByteS0(byte[] prefix, out byte[] buf)
        {
            //if (vExaminee.Count == 0)
            //{
            //    buf = prefix;
            //    return;
            //}
            List<byte[]> l = new List<byte[]>();
            int n = 0;
            foreach (ExamineeA e in vExaminee.Values)
                if (e.eStt == ExamStt.Finished)
                {
                    ++n;
                    foreach (byte[] b in e.ToByte())
                        l.Add(b);
                }
            //join
            int sz = 4;//sizeof(n)
            if (prefix != null)
                sz += prefix.Length;
            foreach (byte[] i in l)
                sz += i.Length;
            buf = new byte[sz];
            int offs = 0;
            if (prefix != null)
            {
                Buffer.BlockCopy(prefix, 0, buf, offs, prefix.Length);
                offs += prefix.Length;
            }
            Array.Copy(BitConverter.GetBytes(n), 0, buf, offs, 4);
            offs += 4;
            foreach (byte[] i in l)
            {
                Buffer.BlockCopy(i, 0, buf, offs, i.Length);
                offs += i.Length;
            }
        }

        public bool ReadByteS0(byte[] buf, ref int offs, ref List<ExamineeA> v)
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
            return r;
        }
    }
}
