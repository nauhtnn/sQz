using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace WpfApplication1
{

    class Date
    {
        /*
         *format yyyy/mm/dd for sorting easily
        CREATE TABLE IF NOT EXISTS `dates` (`idx` INT(4) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
         `date` CHAR(10) CHARACTER SET `ascii`)
         */
        public static List<int> svIdx = new List<int>();
        public static List<string> svDate = new List<string>();
        const int nAttb = 2;//hardcode

        public static bool ChkFmt(string s)
        {
            if (s == null)
                return false;
            if (s.Length != 10)
                return false;
            int n = 0;
            int offs = 0;
            if (!int.TryParse(s.Substring(offs, 4), out n))
                return false;
            offs += 5;
            if (!int.TryParse(s.Substring(offs, 2), out n))
                return false;
            offs += 3;
            if (!int.TryParse(s.Substring(offs, 2), out n))
                return false;
            return true;
        }

        public static void DBInsert(string date)
        {
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
            string qry = DBConnect.mkQrySelect("dates", null, nAttb, null, null, null);
            MySqlDataReader reader = DBConnect.exeQrySelect(conn, qry);
            svIdx.Clear();
            svDate.Clear();
            while (reader.Read())
            {
                svIdx.Add(reader.GetInt32(0));//hardcode
                svDate.Add(reader.GetString(1));
            }
            reader.Close();
            DBConnect.Close(ref conn);
        }
    }
}
