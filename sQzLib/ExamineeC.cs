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

        public List<byte[]> GetBytes_SendingToS1()
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

            l.Add(BitConverter.GetBytes(AnswerSheet.QuestSheetID));

            if (eStt < NeeStt.Submitting)
                return l;

            l.Add(BitConverter.GetBytes(AnswerSheet.BytesOfAnswer_Length));
            l.Add(AnswerSheet.BytesOfAnswer);

            return l;
        }

        public bool ReadBytes_FromS1(byte[] buf, ref int offs)
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

        public void MergeWithS1(ExamineeA e)
        {
            if (e.eStt == NeeStt.Finished)
                Grade = e.Grade;
            if (e.eStt < NeeStt.Finished || bLog)
            {
                Birthdate = e.Birthdate;
                Name = e.Name;
                Birthplace = e.Birthplace;
            }
            bLog = false;
            eStt = e.eStt;//for safety, set the status last
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
            w.Write(mDt.ToBinary());
            w.Write(ID);
            w.Write((int)eStt);
            w.Write(AnswerSheet.QuestSheetID);
            w.Write(AnswerSheet.BytesOfAnswer_Length);
            w.Write(AnswerSheet.BytesOfAnswer, 0, AnswerSheet.BytesOfAnswer_Length);
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
            AnswerSheet.bChanged = false;
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
                catch (UnauthorizedAccessException)
                {
                    r = null;
                }
            if (r == null)
                return false;
            StringBuilder msg = new StringBuilder();
            try
            {
                msg.Append("Read DateTime.\n");
                mDt = DateTime.FromBinary(r.ReadInt64());
                msg.Append("Read ID.\n");
                ID = r.ReadString();
                msg.Append("Read status.\n");
                int x;
                if (Enum.IsDefined(typeof(NeeStt), x = r.ReadInt32()))
                    eStt = (NeeStt)x;
                msg.Append("Read sheet ID.\n");
                AnswerSheet.QuestSheetID = r.ReadInt32();
                msg.Append("Read answer length.\n");
                AnswerSheet.QuestSheetID = r.ReadInt32();
                msg.Append("Read answer array.\n");
                AnswerSheet.BytesOfAnswer = r.ReadBytes(AnswerSheet.BytesOfAnswer_Length);
                int h, m;
                if (eStt == NeeStt.Finished)
                {
                    msg.Append("Read start hour.\n");
                    h = r.ReadInt32();
                    msg.Append("Read start minute.\n");
                    m = r.ReadInt32();
                    if(DT.Toh(h.ToString() + ':' + m, DT.h, out dtTim1))
                        msg.Append("Start time invalid " + h + ":" + m + "\n");
                    h = r.ReadInt32();
                    msg.Append("Read end hour.\n");
                    m = r.ReadInt32();
                    msg.Append("Read end minute.\n");
                    if(DT.Toh(h.ToString() + ':' + m, DT.h, out dtTim2))
                        msg.Append("End time invalid " + h + ":" + m + "\n");
                }
                else
                {
                    msg.Append("Read current hour.\n");
                    h = r.ReadInt32();
                    msg.Append("Read current minute.\n");
                    m = r.ReadInt32();
                    msg.Append("Set duration " + h + ":" + m + "\n");
                    kDtDuration = new TimeSpan(0, h, m);
                }
                bLog = true;
                msg.Append("Finish!");
            }
            catch (System.IO.EndOfStreamException e)
            {
                System.Windows.MessageBox.Show(msg.ToString() + e.ToString());
                return false;
            }
            catch (System.IO.IOException e)
            {
                System.Windows.MessageBox.Show(msg.ToString() + e.ToString());
                return false;
            }
            catch (System.ArgumentException e)
            {
                System.Windows.MessageBox.Show(msg.ToString() + e.ToString());
                return false;
            }
            return true;
        }

        public void UpdateLogStr(string s)
        {
            tLog.Append(s);
        }

        public override string ToString()
        {
            return ID + ", " + Name + ", " + Birthdate;
        }
    }
}
