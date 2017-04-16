using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using MySql.Data.MySqlClient;

/*
CREATE TABLE IF NOT EXISTS `date` (`idx` INT(4) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
 `date` DATETIME);
*/

namespace sQzLib
{
    public class ExamDate
    {
        public DateTime mDt;
        static CultureInfo sCultInfo = null;
        public uint uId;
        public const int INVALID = 2016;
        public const string FORM_H = "dd/MM/yyyy HH:mm";
        public const string FORM = "dd/MM/yyyy";
        public const string FORM_MYSQL = "yyyy-MM-dd HH:00";

        public ExamDate()
        {
            Parse("01/01/2016 01:00", FORM_H, out mDt);
            uId = uint.MaxValue;
        }

        public void DBInsert()
        {
			string v = "('" + mDt.ToString(FORM_MYSQL) + "')";
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return;
            DBConnect.Ins(conn, "date", "date", v);
            DBConnect.Close(ref conn);
        }

        public Dictionary<uint, DateTime> DBSelect()
        {
            Dictionary<uint, DateTime> r = new Dictionary<uint, DateTime>();
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return r;
            string qry = DBConnect.mkQrySelect("date", null, null, null);
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
            if(buf.Length - offs < 16)
                return true;
            int y = BitConverter.ToInt32(buf, offs);
            offs += 4;
            int M = BitConverter.ToInt32(buf, offs);
            offs += 4;
            int d = BitConverter.ToInt32(buf, offs);
            offs += 4;
            int H = BitConverter.ToInt32(buf, offs);
            offs += 4;
            if (Parse(d.ToString("00") + '/' + M.ToString("00") + '/' + y.ToString("0000") +
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
            return "2016-01-01";
        }
    }
}
