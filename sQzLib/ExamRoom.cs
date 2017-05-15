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
        public Dictionary<int, Examinee> vExaminee;
        public ExamRoom()
        {
            uId = ushort.MaxValue;
            vExaminee = new Dictionary<int, Examinee>();
        }

        public Dictionary<int, ushort>  DBSelectId(uint slId, out Dictionary<int, string> vAns)
        {
            vAns = new Dictionary<int, string>();
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return null;
            string qry = DBConnect.mkQrySelect("examinee", "lv,id,qId,anssh", "slId=" + slId, null);
            MySqlDataReader reader = DBConnect.exeQrySelect(conn, qry);
            Dictionary<int, ushort> r = new Dictionary<int, ushort>();
            if (reader != null)
            {
                while (reader.Read())
                {
                    int id = reader.GetInt16(0) * reader.GetUInt16(1);
                    if(!reader.IsDBNull(2))
                        r.Add(id, reader.GetUInt16(2));
                    if (!reader.IsDBNull(3))
                        vAns.Add(id, reader.GetString(3));
                }
                reader.Close();
            }
            DBConnect.Close(ref conn);
            return r;
        }

        public void DBInsert()
        {
            if (vExaminee == null || vExaminee.Count < 1)
                return;
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return;
            string attbs = "slId,lv,id,name,birdate,birthplace";
            StringBuilder vals = new StringBuilder();
            foreach (Examinee e in vExaminee.Values)
            {
                vals.Append("(" + e.uSlId + ",");
                vals.Append(e.Lv + ",");
                vals.Append(e.uId + ",");
                vals.Append("'" + e.tName + "',");
                vals.Append("'" + ExamSlot.ToMysqlForm(e.tBirdate, ExamSlot.FORM_R) + "',");
                vals.Append("'" + e.tBirthplace + "'),");
            }
            vals.Remove(vals.Length - 1, 1);//remove the last comma
            DBConnect.Ins(conn, Examinee.tDBtbl + uId, attbs, vals.ToString());
            DBConnect.Close(ref conn);
        }

        public byte[] ToByte()
        {
            List<byte[]> l = new List<byte[]>();
            byte[] b = BitConverter.GetBytes(vExaminee.Count);
            l.Add(b);
            foreach (Examinee e in vExaminee.Values)
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
        public void ReadByte(byte[] buf, ref int offs)
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
                Examinee e = new Examinee();
                if(!e.ReadByte(buf, ref offs))
                    vExaminee.Add((short)(e.Lv * e.uId), e);
            }
        }

        public void DBUpdateRs(MySqlConnection conn)
        {
            foreach (Examinee e in vExaminee.Values)
            {
                StringBuilder qry = new StringBuilder("UPDATE exnee" + uId + " SET ");
                if (e.dtTim1.Hour != 0)
                {
                    qry.Append("t1='" + e.dtTim1.ToString("HH:mm") + "'");
                    if (e.dtTim2.Hour != 0)
                    {
                        qry.Append(",t2='" + e.dtTim2.ToString("HH:mm") + "'");
                        if (e.uGrade != short.MaxValue)
                            qry.Append(",grd=" + e.uGrade);
                        if (e.tComp != null)
                        {
                            if (32 < e.tComp.Length)
                                e.tComp = e.tComp.Substring(0, 32);//todo: more responsive
                            qry.Append(",comp='" + e.tComp + "'");
                        }
                        if (e.mAnsSh != null)
                        {
                            qry.Append(",qId=" + e.mAnsSh.uQSId);
                            if (e.mAnsSh.aAns != null)
                            {
                                qry.Append(",anssh='");
                                foreach (byte i in e.mAnsSh.aAns)
                                    qry.Append(i);
                                qry.Append("'");
                            }
                        }
                    }
                    qry.Append(" WHERE slId=" + e.uSlId +
                        " AND lv=" + (int)e.eLvl + " AND id=" + e.uId);
                    DBConnect.Update(conn, qry.ToString());
                }
            }
        }

        public Examinee ReadByteSgning(byte[] buf, int offs)
        {
            Examinee e = new Examinee();
            e.ReadByte(buf, ref offs);
            Examinee o;
            short key = (short)(e.Lv * e.uId);
            if (vExaminee.TryGetValue(key, out o) && o.tBirdate == e.tBirdate)
            {
                //vExaminee.Remove(key);
                //vExaminee.Add(key, e);
                o.Merge(e);
                if (o.eStt < Examinee.eAUTHENTICATED)
                    o.eStt = Examinee.eAUTHENTICATED;
                return o;
            }
            return null;
        }

        public Examinee Signing(Examinee e)
        {
            Examinee o;
            short key = (short)(e.Lv * e.uId);
            if (vExaminee.TryGetValue(key, out o) && o.tBirdate == e.tBirdate)
            {
                o.Merge(e);
                if (o.eStt < Examinee.eAUTHENTICATED)
                    o.eStt = Examinee.eAUTHENTICATED;
                return o;
            }
            return null;
        }

        public void ToByteGrade(byte[] prefix, out byte[] buf)
        {
            if (vExaminee.Count == 0)
            {
                buf = prefix;
                return;
            }
            List<byte[]> l = new List<byte[]>();
            int n = 0;
            foreach (Examinee e in vExaminee.Values)
                if (e.uGrade != ushort.MaxValue)
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

        public bool ReadByteGrade(byte[] buf, ref int offs, ref List<Examinee> v)
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
                Examinee e = new Examinee();
                if (e.ReadByte(buf, ref offs))
                    return true;
                Examinee o;
                if (vExaminee.TryGetValue(e.Lv * e.uId, out o))
                    o.Merge(e);
                else
                    v.Add(e);
            }
            return false;
        }
    }
}
