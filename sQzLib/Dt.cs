using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace sQzLib
{
    public class DT
    {
        static CultureInfo sCultInfo = null;
        public const int INV = 0;
        public static DateTime INV_H = DateTime.Parse("1000-01-01 00:00:00");//h = m = INVALID
        public static DateTime INVALID = DateTime.Parse("1000-01-01");
        public static readonly string SYSTEM_DT_FMT = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern + " " +
                CultureInfo.CurrentCulture.DateTimeFormat.LongTimePattern;
        public const string h = "H:m";
        public const string hh = "HH:mm";
        public const string hs = "H:m:s";
        //public const string H = "yyyy-M-d HH:mm";
        //public const string HS = "yyyy-M-d H:m:s";
        public const string _ = "yyyy-M-d HH:mm";
        //public const string _ = "yyyy-M-d";
        //public const string __ = "yyyy-MM-dd";
        public const string RR = "dd-MM-yyyy";
        //public const int BYTE_COUNT = 12;
        //public const int BYTE_COUNT_h = 8;

        public static bool To_(string s, out DateTime dt)
        {
            return To_(s, _, out dt);
        }

        public static bool To_(string s, string format, out DateTime dt)
        {
            if (DateTime.TryParseExact(s, format, null, DateTimeStyles.None, out dt))
                return false;
            return true;
        }

        public static string ToS(string s)
        {
            DateTime dt;
            if (!To_(s, out dt))
                return dt.ToString(_);
            return INVALID.ToString(_);
        }

        public static bool Toh(string s, string form, out DateTime t)
        {
            if (sCultInfo == null)
                sCultInfo = CultureInfo.CreateSpecificCulture("en-US");
            if (DateTime.TryParseExact(s, form, sCultInfo, DateTimeStyles.None, out t))
                return false;
            return true;
        }

        public static byte[] GetBytes(DateTime dt)
        {
            byte[] buf = new byte[sizeof(long)];
            Array.Copy(BitConverter.GetBytes(dt.ToBinary()), 0, buf, 0, sizeof(long));
            return buf;
        }

        public static bool CopyBytesToBuffer(byte[] buf, ref int offs, DateTime dt)
        {
            if (buf.Length < sizeof(long))
                return true;
            Array.Copy(BitConverter.GetBytes(dt.ToBinary()), 0, buf, offs, sizeof(long));
            offs += sizeof(long);
            return false;
        }

        public static DateTime ReadByte(byte[] buf, ref int offs)
        {
            if (buf.Length - offs < sizeof(long))
                return INVALID;
            offs += sizeof(long);
            return DateTime.FromBinary(BitConverter.ToInt64(buf, offs - sizeof(long)));
        }

        public static string CreateNameFromDateTime(string dt)
        {
            return "_" + dt.Replace(':', '_').Replace('-', '_').Replace(' ', '_');
        }
    }
}
