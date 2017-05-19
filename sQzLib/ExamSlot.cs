using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using MySql.Data.MySqlClient;

/*
CREATE TABLE IF NOT EXISTS `slot` (`id` INT(4) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
 `dt` DATETIME);
*/

namespace sQzLib
{
    public class ExamSlot
    {
        public DateTime mDt;
        static CultureInfo sCultInfo = null;
        public uint uId;
        public const int INVALID = 0;
        public static DateTime INVALID_DT = DateTime.Parse("2016/01/01 00:00");//h = m = INVALID
        public const string MYSQL2016 = "2016-01-01";
        public const string FORM_h = "H:m";
        public const string FORM_H = "yyyy/MM/dd HH:mm";
        public const string FORM = "yyyy/MM/dd";
        public const string FORM_RH = "dd/MM/yyyy HH:mm";
        public const string FORM_R = "dd/MM/yyyy";
        public const string FORM_MYSQL = "yyyy-MM-dd HH:00";

        public Dictionary<int, ExamRoom> vRoom;

        public ExamSlot()
        {
            mDt = INVALID_DT;
            uId = uint.MaxValue;

            vRoom = new Dictionary<int, ExamRoom>();
            for (int i = 1; i < 7; ++i)//todo: read from db
            {
                ExamRoom r = new ExamRoom();
                r.uId = i;
                vRoom.Add(i, r);
            }
        }

        public void DBInsert()
        {
            string v = "('" + mDt.ToString(FORM_MYSQL) + "')";
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return;
            DBConnect.Ins(conn, "slot", "dt", v);
            DBConnect.Close(ref conn);
        }

        public Dictionary<uint, DateTime> DBSelect()
        {
            Dictionary<uint, DateTime> r = new Dictionary<uint, DateTime>();
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return r;
            string qry = DBConnect.mkQrySelect("slot", null, null, null);
            MySqlDataReader reader = DBConnect.exeQrySelect(conn, qry);
            while (reader.Read())
                r.Add(reader.GetUInt32(0), reader.GetDateTime(1));
            reader.Close();
            DBConnect.Close(ref conn);
            return r;
        }

        public int GetByteCountDt()
        {
            return 20;
        }

        public static void ToByteDt(byte[] buf, ref int offs, DateTime dt)
        {
            Array.Copy(BitConverter.GetBytes(dt.Year), 0, buf, offs, 4);
            offs += 4;
            Array.Copy(BitConverter.GetBytes(dt.Month), 0, buf, offs, 4);
            offs += 4;
            Array.Copy(BitConverter.GetBytes(dt.Day), 0, buf, offs, 4);
            offs += 4;
            Array.Copy(BitConverter.GetBytes(dt.Hour), 0, buf, offs, 4);
            offs += 4;
            Array.Copy(BitConverter.GetBytes(dt.Minute), 0, buf, offs, 4);
            offs += 4;
        }

        public static bool ReadByteDt(byte[] buf, ref int offs, out DateTime dt)
        {
            if (buf.Length - offs < 20)
            {
                dt = INVALID_DT;
                return true;
            }
            int y = BitConverter.ToInt32(buf, offs);
            offs += 4;
            int M = BitConverter.ToInt32(buf, offs);
            offs += 4;
            int d = BitConverter.ToInt32(buf, offs);
            offs += 4;
            int H = BitConverter.ToInt32(buf, offs);
            offs += 4;
            int m = BitConverter.ToInt32(buf, offs);
            offs += 4;
            if (Parse(y.ToString("d4") + '/' + M.ToString("d2") + '/' + d.ToString("d2") +
                ' ' + H.ToString("d2") + ':' + m.ToString("d2"), FORM_H, out dt))
                return true;
            return false;
        }

        public List<byte[]> ToByteR(int rId)
        {
            List<byte[]> l = new List<byte[]>();
            ExamRoom r;
            if (rId == 0)
                foreach (ExamRoom i in vRoom.Values)
                {
                    byte[] a = i.ToByteS1();
                    if (4 < a.Length)
                        l.Add(a);
                }
            else if (vRoom.TryGetValue(rId, out r))
                l.Add(r.ToByteS1());
            return l;
        }

        public static bool Parse(string s, string form, out DateTime dt)
        {
            if (sCultInfo == null)
                sCultInfo = CultureInfo.CreateSpecificCulture("en-US");
            if (DateTime.TryParseExact(s, form, sCultInfo, DateTimeStyles.None, out dt))
                return false;
            return true;
        }

        public static string ToMysqlForm(string s, string curForm)
        {
            DateTime dt;
            if (!Parse(s, curForm, out dt))
                return dt.ToString(FORM_MYSQL);
            return MYSQL2016;
        }

        public void ReadF(string fp)
        {
            string buf = Utils.ReadFile(fp);
            if (buf == null)
                return;
            string[] vs = buf.Split('\n');
            foreach (string s in vs)
            {
                ExamineeS0 e = new ExamineeS0();
                string[] v = s.Split('\t');
                if (v.Length == 5)
                {
                    if (v[0].Length < 2)
                        continue;
                    v[0] = v[0].ToUpper();
                    ExamLv x;
                    if(Enum.TryParse(v[0].Substring(0, 1), out x))
                        e.eLv = x;
                    else
                        continue;
                    int uRId;
                    if (!int.TryParse(v[0].Substring(1), out e.uId)
                        || !int.TryParse(v[1], out uRId) || !vRoom.ContainsKey(uRId))
                        continue;
                    e.uSlId = uId;
                    e.tName = v[2].Trim();
                    e.tBirdate = v[3];
                    e.tBirthplace = v[4].Trim();
                    vRoom[uRId].vExaminee.Add(e.mLv + e.uId, e);
                }
            }
        }

        public void DBInsertNee()
        {
            foreach (ExamRoom r in vRoom.Values)
                r.DBInsert();
        }

        public void DBSelectNee()
        {
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return;
            foreach (ExamRoom r in vRoom.Values)
            {
                r.vExaminee.Clear();
                string qry = DBConnect.mkQrySelect(ExamineeS0.tDBtbl + r.uId,
                    "lv,id,name,birdate,birthplace,t1,t2,grd,comp,qId,anssh",
                    "slId=" + uId, null);
                MySqlDataReader reader = DBConnect.exeQrySelect(conn, qry);
                if (reader != null)
                {
                    while (reader.Read())
                    {
                        ExamineeS0 e = new ExamineeS0();
                        e.uSlId = uId;
                        int lv;
                        if (Enum.IsDefined(typeof(ExamLv), lv = reader.GetInt16(0)))
                            e.eLv = (ExamLv)lv;
                        e.uId = reader.GetInt32(1);
                        e.tName = reader.GetString(2);
                        e.tBirdate = reader.GetDateTime(3).ToString(FORM_R);
                        e.tBirthplace = reader.GetString(4);
                        e.dtTim1 = (reader.IsDBNull(5)) ? INVALID_DT :
                            DateTime.Parse(reader.GetString(5));
                        e.dtTim2 = (reader.IsDBNull(6)) ? INVALID_DT :
                            DateTime.Parse(reader.GetString(6));
                        if (!reader.IsDBNull(7))
                        {
                            e.eStt = ExamStt.Finished;
                            e.uGrade = reader.GetInt32(7);
                        }
                        else
                            e.eStt = ExamStt.Info;
                        if (!reader.IsDBNull(8))
                            e.tComp = reader.GetString(8);
                        else
                            e.tComp = "unknown";//todo
                        r.vExaminee.Add(e.mLv + e.uId, e);
                    }
                    reader.Close();
                }
            }
            DBConnect.Close(ref conn);
        }

        public void ReadByteNee(byte[] buf, ref int offs)
        {
            List<ExamineeA> v = new List<ExamineeA>();
            List<ExamineeA> l = new List<ExamineeA>();
            while (true)
            {
                if (buf.Length - offs < 4)
                    break;
                int rId = BitConverter.ToInt32(buf, offs);
                offs += 4;
                ExamRoom r;
                if (!vRoom.TryGetValue(rId, out r))
                    break;
                if (r.ReadByteS0(buf, ref offs, ref v))
                    break;
                foreach (ExamineeS0 e in v)
                {
                    ExamineeA o;
                    bool unfound = true;
                    foreach (ExamRoom i in vRoom.Values)
                        if (i.uId != rId && i.vExaminee.TryGetValue(e.mLv + e.uId, out o))
                        {
                            unfound = false;
                            //o.bFromC = false;
                            o.Merge(e);
                            break;
                        }
                    if (unfound)
                        l.Add(e);
                }
                v.Clear();
            }
        }

        public void DBUpdateRs()
        {
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return;
            foreach (ExamRoom r in vRoom.Values)
                r.DBUpdateRs(conn);
            DBConnect.Close(ref conn);
        }
    }
}
