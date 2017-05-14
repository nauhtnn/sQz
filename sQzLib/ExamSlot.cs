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
            for (int i = 1; i < 6; ++i)//todo: read from db
                vRoom.Add(i, new ExamRoom(i));
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

        public int GetByteCount()
        {
            return 16;
        }

        public void ToByte(byte[] buf, ref int offs)
        {
            Buffer.BlockCopy(BitConverter.GetBytes(mDt.Year), 0, buf, offs, 4);
            offs += 4;
            Buffer.BlockCopy(BitConverter.GetBytes(mDt.Month), 0, buf, offs, 4);
            offs += 4;
            Buffer.BlockCopy(BitConverter.GetBytes(mDt.Day), 0, buf, offs, 4);
            offs += 4;
            Buffer.BlockCopy(BitConverter.GetBytes(mDt.Hour), 0, buf, offs, 4);
            offs += 4;
        }

        public bool ReadByte(byte[] buf, ref int offs)
        {
            if (buf.Length - offs < 16)
                return true;
            int y = BitConverter.ToInt32(buf, offs);
            offs += 4;
            int M = BitConverter.ToInt32(buf, offs);
            offs += 4;
            int d = BitConverter.ToInt32(buf, offs);
            offs += 4;
            int H = BitConverter.ToInt32(buf, offs);
            offs += 4;
            if (Parse(y.ToString("d4") + '/' + M.ToString("d2") + '/' + d.ToString("d2") +
                ' ' + H.ToString("00") + ":00", FORM_H, out mDt))
                return true;
            return false;
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

        public void ReadS(string buf)
        {
            string[] vs = buf.Split('\n');
            foreach (string s in vs)
            {
                Examinee nee = new Examinee();
                string[] v = s.Split('\t');
                if (v.Length == 5)
                {
                    if (v[0].Length < 3)
                        continue;
                    v[0] = v[0].ToUpper();
                    if (v[0][0] == 'C' && v[0][1] == 'B')
                        nee.eLvl = ExamLvl.Basis;
                    else if (v[0][0] == 'N' && v[0][1] == 'C')
                        nee.eLvl = ExamLvl.Advance;
                    else
                        continue;
                    if (!ushort.TryParse(v[0].Substring(2), out nee.uId)
                        || !int.TryParse(v[1], out nee.uRId) || !vRoom.ContainsKey(nee.uRId))
                        continue;
                    nee.uSlId = uId;
                    nee.tName = v[2].Trim();
                    nee.tBirdate = v[3];
                    nee.tBirthplace = v[4].Trim();
                    vRoom[nee.uRId].vExaminee.Add(nee.Lvl * nee.uId, nee);
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
            string qry = DBConnect.mkQrySelect("examinee",
                "rId,lvl,id,name,birdate,birthplace,t1,t2,grd,comp,qId,anssh",
                "slId=" + uId, null);
            MySqlDataReader reader = DBConnect.exeQrySelect(conn, qry);
            foreach (ExamRoom r in vRoom.Values)
                r.vExaminee.Clear();
            if (reader != null)
            {
                while (reader.Read())
                {
                    Examinee e = new Examinee();
                    e.eStt = Examinee.eINFO;
                    e.uSlId = uId;
                    e.uRId = reader.GetInt16(0);
                    e.Lvl = reader.GetInt16(1);
                    e.uId = reader.GetUInt16(2);
                    e.tName = reader.GetString(3);
                    e.tBirdate = reader.GetDateTime(4).ToString(FORM_R);
                    e.tBirthplace = reader.GetString(5);
                    e.dtTim1 = (reader.IsDBNull(6)) ? INVALID_DT :
                        e.dtTim1 = DateTime.Parse(reader.GetString(6));
                    e.dtTim2 = (reader.IsDBNull(7)) ? INVALID_DT :
                        DateTime.Parse(reader.GetString(7));
                    if (!reader.IsDBNull(8))
                        e.uGrade = reader.GetUInt16(8);
                    if (!reader.IsDBNull(9))
                        e.tComp = reader.GetString(9);
                    vRoom[e.uRId].vExaminee.Add((short)(e.Lvl * e.uId), e);
                }
                reader.Close();
            }
            DBConnect.Close(ref conn);
        }
    }
}
