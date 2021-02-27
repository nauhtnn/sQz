using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sQzLib
{
    public sealed class ExamineeC : ExamineeA
    {

        public TimeSpan kDtDuration;

        public StringBuilder tLog;

        const string tLOG_DIR = "sQz\\";
        const string tLOG_PRE = "sav";

        public bool bLog;

        public ExamineeC()
        {
            Reset();
            bLog = false;
        }

        public override void Reset()
        {
            _Reset();
            kDtDuration = new TimeSpan(0, 30, 0);
            tLog = new StringBuilder();
        }

        public override List<byte[]> GetBytes_ClientSendingToS1()
        {
            List<byte[]> l = new List<byte[]>();
            Utils.AppendBytesOfString(ID, l);
            l.Add(BitConverter.GetBytes((int)eStt));
			l.Add(BitConverter.GetBytes(bLog));

            if (eStt < NeeStt.Examing || bLog)
            {
                Utils.AppendBytesOfString(Birthdate, l);
                Utils.AppendBytesOfString(ComputerName, l);
            }

            if (eStt < NeeStt.Examing)
                return l;

            l.Add(BitConverter.GetBytes(mAnsSh.questSheetID));

            if (eStt < NeeStt.Submitting)
                return l;

            l.Add(mAnsSh.aAns);

            return l;
        }

        public override bool ReadByte(byte[] buf, ref int offs)
        {
            int l = buf.Length - offs;

            if (l < 4)
                return true;
            int x;
            if (Enum.IsDefined(typeof(NeeStt), x = BitConverter.ToInt32(buf, offs)))
                eStt = (NeeStt)x;
            l -= 4;
            offs += 4;

            if (eStt == NeeStt.Finished)
            {
                Grade = BitConverter.ToInt32(buf, offs);
                l -= 4;
                offs += 4;
            }

			if(eStt < NeeStt.Submitting || bLog)
			{
				if (l < 4)
					return true;
				int sz = BitConverter.ToInt32(buf, offs);
				l -= 4;
				offs += 4;
				if (l < sz)
					return true;
				Birthdate = Encoding.UTF8.GetString(buf, offs, sz);
				l -= sz;
				offs += sz;

				if (l < 4)
					return true;
				sz = BitConverter.ToInt32(buf, offs);
				l -= 4;
				offs += 4;
				if (l < sz)
					return true;
				Name = Encoding.UTF8.GetString(buf, offs, sz);
				l -= sz;
				offs += sz;

				if (l < 4)
					return true;
				sz = BitConverter.ToInt32(buf, offs);
				l -= 4;
				offs += 4;
				if (l < sz)
					return true;
				Birthplace = Encoding.UTF8.GetString(buf, offs, sz);
				l -= sz;
				offs += sz;
            }

            bLog = false;

            return false;
        }

        public override void Merge(ExamineeA e)
        {
            eStt = e.eStt;
            if (eStt == NeeStt.Finished)
                Grade = e.Grade;
            if (eStt < NeeStt.Finished || bLog)
            {
                Birthdate = e.Birthdate;
                Name = e.Name;
                Birthplace = e.Birthplace;
            }
            bLog = false;
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
            catch (UnauthorizedAccessException) { err = true; }
            if (err)
                return true;
            var fileName = System.IO.Path.Combine(p, tLOG_PRE +
                ID + '-' + m.ToString("d2") + s.ToString("d2"));
            System.IO.BinaryWriter w = null;
            try
            {
                w = new System.IO.BinaryWriter(System.IO.File.OpenWrite(fileName),
                    Encoding.UTF8);
            }
            catch (UnauthorizedAccessException) { err = true; }
            if (err)
                return true;
            w.Write(ID);
            w.Write((int)eStt);
            w.Write(mAnsSh.questSheetID);
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
            //if (Enum.IsDefined(typeof(ExamLv), x = r.ReadInt32()))
            //    eLv = (ExamLv)x;
            //uId = r.ReadInt32();
            ID = r.ReadString();
            if (Enum.IsDefined(typeof(NeeStt), x = r.ReadInt32()))
                eStt = (NeeStt)x;
            mAnsSh.questSheetID = r.ReadInt32();
            mAnsSh.aAns = r.ReadBytes(AnsSheet.LEN);
            int h, m;
            if (eStt == NeeStt.Finished)
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
    }
}
