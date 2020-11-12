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
        public static DateTime INV_ = DateTime.Parse("1000-01-01");
        //public const string h = "H:m";
        //public const string hh = "HH:mm";
        //public const string hs = "H:m:s";
        public const string H = "yyyy-M-d HH:mm";
        //public const string HS = "yyyy-M-d H:m:s";
        //public const string _ = "yyyy-M-d";
        //public const string __ = "yyyy-MM-dd";
        //public const string RR = "dd-MM-yyyy";
        public const int BYTE_COUNT = 12;
        public const int BYTE_COUNT_h = 8;

        public static bool To_(string s, out DateTime dt)
        {
            if (DateTime.TryParseExact(s, CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern + " " +
                CultureInfo.CurrentCulture.DateTimeFormat.LongTimePattern, null, DateTimeStyles.None, out dt))
                return false;
            return true;
        }

        public static string ToS(string s)
        {
            DateTime dt;
            if (!To_(s, out dt))
                return dt.ToString(H);
            return INV_.ToString(H);
        }

        public static bool Toh(string s, string form, out DateTime t)
        {
            if (sCultInfo == null)
                sCultInfo = CultureInfo.CreateSpecificCulture("en-US");
            if (DateTime.TryParseExact(s, form, sCultInfo, DateTimeStyles.None, out t))
                return false;
            return true;
        }

        public static bool ToByte(byte[] buf, ref int offs, DateTime dt)
        {
            if (buf.Length < 12)
                return true;
            Array.Copy(BitConverter.GetBytes(dt.Year), 0, buf, offs, 4);
            offs += 4;
            Array.Copy(BitConverter.GetBytes(dt.Month), 0, buf, offs, 4);
            offs += 4;
            Array.Copy(BitConverter.GetBytes(dt.Day), 0, buf, offs, 4);
            offs += 4;
            return false;
        }

        //public static bool ReadByte(byte[] buf, ref int offs, out DateTime dt)
        //{
        //    if (buf.Length - offs < 12)
        //    {
        //        dt = INV_;
        //        return true;
        //    }
        //    int y = BitConverter.ToInt32(buf, offs);
        //    offs += 4;
        //    int M = BitConverter.ToInt32(buf, offs);
        //    offs += 4;
        //    int d = BitConverter.ToInt32(buf, offs);
        //    offs += 4;
        //    if (To_(y.ToString("d4") + '-' + M.ToString("d2") + '-' + d.ToString("d2"), _, out dt))
        //        return true;
        //    return false;
        //}

        public static bool ToByteh(byte[] buf, ref int offs, DateTime dt)
        {
            if (buf.Length < 8)
                return true;
            Array.Copy(BitConverter.GetBytes(dt.Hour), 0, buf, offs, 4);
            offs += 4;
            Array.Copy(BitConverter.GetBytes(dt.Minute), 0, buf, offs, 4);
            offs += 4;
            return false;
        }

        public static byte[] ToByteh(DateTime dt)
        {
            byte[] buf = new byte[8];
            Array.Copy(BitConverter.GetBytes(dt.Hour), 0, buf, 0, 4);
            Array.Copy(BitConverter.GetBytes(dt.Minute), 0, buf, 4, 4);
            return buf;
        }

        //public static bool ReadByteh(byte[] buf, ref int offs, out DateTime dt)
        //{
        //    if (buf.Length - offs < 8)
        //    {
        //        dt = INV_;
        //        return true;
        //    }
        //    int H = BitConverter.ToInt32(buf, offs);
        //    offs += 4;
        //    int m = BitConverter.ToInt32(buf, offs);
        //    offs += 4;
        //    if (To_(H.ToString("d2") + ':' + m.ToString("d2"), hh, out dt))
        //        return true;
        //    return false;
        //}
    }
}
