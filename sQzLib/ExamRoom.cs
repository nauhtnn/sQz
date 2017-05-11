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
        public Dictionary<short, Examinee> vExaminee;
        public ExamRoom()
        {
            vExaminee = new Dictionary<short, Examinee>();
        }

        public void ReadTxt(string buf)
        {
            if (buf == null)
                return;
            vExaminee.Clear();
            string[] vs = buf.Split('\n');
            foreach (string s in vs)
            {
                Examinee nee = new Examinee();
                string[] v = s.Split('\t');
                if (v.Length == 4) //todo: hardcode, unsafe
                {
                    if (v[0].Length < 3)
                        continue;
                    if (v[0][0] == 'C' && v[0][1] == 'B')
                        nee.eLvl = ExamLvl.Basis;
                    else if (v[0][0] == 'N' && v[0][1] == 'C')
                        nee.eLvl = ExamLvl.Advance;
                    else
                        continue;
                    if (ushort.TryParse(v[0].Substring(2), out nee.uId))
                    {
                        nee.tName = v[1];
                        nee.tBirdate = v[2];
                        nee.tBirthplace = v[3];
                        vExaminee.Add((short)(nee.Lvl * nee.uId), nee);
                    }
                }
            }
        }

        public void DBSelect(uint dateIdx)
        {
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return;
            string qry = DBConnect.mkQrySelect("examinee", null, "dateIdx=" + dateIdx, null);
            MySqlDataReader reader = DBConnect.exeQrySelect(conn, qry);
            vExaminee.Clear();
            if (reader != null)
            {
                while (reader.Read())
                {
                    Examinee e = new Examinee();
                    e.eStt = Examinee.eINFO;
                    e.uDtId = dateIdx;//reader.GetUInt16(0)
                    e.Lvl = reader.GetInt16(1);
                    e.uId = reader.GetUInt16(2);
                    e.tName = reader.GetString(3);
                    e.tBirdate = reader.GetDateTime(4).ToString(ExamDate.FORM_R);
                    e.tBirthplace = reader.GetString(5);
                    if (reader.IsDBNull(6))
                        e.dtTim1 = ExamDate.INVALID_DT;
                    else
                        DateTime.TryParse(reader.GetString(6), out e.dtTim1);
                    if (reader.IsDBNull(7))
                        e.dtTim2 = ExamDate.INVALID_DT;
                    else
                    {
                        e.eStt = Examinee.eFINISHED;
                        DateTime.TryParse(reader.GetString(7), out e.dtTim2);
                    }
                    if (!reader.IsDBNull(8))
                        e.uGrade = reader.GetUInt16(8);
                    if (!reader.IsDBNull(9))
                        e.tComp = reader.GetString(9);
                    vExaminee.Add((short)(e.Lvl * e.uId), e);
                }
                reader.Close();
            }
            DBConnect.Close(ref conn);
        }

        public Dictionary<int, ushort>  DBSelectId(uint dateIdx, out Dictionary<int, string> vAns)
        {
            vAns = new Dictionary<int, string>();
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return null;
            string qry = DBConnect.mkQrySelect("examinee", "level,idx,questIdx,anssh", "dateIdx=" + dateIdx, null);
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

        public void DBInsert(uint dateIdx)
        {
            if (vExaminee == null || vExaminee.Count < 1)
                return;
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return;
            string attbs = "dateIdx,level,idx,name,birthdate,birthplace";
            StringBuilder vals = new StringBuilder();
            foreach (Examinee e in vExaminee.Values)
            {
                vals.Append("(" + dateIdx + ",");
                vals.Append(e.Lvl + ",");
                vals.Append(e.uId + ",");
                vals.Append("'" + e.tName + "',");
                vals.Append("'" + ExamDate.ToMysqlForm(e.tBirdate, ExamDate.FORM_R) + "',");
                vals.Append("'" + e.tBirthplace + "'),");
            }
            vals.Remove(vals.Length - 1, 1);//remove the last comma
            DBConnect.Ins(conn, "examinee", attbs, vals.ToString());
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
                    vExaminee.Add((short)(e.Lvl * e.uId), e);
            }
        }

        public void DBUpdateRs()
        {
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return;
            foreach (Examinee e in vExaminee.Values)
            {
                StringBuilder qry = new StringBuilder("UPDATE `examinee` SET ");
                if (e.dtTim1.Hour != 0)
                {
                    qry.Append("dt1='" + e.dtTim1.ToString("HH:mm") + "'");
                    if (e.dtTim2.Hour != 0)
                    {
                        qry.Append(",dt2='" + e.dtTim2.ToString("HH:mm") + "'");
                        if (e.uGrade != short.MaxValue)
                            qry.Append(",grade=" + e.uGrade);
                        if (e.tComp != null)
                        {
                            if (32 < e.tComp.Length)
                                e.tComp = e.tComp.Substring(0, 32);//todo: more responsive
                            qry.Append(",comp='" + e.tComp + "'");
                        }
                        if (e.mAnsSh != null)
                        {
                            qry.Append(",questIdx=" + e.mAnsSh.uQSId);
                            if (e.mAnsSh.aAns != null)
                            {
                                qry.Append(",anssh='");
                                foreach (byte i in e.mAnsSh.aAns)
                                    qry.Append(i);
                                qry.Append("'");
                            }
                        }
                    }
                    qry.Append(" WHERE dateIdx=" + e.uDtId +
                        " AND level=" + (int)e.eLvl + " AND idx=" + e.uId);
                    DBConnect.Update(conn, qry.ToString());
                }
            }
            DBConnect.Close(ref conn);
        }

        public Examinee ReadByteSgning(byte[] buf, int offs)
        {
            Examinee e = new Examinee();
            e.ReadByte(buf, ref offs);
            Examinee o;
            short key = (short)(e.Lvl * e.uId);
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
            short key = (short)(e.Lvl * e.uId);
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
            l.Add(BitConverter.GetBytes(vExaminee.Count));
            foreach (Examinee e in vExaminee.Values)
                foreach (byte[] b in e.ToByte())
                    l.Add(b);
            //join
            int sz = 0;
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
            foreach (byte[] i in l)
            {
                Buffer.BlockCopy(i, 0, buf, offs, i.Length);
                offs += i.Length;
            }
        }

        public void ReadByteGrade(byte[] buf, ref int offs)
        {
            if (buf == null || vExaminee.Count < 1)
                return;
            int l = buf.Length - offs;
            if (l < 4)
                return;
            int n = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            while (0 < n)
            {
                --n;
                Examinee e = new Examinee();
                if (e.ReadByte(buf, ref offs))
                    break;
                Examinee o;
                if (!vExaminee.TryGetValue((short)(e.Lvl * e.uId), out o))
                    continue;
                else
                    o.Merge(e);
            }
        }
    }
}
