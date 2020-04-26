using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;

namespace sQzLib
{
    public enum NeeStt
    {
         Signing = 0,
         Info,
         Authenticated,
         Examing,
         Submitting,
         Finished
    }

    public abstract class ExamineeA
    {
        public DateTime mDt;
        public NeeStt eStt;
        public Level Lv;
        public int uId;
        public int LvId { get { return (Lv == Level.A) ? uId : uId + (int)Level.MAX_COUNT_EACH_LEVEL; } }
        public string tId { get { return Lv.ToString() + uId.ToString("d4"); } }
        public static string gId(Level lv, int id) { return lv.ToString() + id.ToString("d4"); }
        public string tName;
        public string tBirdate;
        public string tBirthplace;
        public int uGrade;

        public string tComp;
        public DateTime dtTim1;
        public DateTime dtTim2;
        public AnsSheet mAnsSh;

        public TimeSpan kDtDuration;

        public StringBuilder tLog;

        const string tLOG_DIR = "sQz\\";
        const string tLOG_PRE = "sav";

        public bool bFromC;//used by NeeS1
        public bool bLog;//used by NeeS1 and NeeC

        public ExamineeA() {
            bFromC = bLog = false;
        }

        void Reset()
        {
            mDt = DT.INV_H;
            Lv = Level.A;
            uId = (int)Level.MAX_COUNT_EACH_LEVEL;
            tName = null;
            tBirdate = null;
            tBirthplace = null;
            eStt = NeeStt.Signing;
            uGrade = (int)Level.MAX_COUNT_EACH_LEVEL;
            dtTim1 = dtTim2 = DT.INV_;
            tComp = string.Empty;
            mAnsSh = new AnsSheet();
            kDtDuration = new TimeSpan(0, 30, 0);
            tLog = new StringBuilder();
        }

        protected void ParseLvID(int lvid)
        {
            //if (lvid != Level.MAX_COUNT_EACH_LEVEL &&(lvid < 1 || Level.MAX_COUNT_EACH_LEVEL + Level.MAX_COUNT_EACH_LEVEL <= lvid))
            //    return true;
            //if(lvid < Level.MAX_COUNT_EACH_LEVEL)
            //{
            //    eLv = Level.A;
            //    uId = lvid;
            //}
            //else
            //{
            //    eLv = Level.B;
            //    uId = lvid - Level.MAX_COUNT_EACH_LEVEL;
            //}
            //return false;
        }

        protected void ParseLvID(string s)
        {
            if (s == null || s.Length != 5)
                throw new ArgumentException();
            s = s.ToUpper();
            Level lv;
            if (!Enum.TryParse(s.Substring(0, 1), out lv))
                throw new ArgumentException();
            int uid;
            if (!int.TryParse(s.Substring(1), out uid))
                throw new ArgumentException();
            if (uid < 1 || (int)Level.MAX_COUNT_EACH_LEVEL <= uid)
                throw new ArgumentException();
            Reset();
            Lv = lv;
            uId = uid;
        }

        public override string ToString()
        {
            StringBuilder s = new StringBuilder();
            s.AppendFormat("{0}, {1}, {2}, {3}",
                tId, tName, tBirdate, tBirthplace);
            return s.ToString();
        }

        public int DBGetQSId()
        {
            MySqlDataReader reader = DBConnect.exeQrySelect("sqz_nee_qsheet", "qsid",
                "dt='" + mDt.ToString(DT._) +
                "' AND lv='" + Lv.ToString() +
                "' AND neeid=" + uId);
            if (reader.Read())
            {
                reader.Close();
                return reader.GetInt32(0);
            }
            return -1;
        }

        public char[] DBGetAns()
        {
            MySqlDataReader reader = DBConnect.exeQrySelect("sqz_nee_qsheet", "ans",
                "dt='" + mDt.ToString(DT._) + "' AND lv='" + Lv.ToString() +
                "' AND neeid=" + uId);
            if (reader.Read())
            {
                reader.Close();
                return reader.GetString(0).ToCharArray();
            }
            char[] noans = new char[AnsSheet.LEN];
            for (int i = 0; i < AnsSheet.LEN; ++i)
                noans[i] = MultiChoiceItem.C0;
            return noans;
        }

        public bool DBSelGrade()
        {
            MySqlDataReader reader = DBConnect.exeQrySelect("sqz_nee_qsheet", "grade",
                "dt='" + mDt.ToString(DT._) + "' AND lv='" + Lv.ToString() +
                "' AND neeid=" + uId);
            if (reader.Read())
            {
                reader.Close();
                uGrade = reader.GetInt16(0);
                return true;
            }
            return false;
        }

        public string DBGetT()
        {
            MySqlDataReader reader = DBConnect.exeQrySelect("sqz_examinee", "t",
                "dt='" + mDt.ToString(DT._) + "' AND lv='" + Lv.ToString() +
                "' AND id=" + uId);
            if (reader.Read())
            {
                reader.Close();
                return reader.GetString(0);
            }
            return DT.INV_H.ToString(DT.hh);
        }

        public abstract List<byte[]> ToByte();

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

        public abstract bool ReadByte(byte[] buf, ref int offs);

        public abstract void Merge(ExamineeA e);

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
                tId + '-' + m.ToString("d2") + s.ToString("d2"));
            System.IO.BinaryWriter w = null;
            try
            {
                w = new System.IO.BinaryWriter(System.IO.File.OpenWrite(fileName),
                    Encoding.UTF8);
            }
            catch (UnauthorizedAccessException) { err = true; }
            if (err)
                return true;
            w.Write((int)Lv);
            w.Write(uId);
            w.Write((int)eStt);
            w.Write(mAnsSh.uQSId);
            w.Write(mAnsSh.aAns, 0, AnsSheet.LEN);
            if (eStt == NeeStt.Finished)
            {
                w.Write(dtTim1.Hour);
                w.Write(dtTim1.Minute);
                w.Write(dtTim2.Hour);
                w.Write(dtTim2.Minute);
            }
            else
            {
                w.Write(m);
                w.Write(s);
            }
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
            //uSlId = r.ReadUInt32();
            int x;
            if (Enum.IsDefined(typeof(Level), x = r.ReadInt32()))
                Lv = (Level)x;
            uId = r.ReadInt32();
            if (Enum.IsDefined(typeof(NeeStt), x = r.ReadInt32()))
                eStt = (NeeStt)x;
            mAnsSh.uQSLvId = r.ReadInt32();
            mAnsSh.aAns = r.ReadBytes(AnsSheet.LEN);
            int h, m;
            if(eStt == NeeStt.Finished)
            {
                h = r.ReadInt32();
                m = r.ReadInt32();
                DT.Toh(h.ToString() + ':' + m, DT.h, out dtTim1);
                h = r.ReadInt32();
                m = r.ReadInt32();
                DT.Toh(h.ToString() + ':' + m, DT.h, out dtTim2);
            }
            else
            {
                h = r.ReadInt32();
                m = r.ReadInt32();
                kDtDuration = new TimeSpan(0, h, m);
            }
            bLog = true;
            return true;
        }

        public void UpdateLogStr(string s)
        {
            tLog.Append(s);
        }

        public string Grade { get { return Math.Round((float)uGrade * 0.333, 1).ToString(); } }
    }
}
