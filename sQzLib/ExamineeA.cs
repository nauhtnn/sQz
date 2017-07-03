using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;

namespace sQzLib
{
    public enum ExamLv
    {
        A = 'A',
        B = 'B'
    }

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
        public const int LV_CAP = 10000;//db sqz_examinee `id` SMALLINT UNSIGNED
        public DateTime mDt;
        public NeeStt eStt;
        public ExamLv eLv;
        public int uId;
        public int LvId { get { return (eLv == ExamLv.A) ? uId : uId + LV_CAP; } }
        public string tId { get { return eLv.ToString() + uId.ToString("d4"); } }
        public static string gId(ExamLv lv, int id) { return lv.ToString() + id.ToString("d4"); }
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
            Reset();
        }

        public void Reset()
        {
            mDt = DT.INV_H;
            eLv = ExamLv.A;
            uId = LV_CAP;
            tName = null;
            tBirdate = null;
            tBirthplace = null;
            eStt = NeeStt.Signing;
            uGrade = LV_CAP;
            dtTim1 = dtTim2 = DT.INV_;
            tComp = string.Empty;
            mAnsSh = new AnsSheet();
            kDtDuration = new TimeSpan(0, 30, 0);
            tLog = new StringBuilder();
        }

        public bool ParseLvId(int lvid)
        {
            if (lvid != LV_CAP &&(lvid < 1 || LV_CAP + LV_CAP <= lvid))
                return true;
            if(lvid < LV_CAP)
            {
                eLv = ExamLv.A;
                uId = lvid;
            }
            else
            {
                eLv = ExamLv.B;
                uId = lvid - (int)eLv;
            }
            return false;
        }

        public bool ParseLvId(string s)
        {
            if (s == null || s.Length != 5)
                return true;
            s = s.ToUpper();
            ExamLv lv;
            if (!Enum.TryParse(s.Substring(0, 1), out lv))
                return true;
            int uid;
            if (!int.TryParse(s.Substring(1), out uid))
                return true;
            if (uid < 1 || LV_CAP <= uid)
                return true;
            if (eLv != lv || uId != uid)
                Reset();
            eLv = lv;
            uId = uid;
            return false;
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
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return -1;
            string qry = DBConnect.mkQrySelect("sqz_nee_qsheet", "qsid",
                "dt='" + mDt.ToString(DT._) + "' AND lv='" + eLv.ToString() +
                "' AND neeid=" + uId);
            string eMsg;
            MySqlDataReader reader = DBConnect.exeQrySelect(conn, qry, out eMsg);
            if (reader == null)
            {
                DBConnect.Close(ref conn);
                return -1;
            }
            int qsid = -1;
            if (reader.Read())
                qsid = reader.GetInt32(0);
            reader.Close();
            DBConnect.Close(ref conn);
            return qsid;
        }

        public char[] DBGetAns()
        {
            char[] noans = new char[AnsSheet.LEN];
            for (int i = 0; i < AnsSheet.LEN; ++i)
                noans[i] = Question.C0;
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return noans;
            string qry = DBConnect.mkQrySelect("sqz_nee_qsheet", "ans",
                "dt='" + mDt.ToString(DT._) + "' AND lv='" + eLv.ToString() +
                "' AND neeid=" + uId);
            string eMsg;
            MySqlDataReader reader = DBConnect.exeQrySelect(conn, qry, out eMsg);
            if (reader == null)
            {
                DBConnect.Close(ref conn);
                return noans;
            }
            string ans = noans.ToString();
            if (reader.Read())
                ans = reader.GetString(0);
            reader.Close();
            DBConnect.Close(ref conn);
            return ans.ToCharArray();
        }

        public bool DBSelGrade()
        {
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return true;
            string qry = DBConnect.mkQrySelect("sqz_nee_qsheet", "grade",
                "dt='" + mDt.ToString(DT._) + "' AND lv='" + eLv.ToString() +
                "' AND neeid=" + uId);
            string eMsg;
            MySqlDataReader reader = DBConnect.exeQrySelect(conn, qry, out eMsg);
            if (reader == null)
            {
                DBConnect.Close(ref conn);
                return true;
            }
            if (reader.Read())
                uGrade = reader.GetInt16(0);
            reader.Close();
            DBConnect.Close(ref conn);
            return false;
        }

        public string DBGetT()
        {
            MySqlConnection conn = DBConnect.Init();
            string t = DT.INV_H.ToString(DT.hh);
            if (conn == null)
                return t;
            string qry = DBConnect.mkQrySelect("sqz_examinee",
                "t", "dt='" + mDt.ToString(DT._) + "' AND lv='" + eLv.ToString() +
                "' AND id=" + uId);
            string eMsg;
            MySqlDataReader reader = DBConnect.exeQrySelect(conn, qry, out eMsg);
            if (reader == null)
            {
                DBConnect.Close(ref conn);
                return t;
            }
            if (reader.Read())
            {
                t = reader.GetString(0);
            }
            reader.Close();
            DBConnect.Close(ref conn);
            return t;
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
            w.Write((int)eLv);
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
            if (Enum.IsDefined(typeof(ExamLv), x = r.ReadInt32()))
                eLv = (ExamLv)x;
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
