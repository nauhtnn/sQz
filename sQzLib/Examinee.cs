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
        Basis = -1,
        Advance = 1
    }
    public class Examinee
    {
        /*
         CREATE TABLE IF NOT EXISTS `examinees` (`dateIdx` INT(4) UNSIGNED, `level` SMALLINT(2),
          `idx` SMALLINT(2) UNSIGNED, `name` VARCHAR(64) CHARACTER SET `utf32`,
          `birthdate` CHAR(10) CHARACTER SET `ascii`, `birthplace` VARCHAR(96) CHARACTER SET `utf32`,
		  `mark` SMALLINT(2) UNSIGNED DEFAULT 65535, `dt1` DATETIME, `dt2` DATETIME,
          PRIMARY KEY(`dateIdx`, `level`, `idx`), FOREIGN KEY(`dateIdx`) REFERENCES dates(`idx`));
         */
        public static List<Examinee> svExaminee = new List<Examinee>();
        public static byte[] sbArr = null;
        public uint mDate;
        public ExamLvl mLvl;
        public ushort mId;
        public string mName;
        public string mBirthdate;
        public string mBirthplace;
        public ushort mMark;
        public static Examinee sAuthNee = null;
        public static Dictionary<int, int> svLvId2Idx = new Dictionary<int, int>();

        public string mComp = null;
        public DateTime mDt1;
		public DateTime mDt2;

        public Examinee() {
            mMark = ushort.MaxValue;
            mDt1 = mDt2 = DateTime.Parse("2016/01/01");
        }
        public string ID {
            get {
                if (mLvl == ExamLvl.Basis)
                    return "A" + mId;
                else
                    return "B" + mId;
            }
        }
        public static bool ToID(string s, ref ExamLvl lv, ref ushort id)
        {
            if(s == null || s.Length < 2)
                return false;
            if (s[0] == 'A')
            {
                if (ushort.TryParse(s.Substring(1), out id))
                {
                    lv = ExamLvl.Basis;
                    return true;
                }
                else
                    return false;
            }
            else if (s[0] == 'B')
            {
                if (ushort.TryParse(s.Substring(1), out id))
                {
                    lv = ExamLvl.Advance;
                    return true;
                }
                else
                    return false;
            }
            return false;
        }
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
            svLvId2Idx.Clear();
            int i = -1;
            if (reader != null)
            {
                while (reader.Read())
                {
                    Examinee s = new Examinee();
					s.mDate = dateIdx;
                    s.mLvl = (ExamLvl)(reader.GetInt16(1));//hardcode
                    s.mId = reader.GetUInt16(2);
                    s.mName = reader.GetString(3);
                    s.mBirthdate = reader.GetString(4);
                    s.mBirthplace = reader.GetString(5);
					s.mMark = reader.GetUInt16(6);
                    svExaminee.Add(s);
                    svLvId2Idx.Add((int)s.mLvl * s.mId, ++i);
                }
                reader.Close();
            }
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
                vals[1] = "" + (int)s.mLvl;
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
                b = BitConverter.GetBytes((short)s.mLvl);
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
            svLvId2Idx.Clear();
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
                s.mLvl = (ExamLvl)BitConverter.ToInt16(buf, offs);
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
                svLvId2Idx.Add((int)s.mLvl * s.mId, i);
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

        public static void CliToAuthArr(out byte[] buf, int state, string rid, string birdate)
        {
            if (birdate == null || birdate.Length != 10)
            {
                buf = null;
                return;
            }
            byte[] a = Encoding.UTF32.GetBytes(birdate);
            byte[] b = null;
            try {
               b = Encoding.UTF32.GetBytes(Environment.MachineName);
            } catch(InvalidOperationException) { b = null; }
            ExamLvl lv = ExamLvl.Basis;
            ushort id = 0;
            bool rs = ToID(rid, ref lv, ref id);
            if (!rs)
            {
                buf = null;
                return;
            }
            if (b == null)
                buf = new byte[50];
            else
                buf = new byte[54 + b.Length];
            Buffer.BlockCopy(BitConverter.GetBytes(state), 0, buf, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes((int)lv), 0, buf, 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(id), 0, buf, 8, 2);
            Buffer.BlockCopy(a, 0, buf, 10, a.Length);
            if (b != null)
            {
                Buffer.BlockCopy(BitConverter.GetBytes(b.Length), 0, buf, 50, 4);
                Buffer.BlockCopy(b, 0, buf, 54, b.Length);
            }
        }
        public static int SrvrReadAuthArr(byte[] buf, int offs, out ExamLvl lv, out string cname)
        {
            cname = null;
            lv = ExamLvl.Basis;
            int l = buf.Length - offs;
            if (l < 4)
                return -1;
            lv = (ExamLvl)BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            if (l < 2)
                return -1;
            ushort id = BitConverter.ToUInt16(buf, offs);
            l -= 2;
            offs += 2;
            if (l < 40)
                return -1;
            string date = Encoding.UTF32.GetString(buf, offs, 40);//hardcode
            l -= 40;
            offs += 40;
            int rid = 0;
            foreach (Examinee st in svExaminee)
            {
                if (st.mId == id && st.mLvl == lv && st.mBirthdate == date)
                {
                    if (4 < l)
                        cname = Encoding.UTF32.GetString(buf, offs + 4, l - 4);
                    return rid;
                }
                ++rid;
            }
            return -1;
        }
        public static void SrvrToAuthArr(int rid, out byte[] buf)
        {
            buf = null;
            if (rid < 0 || svExaminee.Count < rid)
            {
                buf = BitConverter.GetBytes(false);
                return;
            }
            Examinee s = svExaminee[rid];
            byte[] a = Encoding.UTF32.GetBytes(s.mName);
            byte[] b = Encoding.UTF32.GetBytes(s.mBirthdate);
            byte[] c = Encoding.UTF32.GetBytes(s.mBirthplace);
            byte[] d = Encoding.UTF32.GetBytes(s.mDt1.ToString("yyyy/MM/dd"));
            byte[] e = Encoding.UTF32.GetBytes(s.mComp);
            buf = new byte[1 + 4 + 2 + 4 + a.Length + 4 + b.Length + 4 + c.Length +
                4 + d.Length + 4 + e.Length];
            int offs = 0;
            Buffer.BlockCopy(BitConverter.GetBytes(true), 0, buf, offs, 1);
            offs += 1;
            Buffer.BlockCopy(BitConverter.GetBytes((int)s.mLvl), 0, buf, offs, 4);
            offs += 4;
            Buffer.BlockCopy(BitConverter.GetBytes(s.mId), 0, buf, offs, 2);
            offs += 2;
            Buffer.BlockCopy(BitConverter.GetBytes(a.Length), 0, buf, offs, 4);
            offs += 4;
            Buffer.BlockCopy(a, 0, buf, offs, a.Length);
            offs += a.Length;
            Buffer.BlockCopy(BitConverter.GetBytes(b.Length), 0, buf, offs, 4);
            offs += 4;
            Buffer.BlockCopy(b, 0, buf, offs, b.Length);
            offs += b.Length;
            Buffer.BlockCopy(BitConverter.GetBytes(c.Length), 0, buf, offs, 4);
            offs += 4;
            Buffer.BlockCopy(c, 0, buf, offs, c.Length);
            offs += c.Length;
            Buffer.BlockCopy(BitConverter.GetBytes(d.Length), 0, buf, offs, 4);
            offs += 4;
            Buffer.BlockCopy(d, 0, buf, offs, d.Length);
            offs += d.Length;
            Buffer.BlockCopy(BitConverter.GetBytes(e.Length), 0, buf, offs, 4);
            offs += 4;
            Buffer.BlockCopy(e, 0, buf, offs, e.Length);
            offs += e.Length;
        }
        public static bool CliReadAuthArr(byte[] buf, int offs, out Examinee nee)
        {
            nee = null;
            int l = buf.Length - offs;
            if (l < 1)
                return false;
            bool rs = BitConverter.ToBoolean(buf, offs);
            l -= 1;
            offs += 1;
            if (!rs)
                return false;
            if (l < 4)
                return false;
            nee = new Examinee();
            nee.mLvl = (ExamLvl)BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            if (l < 2)
                return false;
            nee.mId = BitConverter.ToUInt16(buf, offs);
            l -= 2;
            offs += 2;
            if (l < 4)
                return false;
            int sz = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            if (l < sz)
                return false;
            nee.mName = Encoding.UTF32.GetString(buf, offs, sz);
            l -= sz;
            offs += sz;
            if (l < 4)
                return false;
            sz = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            if (l < sz)
                return false;
            nee.mBirthdate = Encoding.UTF32.GetString(buf, offs, sz);
            l -= sz;
            offs += sz;
            if (l < 4)
                return false;
            sz = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            if (l < sz)
                return false;
            nee.mBirthplace = Encoding.UTF32.GetString(buf, offs, sz);

            l -= sz;
            offs += sz;
            if (l < 4)
                return false;
            sz = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            if (l < sz)
                return false;
            DateTime.TryParse(Encoding.UTF32.GetString(buf, offs, sz), out nee.mDt1);

            l -= sz;
            offs += sz;
            if (l < 4)
                return false;
            sz = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            if (l < sz)
                return false;
            nee.mComp = Encoding.UTF32.GetString(buf, offs, sz);
            return true;
        }


        public static void ToMarkArr(byte[] prefix, out byte[] buf)
        {
            if (svExaminee.Count == 0)
            {
                buf = null;
                return;
            }
            List<byte[]> l = new List<byte[]>();
            l.Add(BitConverter.GetBytes(svExaminee.Count));
            for (int i = 0; i < svExaminee.Count; ++i)
            {
                Examinee s = svExaminee[i];
                l.Add(BitConverter.GetBytes((short)s.mLvl));
                l.Add(BitConverter.GetBytes(s.mId));
                l.Add(BitConverter.GetBytes(s.mMark));
            }
            int sz = 0;
            if(prefix != null)
                sz += prefix.Length;
            foreach (byte[] i in l)
                sz += i.Length;
            buf = new byte[sz];
            int offs = 0;
            if(prefix != null)
            {
                Buffer.BlockCopy(prefix, 0, buf, offs, prefix.Length);
                offs += prefix.Length;
            }
            for (int i = 0; i < l.Count; ++i)
            {
                Buffer.BlockCopy(l[i], 0, buf, offs, l[i].Length);
                offs += l[i].Length;
            }
        }

        public static void ReadMarkArr(byte[] buf, ref int offs)
        {
            if (buf == null || svLvId2Idx.Count < 1)
                return;
            int l = buf.Length - offs;
            if (l < 4)
                return;
            int nNee = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            int idx;
            for (int i = 0; i < nNee; ++i)
            {
                if (l < 2)
                    return;
                ExamLvl lv = (ExamLvl)BitConverter.ToInt16(buf, offs);
                l -= 2;
                offs += 2;
                if (l < 2)
                    return;
                ushort id = BitConverter.ToUInt16(buf, offs);
                l -= 2;
                offs += 2;
                if (l < 2)
                    return;
                ushort mark = BitConverter.ToUInt16(buf, offs);
                l -= 2;
                offs += 2;
                if (svLvId2Idx.TryGetValue((int)lv * id, out idx)) {
                    svExaminee[idx].mMark = mark;
					//Update(idx);
				}
                else
                    Console.Write(idx);
            }
        }
		
		public static void Update(/*int vi*/) {
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return;
			foreach(Examinee nee in svExaminee) {
				StringBuilder qry = new StringBuilder("UPDATE `examinees` SET ");
				// if(2016 < nee.mDt1.Year)
				// {
					// qry.Append("dt1='" + nee.mDt1 + "'");
					// if(2016 < nee.mDt2.Year) {
						// qry.Append(",dt2='" + nee.mMark + "'");
						if(nee.mMark != short.MaxValue) {
                            //qry.Append(",mark=" + nee.mMark);
                            qry.Append("mark=" + nee.mMark);
							qry.Append(" WHERE dateIdx=" + nee.mDate +
								" AND level=" + (int)nee.mLvl + " AND idx=" + nee.mId);
							DBConnect.Update(conn, qry.ToString());
						}
					// }
					// qry.Append(" WHERE dateIdx='" + nee.mDate +
						// ",level=" + nee.mLvl + ",idx=" + nee.mId +";");
					// DBConnect.Update(conn, qry);
				// }
			}
			DBConnect.Close(ref conn);
		}
    }
}
