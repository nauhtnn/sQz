using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

/*
CREATE TABLE IF NOT EXISTS `examinee` (`dateIdx` INT(4) UNSIGNED, `level` SMALLINT(2),
`idx` SMALLINT(2) UNSIGNED, `name` VARCHAR(64) CHARACTER SET `utf8`,
`birthdate` DATE, `birthplace` VARCHAR(96) CHARACTER SET `utf8`,
`dt1` TIME, `dt2` TIME, `mark` SMALLINT(2) UNSIGNED, 
PRIMARY KEY(`dateIdx`, `level`, `idx`), FOREIGN KEY(`dateIdx`) REFERENCES date(`idx`));
*/

namespace sQzLib
{
    public class Examinee
    {
        public uint uDtId;
        public ExamLvl eLvl;
        public ushort uId;
        public string tName;
        public string tBirdate;
        public string tBirthplace;
        public ushort uGrade;

        public string tComp;
        public DateTime dtTim1;
		public DateTime dtTim2;

        public Examinee() {
            uGrade = ushort.MaxValue;
            dtTim1 = dtTim2 = DateTime.Parse("2016/01/01");
        }
        public string tId {
            get {
                if (eLvl == ExamLvl.Basis)
                    return "CB" + uId;
                else
                    return "NC" + uId;
            }
        }
        public bool ParseTxId(string s)
        {
            if(s == null || s.Length < 3)
                return true;
            if (s[0] == 'C' && s[1] == 'B')
            {
                if (ushort.TryParse(s.Substring(2), out uId))
                {
                    eLvl = ExamLvl.Basis;
                    return false;
                }
                else
                    return true;
            }
            else if (s[0] == 'N' && s[1] == 'C')
            {
                if (ushort.TryParse(s.Substring(2), out uId))
                {
                    eLvl = ExamLvl.Advance;
                    return false;
                }
                else
                    return true;
            }
            return true;
        }
        public short Lvl
        {
            get { return (short)eLvl; }
            set { eLvl = (ExamLvl)value; }
        }
        public override string ToString()
        {
            StringBuilder s = new StringBuilder();
            if (eLvl == ExamLvl.Basis)
                s.Append("CB");
            else
                s.Append("NC");
            s.AppendFormat("{0}, {1}, {2}, ", uId, tName, tBirdate);
            s.Append(tBirthplace);
            return s.ToString();
        }

        public void ToByteSgning(out byte[] buf, int state)
        {
            if (tBirdate == null || tBirdate.Length != 10)
            {
                buf = null;
                return;
            }
            byte[] a = Encoding.UTF8.GetBytes(tBirdate);
            byte[] b = null;
            try {
               b = Encoding.UTF8.GetBytes(Environment.MachineName);
            } catch(InvalidOperationException) { b = null; }
            ExamLvl lv = ExamLvl.Basis;
            if (b == null)
                buf = new byte[12 + a.Length];
            else
                buf = new byte[12 + a.Length + 4 + b.Length];
            int offs = 0;
            Buffer.BlockCopy(BitConverter.GetBytes(state), 0, buf, offs, 4);
            offs += 4;
            Buffer.BlockCopy(BitConverter.GetBytes((short)lv), 0, buf, offs, 2);
            offs += 2;
            Buffer.BlockCopy(BitConverter.GetBytes(uId), 0, buf, offs, 2);
            offs += 2;
            Buffer.BlockCopy(BitConverter.GetBytes(a.Length), 0, buf, offs, 4);
            offs += 4;
            Buffer.BlockCopy(a, 0, buf, offs, a.Length);
            offs += a.Length;
            if (b != null)
            {
                Buffer.BlockCopy(BitConverter.GetBytes(b.Length), 0, buf, offs, 4);
                offs += 4;
                Buffer.BlockCopy(b, 0, buf, offs, b.Length);
            }
        }
        
        public bool ReadByteSgning(byte[] buf, ref int offs)
        {
            int l = buf.Length - offs;
            if (l < 2)
                return false;
            Lvl = BitConverter.ToInt16(buf, offs);
            l -= 2;
            offs += 2;
            if (l < 2)
                return false;
            uId = BitConverter.ToUInt16(buf, offs);
            l -= 2;
            offs += 2;
            if (l < 4)
                return false;
            int sz = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            if (l < sz)
                return false;
            tName = Encoding.UTF8.GetString(buf, offs, sz);
            l -= sz;
            offs += sz;
            if (l < 4)
                return false;
            sz = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            if (l < sz)
                return false;
            tBirdate = Encoding.UTF8.GetString(buf, offs, sz);
            l -= sz;
            offs += sz;
            if (l < 4)
                return false;
            sz = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            if (l < sz)
                return false;
            tBirthplace = Encoding.UTF8.GetString(buf, offs, sz);

            l -= sz;
            offs += sz;
            if (l < 4)
                return false;
            sz = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            if (l < sz)
                return false;
            DateTime.TryParse(Encoding.UTF8.GetString(buf, offs, sz), out dtTim1);

            l -= sz;
            offs += sz;
            if (l < 4)
                return false;
            sz = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            if (l < sz)
                return false;
            tComp = Encoding.UTF8.GetString(buf, offs, sz);
            return true;
        }

        public List<byte[]> ToByte()
        {
            List<byte[]> l = new List<byte[]>();
            byte[] b = BitConverter.GetBytes(Lvl);
            l.Add(b);
            b = BitConverter.GetBytes(uId);
            l.Add(b);
            b = Encoding.UTF8.GetBytes(tName);
            l.Add(BitConverter.GetBytes(b.Length));
            l.Add(b);
            b = Encoding.UTF8.GetBytes(tBirdate);
            l.Add(BitConverter.GetBytes(b.Length));
            l.Add(b);
            b = Encoding.UTF8.GetBytes(tBirthplace);
            l.Add(BitConverter.GetBytes(b.Length));
            l.Add(b);
            l.Add(BitConverter.GetBytes(dtTim1.Hour));
            l.Add(BitConverter.GetBytes(dtTim1.Minute));
            l.Add(BitConverter.GetBytes(dtTim2.Hour));
            l.Add(BitConverter.GetBytes(dtTim2.Minute));
            l.Add(BitConverter.GetBytes(uGrade));
            return l;
        }

        public void ToByte(out byte[] buf)
        {
            List<byte[]> l = ToByte();
            int sz = 0;
            foreach (byte[] i in l)
                sz += i.Length;
            buf = new byte[sz];
            sz = 0;
            foreach (byte[] i in l)
            {
                Buffer.BlockCopy(i, 0, buf, sz, i.Length);
                sz += i.Length;
            }
        }

        public bool ReadByte(byte[] buf, ref int offs)
        {
            int l = buf.Length - offs;
            if (l < 2)      
                return true;
            Lvl = BitConverter.ToInt16(buf, offs);
            l -= 2;
            offs += 2;
            if (l < 2)
                return true;
            uId = BitConverter.ToUInt16(buf, offs);
            l -= 2;
            offs += 2;
            if (l < 4)
                return true;
            int sz = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            if (l < sz)
                return true;
            tName = Encoding.UTF8.GetString(buf, offs, sz);
            l -= sz;
            offs += sz;
            if (l < 4)
                return true;
            sz = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            if (l < sz)
                return true;
            tBirdate = Encoding.UTF8.GetString(buf, offs, sz);
            l -= sz;
            offs += sz;
            if (l < 4)
                return true;
            sz = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            if (l < sz)
                return true;
            tBirthplace = Encoding.UTF8.GetString(buf, offs, sz);
            l -= sz;
            offs += sz;
            if (l < 4)
                return true;
            int h = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            if (l < 4)
                return true;
            int m = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            if (!DateTime.TryParse(h.ToString("d2") + ':' + m.ToString("d2"), out dtTim1))
                dtTim1 = DateTime.Parse("00:00");
            if (l < 4)
                return true;
            h = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            if (l < 4)
                return true;
            m = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            if (!DateTime.TryParse(h.ToString("d2") + ':' + m.ToString("d2"), out dtTim2))
                dtTim2 = DateTime.Parse("00:00");
            if (l < 2)
                return true;
            uGrade = BitConverter.ToUInt16(buf, offs);
            l -= 2;
            offs += 2;
            return false;
        }
    }
}
