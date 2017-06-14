using System;
using System.Collections.Generic;
using System.Text;

namespace sQzLib
{
    public enum ExamLv
    {
        A = 'A',
        B = 'B'
    }

    public enum ExamStt
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
        protected const int LV_CAP = 100000;
        public DateTime mDt;
        public ExamStt eStt;
        public ExamLv eLv;
        public int uId;
        public int LvId { get { return uId + ((eLv == ExamLv.A) ? 0 : LV_CAP); } }
        public string tId { get { return eLv.ToString() + uId.ToString("d4"); } }
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
            tBirdate = null;
            tBirthplace = null;
            tName = null;
            mDt = DT.INV_H;
            uId = ushort.MaxValue;
            eStt = ExamStt.Signing;
            uGrade = ushort.MaxValue;
            dtTim1 = dtTim2 = DT.INV_;
            tComp = string.Empty;
            mAnsSh = new AnsSheet();
            kDtDuration = new TimeSpan(0, 30, 0);
            tLog = new StringBuilder();
        }
        
        public bool ParseTxId(string s)
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
            if (eStt == ExamStt.Finished)
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
            if (Enum.IsDefined(typeof(ExamStt), x = r.ReadInt32()))
                eStt = (ExamStt)x;
            mAnsSh.uQSId = r.ReadInt32();
            mAnsSh.aAns = r.ReadBytes(AnsSheet.LEN);
            int h, m;
            if(eStt == ExamStt.Finished)
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

        public double Grade { get { return Math.Round((float)uGrade * 0.333, 1); } }
    }
}
