using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

/*
CREATE TABLE IF NOT EXISTS `board` (`dt` DATE PRIMARY KEY);
*/

namespace sQzLib
{
    public class ExamBoard
    {
        public DateTime mDt;
        Dictionary<uint, ExamSlot> vSl;
        Dictionary<uint, ExamSlotView> vSlVw;

        public ExamBoard()
        {
            vSl = new Dictionary<uint, ExamSlot>();
            vSlVw = new Dictionary<uint, ExamSlotView>();
        }

        public int DBIns(out string eMsg)
        {
            string v = "('" + mDt.ToString(DtFmt.H) + "')";
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
            {
                eMsg = Txt.s._[(int)TxI.DB_NOK];
                return 0;
            }
            int n = DBConnect.Ins(conn, "board", "dt", v, out eMsg);
            DBConnect.Close(ref conn);
            return n;
        }

        public static List<DateTime> DBSel()
        {
            List<DateTime> r = new List<DateTime>();
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return r;
            string qry = DBConnect.mkQrySelect("board", null, null, null);
            MySqlDataReader reader = DBConnect.exeQrySelect(conn, qry);
            while (reader.Read())
                r.Add(reader.GetDateTime(0));
            reader.Close();
            DBConnect.Close(ref conn);
            return r;
        }

        public List<DateTime> DBSelSl()
        {
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return null;
            string qry = DBConnect.mkQrySelect("slot", "t,open",
                "dt='" + mDt.ToString(DtFmt._) + "'", null);
            MySqlDataReader reader = DBConnect.exeQrySelect(conn, qry);
            if (reader == null)
            {
                DBConnect.Close(ref conn);
                return null;
            }
            List<DateTime> r = new List<DateTime>();
            while (reader.Read())
            {
                ExamSlot sl = new ExamSlot();
                DtFmt.ToDt(mDt.ToString(DtFmt._) + ' ' +
                    reader.GetDateTime(0).ToString(DtFmt.h), DtFmt.H, out sl.mDt);
                sl.bOpen = reader.GetBoolean(1);
                r.Add(sl.mDt);
            }
            reader.Close();
            DBConnect.Close(ref conn);
            return r;
        }

        public int DBInsSl(DateTime t, out string eMsg)
        {
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
            {
                eMsg = Txt.s._[(int)TxI.DB_NOK];
                return 0;
            }
            string v = "('" + mDt.ToString(DtFmt._) + "','"
                + t.ToString(DtFmt.h) + "')";
            int n = DBConnect.Ins(conn, "slot", "dt,t", v, out eMsg);
            DBConnect.Close(ref conn);
            return n;
        }
    }
}
