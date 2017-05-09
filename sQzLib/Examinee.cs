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
`dt1` TIME, `dt2` TIME, `grade` SMALLINT(2) UNSIGNED, `comp` VARCHAR(32),
`questIdx` SMALLINT(2) UNSIGNED, `anssh` CHAR(120) CHARACTER SET `utf8`,
PRIMARY KEY(`dateIdx`, `level`, `idx`), FOREIGN KEY(`dateIdx`) REFERENCES date(`idx`));

FOREIGN KEY(`questIdx`) REFERENCES questsh(`idx`));
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
        public AnsSheet mAnsSh;

        public const int eSIGNING = 0;
        public const int eINFO = 1;
        public const int eAUTHENTICATED = 2;
        public const int eEXAMING = 3;
        public const int eSUBMITTING = 4;
        public const int eFINISHED = 5;
        public int eStt;

        public TimeSpan kDtDuration;

        public StringBuilder tLog;

        const string tLOG_DIR = "sQz\\";
        const string tLOG_PRE = "sav";

        public Examinee() {
            Reset();
        }
        public void Reset()
        {
            tBirdate = null;
            tBirthplace = null;
            tName = null;
            uDtId = uint.MaxValue;
            uId = ushort.MaxValue;
            eStt = eSIGNING;
            uGrade = ushort.MaxValue;
            dtTim1 = dtTim2 = ExamDate.INVALID_DT;
            tComp = string.Empty;
            mAnsSh = new AnsSheet();
            kDtDuration = new TimeSpan(0, 30, 0);
            tLog = new StringBuilder();
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
            ushort tuId;
            if (s[0] == 'C' && s[1] == 'B')
            {
                if (ushort.TryParse(s.Substring(2), out tuId))
                {
                    if (eLvl != ExamLvl.Basis || uId != tuId)
                        Reset();
                    eLvl = ExamLvl.Basis;
                    uId = tuId;
                    return false;
                }
                else
                    return true;
            }
            else if (s[0] == 'N' && s[1] == 'C')
            {
                if (ushort.TryParse(s.Substring(2), out tuId))
                {
                    if (eLvl != ExamLvl.Advance || uId != tuId)
                        Reset();
                    uId = tuId;
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

        public List<byte[]> ToByte()
        {
            List<byte[]> l = new List<byte[]>();
            l.Add(BitConverter.GetBytes(uDtId));
            l.Add(BitConverter.GetBytes(Lvl));
            l.Add(BitConverter.GetBytes(uId));
            byte[] b = Encoding.UTF8.GetBytes(tBirdate);
            l.Add(BitConverter.GetBytes(b.Length));
            l.Add(b);
            b = Encoding.UTF8.GetBytes(tComp);
            l.Add(BitConverter.GetBytes(b.Length));
            if(0 < b.Length)
                l.Add(b);
            l.Add(BitConverter.GetBytes(eStt));
            if (eStt == eSIGNING)
                return l;
            b = Encoding.UTF8.GetBytes(tName);
            l.Add(BitConverter.GetBytes(b.Length));
            l.Add(b);
            b = Encoding.UTF8.GetBytes(tBirthplace);
            l.Add(BitConverter.GetBytes(b.Length));
            l.Add(b);
            if (eStt == eINFO)
                return l;
            l.Add(BitConverter.GetBytes(dtTim1.Hour));
            l.Add(BitConverter.GetBytes(dtTim1.Minute));
            if (eStt == eAUTHENTICATED)
                return l;
            l.Add(BitConverter.GetBytes(mAnsSh.uQSId));
            if (eStt == eEXAMING)
                return l;
            if (mAnsSh.aAns == null)
                l.Add(BitConverter.GetBytes(false));
            else
            {
                l.Add(BitConverter.GetBytes(true));
                l.Add(mAnsSh.aAns);
            }
            if (eStt == eSUBMITTING)
                return l;
            l.Add(BitConverter.GetBytes(dtTim2.Hour));
            l.Add(BitConverter.GetBytes(dtTim2.Minute));
            l.Add(BitConverter.GetBytes(uGrade));
            return l;
        }

        public void ToByte(out byte[] buf, int prfx)
        {
            List<byte[]> l = ToByte();
            int sz = 4;
            foreach (byte[] i in l)
                sz += i.Length;
            buf = new byte[sz];
            sz = 0;
            Buffer.BlockCopy(BitConverter.GetBytes(prfx), 0, buf, sz, 4);
            sz += 4;
            foreach (byte[] i in l)
            {
                Buffer.BlockCopy(i, 0, buf, sz, i.Length);
                sz += i.Length;
            }
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
            if (l < 4)
                return true;
            uDtId = BitConverter.ToUInt32(buf, offs);
            l -= 4;
            offs += 4;
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
            if (0 < sz)
            {
                tBirdate = Encoding.UTF8.GetString(buf, offs, sz);
                l -= sz;
                offs += sz;
            }
            if (l < 4)
                return true;
            sz = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            if (l < sz)
                return true;
            if (0 < sz)
            {
                tComp = Encoding.UTF8.GetString(buf, offs, sz);
                l -= sz;
                offs += sz;
            }
            if (l < 4)
                return true;
            eStt = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            if (eStt == eSIGNING)
                return false;
            if (l < 4)
                return true;
            sz = BitConverter.ToInt32(buf, offs);
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
            tBirthplace = Encoding.UTF8.GetString(buf, offs, sz);
            l -= sz;
            offs += sz;
            if (eStt == eINFO)
                return false;
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
            if (!DateTime.TryParse(h.ToString() + ':' + m, out dtTim1))
                dtTim1 = ExamDate.INVALID_DT;
            if (eStt == eAUTHENTICATED)
                return false;
            if (l < 4)
                return true;
            mAnsSh.uQSId = BitConverter.ToUInt16(buf, offs);
            l -= 2;
            offs += 2;
            if (eStt == eEXAMING)
                return false;
            if (l < 1)
                return true;
            if (BitConverter.ToBoolean(buf, offs))
            {
                l -= 1;
                offs += 1;
                if (l < 120)
                    return true;
                mAnsSh.aAns = new byte[120];//hardcode
                Buffer.BlockCopy(buf, offs, mAnsSh.aAns, 0, 120);
                l -= 120;
                offs += 120;
            }
            else
            {
                l -= 1;
                offs += 1;
            }
            if (eStt == eSUBMITTING)
                return false;
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
            if (!DateTime.TryParse(h.ToString() + ':' + m, out dtTim2))
                dtTim2 = ExamDate.INVALID_DT;
            if (l < 2)
                return true;
            uGrade = BitConverter.ToUInt16(buf, offs);
            //l -= 2;
            offs += 2;
            return false;
        }

        public void Merge(Examinee e)
        {
            if (e.eStt != eSIGNING && uDtId != e.uDtId || eStt == eFINISHED)
                return;
            if (Lvl != e.Lvl || uId != e.uId
                || tBirdate != e.tBirdate)
                return;
            tComp += e.tComp;
            if (e.eStt < eStt || e.eStt < eEXAMING)
                return;
            eStt = e.eStt;
            mAnsSh.uQSId = e.mAnsSh.uQSId;
            dtTim1 = e.dtTim1;
            if (e.eStt == eEXAMING)
                return;
            mAnsSh.aAns = e.mAnsSh.aAns;
            if (e.eStt == eSUBMITTING)
                return;
            dtTim2 = e.dtTim2;
            uGrade = e.uGrade;
        }

        public bool ToLogFile(int m, int s)
        {
            bool err = false;
            string p = null;
            try
            {
                p = System.IO.Path.Combine(Environment.GetFolderPath(
                    Environment.SpecialFolder.ApplicationData), tLOG_DIR);
                if (!System.IO.Directory.Exists(p))
                    System.IO.Directory.CreateDirectory(p);
            }
            catch (System.IO.DirectoryNotFoundException) { err = true; }
            catch(UnauthorizedAccessException) { err = true; }
            if (err)
                return true;
            var fileName = System.IO.Path.Combine(p, tLOG_PRE +
                (Lvl * uId) + '-' + m.ToString("d2") + s.ToString("d2"));
            System.IO.BinaryWriter w = null;
            try
            {
                w = new System.IO.BinaryWriter(System.IO.File.OpenWrite(fileName),
                    Encoding.UTF8);
            }
            catch (UnauthorizedAccessException) { err = true; }
            if (err)
                return true;
            w.Write(uDtId);
            w.Write(Lvl);
            w.Write(uId);
            w.Write(uGrade);
            w.Write(tComp.Length);
            if (0 < tComp.Length)
                w.Write(tComp);
            w.Write(dtTim1.Hour);
            w.Write(dtTim1.Minute);
            w.Write(dtTim2.Hour);
            w.Write(dtTim2.Minute);
            w.Write(m);
            w.Write(s);
            w.Write(mAnsSh.uQSId);
            w.Write(mAnsSh.aAns, 0, mAnsSh.aAns.Length);
            w.Close();
            mAnsSh.bChanged = false;
            return false;
        }

        public bool ReadLogFile(string filePath)
        {
            System.IO.BinaryReader r = null;
            if (System.IO.File.Exists(filePath))
                try
                {
                    r = new System.IO.BinaryReader(System.IO.File.OpenRead(filePath));
                }
                catch (UnauthorizedAccessException) { r = null; }
            if (r == null)
                return false;
            uDtId = r.ReadUInt32();
            eLvl = (ExamLvl)r.ReadInt16();
            uId = r.ReadUInt16();
            uGrade = r.ReadUInt16();
            if (0 < r.ReadInt32())
                tComp = r.ReadString();
            int h = r.ReadInt32();
            int m = r.ReadInt32();
            ExamDate.Parse(h.ToString() + ':' + m, ExamDate.FORM_h, out dtTim1);
            h = r.ReadInt32();
            m = r.ReadInt32();
            ExamDate.Parse(h.ToString() + ':' + m, ExamDate.FORM_h, out dtTim2);
            h = r.ReadInt32();
            m = r.ReadInt32();
            kDtDuration = new TimeSpan(0, h, m);
            mAnsSh.uQSId = r.ReadUInt16();
            mAnsSh.aAns = r.ReadBytes(120);//mAnsSh.aAns.Length//hardcode
            return true;
        }

        public void UpdateLogStr(string s)
        {
            tLog.Append(s);
        }
    }
}
