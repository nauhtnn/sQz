﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

/*
CREATE TABLE IF NOT EXISTS `exnee1`(`slId` INT(4) UNSIGNED, `lv` TINYINT,
`id` SMALLINT UNSIGNED, `name` VARCHAR(64) CHARACTER SET `utf8`,
`birdate` DATE, `birthplace` VARCHAR(96) CHARACTER SET `utf8`,
`t1` TIME, `t2` TIME, `grd` TINYINT UNSIGNED, `comp` VARCHAR(32),
`qId` SMALLINT UNSIGNED, `anssh` CHAR(120) CHARACTER SET `utf8`,
PRIMARY KEY(`slId`, `lv`, `id`), FOREIGN KEY(`slId`) REFERENCES slot(`id`));

CREATE TABLE IF NOT EXISTS `exnee2`(`slId` INT(4) UNSIGNED, `lv` TINYINT,
`id` SMALLINT UNSIGNED, `name` VARCHAR(64) CHARACTER SET `utf8`,
`birdate` DATE, `birthplace` VARCHAR(96) CHARACTER SET `utf8`,
`t1` TIME, `t2` TIME, `grd` TINYINT UNSIGNED, `comp` VARCHAR(32),
`qId` SMALLINT UNSIGNED, `anssh` CHAR(120) CHARACTER SET `utf8`,
PRIMARY KEY(`slId`, `lv`, `id`), FOREIGN KEY(`slId`) REFERENCES slot(`id`));

CREATE TABLE IF NOT EXISTS `exnee3`(`slId` INT(4) UNSIGNED, `lv` TINYINT,
`id` SMALLINT UNSIGNED, `name` VARCHAR(64) CHARACTER SET `utf8`,
`birdate` DATE, `birthplace` VARCHAR(96) CHARACTER SET `utf8`,
`t1` TIME, `t2` TIME, `grd` TINYINT UNSIGNED, `comp` VARCHAR(32),
`qId` SMALLINT UNSIGNED, `anssh` CHAR(120) CHARACTER SET `utf8`,
PRIMARY KEY(`slId`, `lv`, `id`), FOREIGN KEY(`slId`) REFERENCES slot(`id`));

CREATE TABLE IF NOT EXISTS `exnee4`(`slId` INT(4) UNSIGNED, `lv` TINYINT,
`id` SMALLINT UNSIGNED, `name` VARCHAR(64) CHARACTER SET `utf8`,
`birdate` DATE, `birthplace` VARCHAR(96) CHARACTER SET `utf8`,
`t1` TIME, `t2` TIME, `grd` TINYINT UNSIGNED, `comp` VARCHAR(32),
`qId` SMALLINT UNSIGNED, `anssh` CHAR(120) CHARACTER SET `utf8`,
PRIMARY KEY(`slId`, `lv`, `id`), FOREIGN KEY(`slId`) REFERENCES slot(`id`));

CREATE TABLE IF NOT EXISTS `exnee5`(`slId` INT(4) UNSIGNED, `lv` TINYINT,
`id` SMALLINT UNSIGNED, `name` VARCHAR(64) CHARACTER SET `utf8`,
`birdate` DATE, `birthplace` VARCHAR(96) CHARACTER SET `utf8`,
`t1` TIME, `t2` TIME, `grd` TINYINT UNSIGNED, `comp` VARCHAR(32),
`qId` SMALLINT UNSIGNED, `anssh` CHAR(120) CHARACTER SET `utf8`,
PRIMARY KEY(`slId`, `lv`, `id`), FOREIGN KEY(`slId`) REFERENCES slot(`id`));

CREATE TABLE IF NOT EXISTS `exnee6`(`slId` INT(4) UNSIGNED, `lv` TINYINT,
`id` SMALLINT UNSIGNED, `name` VARCHAR(64) CHARACTER SET `utf8`,
`birdate` DATE, `birthplace` VARCHAR(96) CHARACTER SET `utf8`,
`t1` TIME, `t2` TIME, `grd` TINYINT UNSIGNED, `comp` VARCHAR(32),
`qId` SMALLINT UNSIGNED, `anssh` CHAR(120) CHARACTER SET `utf8`,
PRIMARY KEY(`slId`, `lv`, `id`), FOREIGN KEY(`slId`) REFERENCES slot(`id`));

FOREIGN KEY(`qId`) REFERENCES questsh(`id`));
*/

namespace sQzLib
{
    public abstract class ExamineeA
    {
        public uint uSlId;
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

        public const string tDBtbl = "exnee";

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
            uSlId = uint.MaxValue;
            uId = ushort.MaxValue;
            eStt = eSIGNING;
            uGrade = ushort.MaxValue;
            dtTim1 = dtTim2 = ExamSlot.INVALID_DT;
            tComp = string.Empty;
            mAnsSh = new AnsSheet();
            kDtDuration = new TimeSpan(0, 30, 0);
            tLog = new StringBuilder();
        }
        public string tId {
            get {
                if (eLvl == ExamLvl.Basis)
                    return "CB" + uId.ToString("d3");
                else
                    return "NC" + uId.ToString("d3");
            }
        }
        public bool ParseTxId(string s)
        {
            if (s == null || s.Length != 5)
                return true;
            s = s.ToUpper();
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
        public short Lv
        {
            get { return (short)eLvl; }
            set { eLvl = (ExamLvl)value; }
        }

        public override string ToString()
        {
            StringBuilder s = new StringBuilder();
            s.AppendFormat("{0}{1}, {2}, {3}, {4}",
                (eLvl == ExamLvl.Basis) ? "CB" : "NC",
                uId.ToString("d3"), tName, tBirdate, tBirthplace);
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
                (Lv * uId) + '-' + m.ToString("d2") + s.ToString("d2"));
            System.IO.BinaryWriter w = null;
            try
            {
                w = new System.IO.BinaryWriter(System.IO.File.OpenWrite(fileName),
                    Encoding.UTF8);
            }
            catch (UnauthorizedAccessException) { err = true; }
            if (err)
                return true;
            w.Write(uSlId);
            w.Write(Lv);
            w.Write(uId);
            w.Write(eStt);
            w.Write(mAnsSh.uQSId);
            w.Write(mAnsSh.aAns, 0, 120);//mAnsSh.aAns.Length);
            if (eStt == eFINISHED)
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
            uSlId = r.ReadUInt32();
            eLvl = (ExamLvl)r.ReadInt16();
            uId = r.ReadUInt16();
            eStt = r.ReadInt32();
            mAnsSh.uQSId = r.ReadUInt16();
            mAnsSh.aAns = r.ReadBytes(120);
            int h, m;
            if(eStt == eFINISHED)
            {
                h = r.ReadInt32();
                m = r.ReadInt32();
                ExamSlot.Parse(h.ToString() + ':' + m, ExamSlot.FORM_h, out dtTim1);
                h = r.ReadInt32();
                m = r.ReadInt32();
                ExamSlot.Parse(h.ToString() + ':' + m, ExamSlot.FORM_h, out dtTim2);
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