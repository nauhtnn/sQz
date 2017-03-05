using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApplication1
{
    class Date
    {
        static List<byte[]> svDate = new List<byte[]>();
        public static void ReadTxt()
        {
            ReadTxt(sQzCS.Utils.ReadFile("Dates.txt"));
        }
        public static void ReadTxt(string buf)
        {
            if (buf == null)
                return;
            string[] s = buf.Split('\n');
            svDate.Clear();
            for (int i = 0; i < s.Length; ++i)
                svDate.Add(Encoding.UTF32.GetBytes(s[i]));
        }
        public static void DBInsert()
        {
            DBConnect conn = new DBConnect();
            conn.OpenConnection();
        }
    }
}
