using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace sQzLib
{
    public class Date
    {
        /*
         CREATE TABLE IF NOT EXISTS `dates` (`idx` INT(4) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
          `date` DATE, `slot` TINYINT(1));
         */
        public static List<uint> svIdx = new List<uint>();
        public static List<string> svDate = new List<string>();
        public static byte[] sbArr = null;
        private static int snDate = 0;//redundant but convenient
        public static uint sDBIdx = uint.MaxValue;
		private static int sDD = 0, sMM = 0, sYYYY = 0;

        public static bool Chk224(string s)
        {
            if (s == null)
                return false;
            if (s.Length < 10)
                return false;
            int n = 0;
            int offs = 0;
            if (!int.TryParse(s.Substring(offs, 2), out sDD))
                return false;
            offs += 3;
            if (!int.TryParse(s.Substring(offs, 2), out sMM))
                return false;
            offs += 3;
            if (!int.TryParse(s.Substring(offs, 4), out sYYYY))
                return false;
            return true;
        }
		private static bool Chk422(string s)
        {
            if (s == null)
                return false;
            if (s.Length < 10)
                return false;
            int n = 0;
            int offs = 0;
            if (!int.TryParse(s.Substring(offs, 4), out sYYYY))
                return false;
            offs += 5;
            if (!int.TryParse(s.Substring(offs, 2), out sMM))
                return false;
            offs += 3;
            if (!int.TryParse(s.Substring(offs, 2), out sDD))
                return false;
            return true;
        }
		public static string To422(string s) {
			if(!Chk224(s))
				return null;
			else
				return "" + sYYYY + '-' + sMM + '-' + sDD;
		}
		public static string To224(string s) {
			if(!Chk422(s))
				return null;
			else
				return "" + sDD + '/' + sMM + '/' + sYYYY;
		}
        public static void Select(string date)
        {
            int i = svDate.IndexOf(date);
            sDBIdx = svIdx[i];
            sbArr = Encoding.UTF32.GetBytes(date);
        }
        public static void DBInsert(string date)
        {
			date = To422(date);
			if(date == null)
				return;
            date = "'" + date + "'";
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return;
            DBConnect.Ins(conn, "dates", "date", date);
            DBConnect.Close(ref conn);
        }

        public static void DBSelect()
        {
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return;
            string qry = DBConnect.mkQrySelect("dates", null, null, null, null);
            MySqlDataReader reader = DBConnect.exeQrySelect(conn, qry);
            svIdx.Clear();
            svDate.Clear();
            string s;
            while (reader.Read())
            {
                try { s = reader.GetDateTime(1).ToString("dd/MM/yyyy"); }
                catch(FormatException) { s = null; }
                if (s != null && 9 < s.Length)
                {
                    svIdx.Add(reader.GetUInt32(0));//hardcode
                    svDate.Add(s.Substring(0, 10));//suppose ok
                }
            }
            reader.Close();
            DBConnect.Close(ref conn);
        }
        public static void ReadByteArr(byte[] buf, ref int offs, int l)
        {
            if(snDate == 0)
                snDate = Encoding.UTF32.GetByteCount("2017/01/01");//learn from example
            if (l < snDate)
                return;
            sbArr = new byte[snDate];
            Buffer.BlockCopy(buf, offs, sbArr, 0, snDate);
            offs += snDate;
        }
    }
}
