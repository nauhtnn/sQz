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
        public Dictionary<string, ExamSlot> vSl;

        public ExamBoard()
        {
            vSl = new Dictionary<string, ExamSlot>();
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
            if (n == -1062)
                eMsg = Txt.s._[(int)TxI.DB_EXCPT] + Txt.s._[(int)TxI.BOARD_EXIST];
            return n;
        }

        public static List<DateTime> DBSel(out string eMsg)
        {
            List<DateTime> r = new List<DateTime>();
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
            {
                eMsg = Txt.s._[(int)TxI.DB_NOK];
                return null;
            }
            string qry = DBConnect.mkQrySelect("board", null, null, null);
            MySqlDataReader reader = DBConnect.exeQrySelect(conn, qry, out eMsg);
            if(reader == null)
            {
                DBConnect.Close(ref conn);
                return null;
            }
            while (reader.Read())
                r.Add(reader.GetDateTime(0));
            reader.Close();
            DBConnect.Close(ref conn);
            return r;
        }

        public List<DateTime> DBSelSl(out string eMsg)
        {
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
            {
                eMsg = Txt.s._[(int)TxI.DB_NOK];
                return null;
            }
            string qry = DBConnect.mkQrySelect("slot", "t,open",
                "dt='" + mDt.ToString(DtFmt._) + "'", null);
            MySqlDataReader reader = DBConnect.exeQrySelect(conn, qry, out eMsg);
            if (reader == null)
            {
                DBConnect.Close(ref conn);
                return null;
            }
            List<DateTime> r = new List<DateTime>();
            while (reader.Read())
            {
                ExamSlot sl = new ExamSlot();
                string s = reader.GetString(0);
                DtFmt.ToDt(mDt.ToString(DtFmt._) + ' ' +
                    s, DtFmt.HS, out sl.mDt);
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
            if (n == -1062)
                eMsg = Txt.s._[(int)TxI.DB_EXCPT] + Txt.s._[(int)TxI.SLOT_EXIST];
            return n;
        }

        public List<DateTime> ListSl()
        {
            List<DateTime> r = new List<DateTime>();
            foreach (ExamSlot sl in vSl.Values)
                r.Add(sl.mDt);
            return r;
        }

        public byte[] ToByteR1(int rId)
        {
            List<byte[]> l = new List<byte[]>();
            int sz;
            byte[] b;
            foreach (ExamSlot sl in vSl.Values)
            {
                List<byte[]> x = sl.ToByteR1(rId);
                sz = ExamSlot.BYTE_COUNT_DT;
                foreach (byte[] a in x)
                    sz += a.Length;
                b = new byte[sz + 4];
                sz = 0;
                ExamSlot.ToByteDt(b, ref sz, sl.mDt);
                Array.Copy(BitConverter.GetBytes(b.Length - ExamSlot.BYTE_COUNT_DT - 4), 0, b, sz, 4);
                sz += 4;
                foreach (byte[] a in x)
                {
                    Array.Copy(a, 0, b, sz, a.Length);
                    sz += a.Length;
                }
                l.Add(b);
            }
            sz = 0;
            foreach (byte[] a in l)
                sz += a.Length;
            b = new byte[sz];
            sz = 0;
            foreach (byte[] a in l)
            {
                Buffer.BlockCopy(a, 0, b, sz, a.Length);
                sz += a.Length;
            }
            return b;
        }

        public bool ReadByteR1(byte[] buf, ref int offs)
        {
            DateTime dt;
            while(ExamSlot.BYTE_COUNT_DT < buf.Length - offs)
            {
                if (ExamSlot.ReadByteDt(buf, ref offs, out dt))
                    return true;
                ExamSlot sl;
                if(vSl.TryGetValue(dt.ToString(DtFmt.hh), out sl))
                {
                    if (sl.ReadByteR1(buf, ref offs))
                        return true;
                }
                else
                {
                    sl = new ExamSlot();
                    sl.mDt = dt;
                    if (sl.ReadByteR1(buf, ref offs))
                        return true;
                    vSl.Add(sl.mDt.ToString(DtFmt.hh), sl);
                }
            }
            if (offs == buf.Length)
                return false;
            else
                return true;
        }

        public byte[] ToByteQPack()
        {
            List<byte[]> l = new List<byte[]>();
            int sz;
            byte[] b;
            foreach (ExamSlot sl in vSl.Values)
            {
                List<byte[]> x = sl.ToByteQPack();
                sz = ExamSlot.BYTE_COUNT_DT;
                foreach (byte[] a in x)
                    sz += a.Length;
                b = new byte[sz + 4];
                sz = 0;
                ExamSlot.ToByteDt(b, ref sz, sl.mDt);
                Array.Copy(BitConverter.GetBytes(b.Length - ExamSlot.BYTE_COUNT_DT - 4), 0, b, sz, 4);
                sz += 4;
                foreach (byte[] a in x)
                {
                    Array.Copy(a, 0, b, sz, a.Length);
                    sz += a.Length;
                }
                l.Add(b);
            }
            sz = 0;
            foreach (byte[] a in l)
                sz += a.Length;
            b = new byte[sz];
            sz = 0;
            foreach (byte[] a in l)
            {
                Buffer.BlockCopy(a, 0, b, sz, a.Length);
                sz += a.Length;
            }
            return b;
        }

        public bool ReadByteQPack(byte[] buf, ref int offs)
        {
            DateTime dt;
            while (ExamSlot.BYTE_COUNT_DT < buf.Length - offs)
            {
                if (ExamSlot.ReadByteDt(buf, ref offs, out dt))
                    return true;
                ExamSlot sl;
                if(!vSl.TryGetValue(dt.ToString(DtFmt.hh), out sl) ||
                    sl.ReadByteQPack(buf, ref offs))
                    return true;
            }
            if (offs == buf.Length)
                return false;
            else
                return true;
        }
    }
}
