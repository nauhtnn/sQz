using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace sQzLib
{
    public enum ExamLvl
    {
        Basis = 0,
        Advance = 1
    }
    public class Examinee
    {
        /*
         CREATE TABLE IF NOT EXISTS `examinees` (`dateIdx` INT(4) UNSIGNED, `level` SMALLINT(2) UNSIGNED,
          `idx` SMALLINT(2) UNSIGNED, `name` VARCHAR(64) CHARACTER SET `utf32`,
          `birthdate` CHAR(10) CHARACTER SET `ascii`, `birthplace` VARCHAR(96) CHARACTER SET `utf32`,
          PRIMARY KEY(`dateIdx`, `level`, `idx`), FOREIGN KEY(`dateIdx`) REFERENCES dates(`idx`));
         */
        public static List<Examinee> svExaminee = new List<Examinee>();
        public static byte[] sbArr = null;
        //not include dateIdx
        public ExamLvl mLvl;
        public ushort mId;
        string mName;
        public string mBirthdate;
        string mBirthplace;
        public Examinee() { }
        public static void ReadTxt(short dateId)
        {
            ReadTxt(Utils.ReadFile("Examinees" + dateId + ".txt"));
        }
        public override string ToString()
        {
            StringBuilder s = new StringBuilder();
            if (mLvl == ExamLvl.Basis)
                s.Append('A');
            else
                s.Append('B');
            s.AppendFormat("{0}, {1}, {2}, ", mId, mName, mBirthdate);
            s.Append(mBirthplace);
            return s.ToString();
        }
        public static void ReadTxt(string buf)
        {
            if (buf == null)
                return;
            svExaminee.Clear();
            string[] students = buf.Split('\n');
            foreach (string stu in students)
            {
                Examinee stud = new Examinee();
                string[] s = stu.Split('\t');
                if (s.Length == 4) //todo: hardcode, unsafe
                {
                    if ((s[0])[0] == 'A')
                        stud.mLvl = ExamLvl.Basis;
                    else
                        stud.mLvl = ExamLvl.Advance;
                    stud.mId = Convert.ToUInt16(s[0].Substring(1));
                    stud.mName = s[1];
                    stud.mBirthdate = s[2];
                    stud.mBirthplace = s[3];
                    svExaminee.Add(stud);
                }
            }
            ToByteArr();
        }
        public static void DBSelect(uint dateIdx)
        {
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return;
            string qry = DBConnect.mkQrySelect("examinees", null, "dateIdx", "" + dateIdx, null);
            MySqlDataReader reader = DBConnect.exeQrySelect(conn, qry);
            svExaminee.Clear();
            while (reader.Read())
            {
                Examinee s = new Examinee();
                s.mLvl = (ExamLvl)(reader.GetUInt16(1));//hardcode
                s.mId = reader.GetUInt16(2);
                s.mName = reader.GetString(3);
                s.mBirthdate = reader.GetString(4);
                s.mBirthplace = reader.GetString(5);
                svExaminee.Add(s);
            }
            reader.Close();
            DBConnect.Close(ref conn);
            ToByteArr();
        }

        public static void DBInsert(uint dateIdx)
        {
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return;
            string[] attbs = new string[6];//hardcode
            attbs[0] = "dateIdx";
            attbs[1] = "level";
            attbs[2] = "idx";
            attbs[3] = "name";
            attbs[4] = "birthdate";
            attbs[5] = "birthplace";
            foreach (Examinee s in svExaminee)
            {
                string[] vals = new string[6];
                vals[0] = "" + dateIdx;
                vals[1] = "" + (uint)s.mLvl;
                vals[2] = "" + s.mId;
                vals[3] = "'" + s.mName + "'";
                vals[4] = "'" + s.mBirthdate + "'";
                vals[5] = "'" + s.mBirthplace + "'";
                DBConnect.Ins(conn, "examinees", attbs, vals);
            }
            DBConnect.Close(ref conn);
        }
        public static void ToByteArr()
        {
            if (svExaminee.Count == 0)
                return;
            List<byte[]> l = new List<byte[]>();
            byte[] b = BitConverter.GetBytes(svExaminee.Count);
            l.Add(b);
            for (int i = 0; i < svExaminee.Count; ++i)
            {
                Examinee s = svExaminee[i];
                b = BitConverter.GetBytes((Int16)s.mLvl);
                l.Add(b);
                b = BitConverter.GetBytes(s.mId);
                l.Add(b);
                b = Encoding.UTF32.GetBytes(s.mName);
                l.Add(BitConverter.GetBytes(b.Length));
                l.Add(b);
                b = Encoding.UTF32.GetBytes(s.mBirthdate);
                l.Add(BitConverter.GetBytes(b.Length));
                l.Add(b);
                b = Encoding.UTF32.GetBytes(s.mBirthplace);
                l.Add(BitConverter.GetBytes(b.Length));
                l.Add(b);
            }
            int sz = 0;
            foreach (byte[] i in l)
                sz += i.Length;
            sbArr = new byte[sz];
            int offs = 0;
            for (int i = 0; i < l.Count; ++i)
            {
                Buffer.BlockCopy(l[i], 0, sbArr, offs, l[i].Length);
                offs += l[i].Length;
            }
        }
        public static void ReadByteArr(byte[] buf, ref int offs, int l)
        {
            svExaminee.Clear();
            if (buf == null)
                return;
            int offs0 = offs;
            int sz = 0;
            if (l < 4)
                return;
            int nNee = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            for (int i = 0; i < nNee; ++i)
            {
                Examinee s = new Examinee();
                if (l < 2)
                    break;
                s.mLvl = (ExamLvl)BitConverter.ToUInt16(buf, offs);
                l -= 2;
                offs += 2;
                if (l < 2)
                    break;
                s.mId = BitConverter.ToUInt16(buf, offs);
                l -= 2;
                offs += 2;
                if (l < 4)
                    break;
                sz = BitConverter.ToInt32(buf, offs);
                l -= 4;
                offs += 4;
                if (l < sz)
                    break;
                byte[] b = new byte[sz];
                Buffer.BlockCopy(buf, offs, b, 0, sz);
                s.mName = Encoding.UTF32.GetString(b);
                l -= sz;
                offs += sz;
                if (l < 4)
                    break;
                sz = BitConverter.ToInt32(buf, offs);
                l -= 4;
                offs += 4;
                if (l < sz)
                    break;
                b = new byte[sz];
                Buffer.BlockCopy(buf, offs, b, 0, sz);
                s.mBirthdate = Encoding.UTF32.GetString(b);
                l -= sz;
                offs += sz;
                if (l < 4)
                    break;
                sz = BitConverter.ToInt32(buf, offs);
                l -= 4;
                offs += 4;
                if (l < sz)
                    break;
                b = new byte[sz];
                Buffer.BlockCopy(buf, offs, b, 0, sz);
                s.mBirthplace = Encoding.UTF32.GetString(b);
                svExaminee.Add(s);
                l -= sz;
                offs += sz;
            }
            if (!Array.Equals(buf, sbArr))
            {
                sz = offs - offs0;
                if (sz == buf.Length)
                    sbArr = (byte[])buf.Clone();
                else
                {
                    sbArr = new byte[sz];
                    Buffer.BlockCopy(buf, 0, sbArr, 0, sz);
                }
            }
        }
    }
}
