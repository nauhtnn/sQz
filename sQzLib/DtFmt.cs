using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace sQzLib
{
    public class DtFmt
    {
        static CultureInfo sCultInfo = null;
        public const int INV = 0;
        public static DateTime INV_H = DateTime.Parse("1000-01-01 00:00:00");//h = m = INVALID
        public static DateTime INV_ = DateTime.Parse("1000-01-01");
        public const string h = "H:m";
        public const string hh = "HH:mm";
        public const string H = "yyyy-M-d H:m";
        public const string HS = "yyyy-M-d H:m:s";
        public const string _ = "yyyy-M-d";
        public const string __ = "yyyy-MM-dd";
        public const string RH = "d-M-yyyy HH:mm";
        public const string R = "d-M-yyyy";

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
                return dt.ToString(_);
            return INV_.ToString(_);
        }
    }
}
