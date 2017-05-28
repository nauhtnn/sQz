using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Globalization;
using MySql.Data.MySqlClient;
using System.Windows.Media;
using System.Windows;

namespace sQzLib
{
    public class DtFmt
    {
        public static CultureInfo sCultInfo = null;
        public const int INV = 0;
        public static DateTime INV_H = DateTime.Parse("2016-01-01 00:00");//h = m = INVALID
        public static DateTime INV_ = DateTime.Parse("2016-01-01");
        public const string h = "HH:mm";
        public const string H = "yyyy-MM-dd HH:mm";
        public const string HS = "yyyy-MM-dd HH:mm:ss";
        public const string _ = "yyyy-MM-dd";
        public const string SH = "yy-MM-dd HH:mm";
        public const string RH = "dd-MM-yyyy HH:mm";
        public const string R = "dd-MM-yyyy";

        public static bool ToDt(string s, string form, out DateTime dt)
        {
            if (sCultInfo == null)
                sCultInfo = CultureInfo.CreateSpecificCulture("en-US");
            if (DateTime.TryParseExact(s, form, sCultInfo, DateTimeStyles.None, out dt))
                return false;
            return true;
        }

        public static string ToDtMysql(string s, string curForm)
        {
            DateTime dt;
            if (!ToDt(s, curForm, out dt))
                return dt.ToString(H);
            return INV_.ToString(H);
        }
    }
}
