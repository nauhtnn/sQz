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
                    Examinee s = new Examinee();
                    s.uDtId = dateIdx;
                    s.Lvl = reader.GetInt16(1);
                    s.uId = reader.GetUInt16(2);
                    s.tName = reader.GetString(3);
                    s.tBirdate = reader.GetDateTime(4).ToString(ExamDate.FORM);
                    s.tBirthplace = reader.GetString(5);
                    if (reader.IsDBNull(6) || !DateTime.TryParse(reader.GetString(6), out s.dtTim1))
                        s.dtTim1 = DateTime.Parse("00:00");
                    if (reader.IsDBNull(7) || !DateTime.TryParse(reader.GetString(7), out s.dtTim2))
                        s.dtTim2 = DateTime.Parse("00:00");
                    if (!reader.IsDBNull(8))
                        s.uGrade = reader.GetUInt16(8);
                    vExaminee.Add((short)(s.Lvl * s.uId), s);
                }
                reader.Close();
            }
            DBConnect.Close(ref conn);
            ToByte();
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
                vals.Append("'" + ExamDate.ToMysqlForm(e.tBirdate, ExamDate.FORM) + "',");
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

        public void UpdateRs()
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
                            qry.Append(",mark=" + e.uGrade);
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
            int l = buf.Length - offs;
            if (l < 2)
                return null;
            short lv = BitConverter.ToInt16(buf, offs);
            l -= 2;
            offs += 2;
            if (l < 2)
                return null;
            ushort id = BitConverter.ToUInt16(buf, offs);
            l -= 2;
            offs += 2;
            if (l < 4)
                return null;
            int sz = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            if (l < sz)
                return null;
            string date = Encoding.UTF8.GetString(buf, offs, sz);
            l -= sz;
            offs += sz;
            short key = (short)(lv * id);
            Examinee e;
            if(vExaminee.TryGetValue(key, out e) && e.tBirdate == date)
            {
                if (4 < l)
                    e.tComp = Encoding.UTF8.GetString(buf, offs + 4, l - 4);
                return e;
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
            {
                l.Add(BitConverter.GetBytes(e.Lvl));
                l.Add(BitConverter.GetBytes(e.uId));
                l.Add(BitConverter.GetBytes(e.dtTim1.Hour));
                l.Add(BitConverter.GetBytes(e.dtTim1.Minute));
                l.Add(BitConverter.GetBytes(e.dtTim2.Hour));
                l.Add(BitConverter.GetBytes(e.dtTim2.Minute));
                l.Add(BitConverter.GetBytes(e.uGrade));
            }
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
                if (l < 2)
                    return;
                short lv = BitConverter.ToInt16(buf, offs);
                l -= 2;
                offs += 2;
                if (l < 2)
                    return;
                ushort id = BitConverter.ToUInt16(buf, offs);
                l -= 2;
                offs += 2;
                Examinee e;
                if (!vExaminee.TryGetValue((short)(lv * id), out e))
                    continue;
                if (l < 4)
                    return;
                int h = BitConverter.ToInt32(buf, offs);
                l -= 4;
                offs += 4;
                if (l < 4)
                    return;
                int m = BitConverter.ToInt32(buf, offs);
                l -= 4;
                offs += 4;
                string t = "" + h.ToString("d2") + ":" + m.ToString("d2");
                if (!DateTime.TryParse(t, out e.dtTim1))
                    e.dtTim1 = DateTime.Parse("00:00");
                if (l < 4)
                    return;
                h = BitConverter.ToInt32(buf, offs);
                l -= 4;
                offs += 4;
                if (l < 4)
                    return;
                m = BitConverter.ToInt32(buf, offs);
                l -= 4;
                offs += 4;
                if (!DateTime.TryParse(h.ToString("d2") + ':' + m.ToString("d2"), out e.dtTim2))
                    e.dtTim2 = DateTime.Parse("00:00");
                if (l < 2)
                    return;
                ushort mark = BitConverter.ToUInt16(buf, offs);
                l -= 2;
                offs += 2;
                e.uGrade = mark;
            }
        }
    }
}
