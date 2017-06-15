using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

//ToByte | [dt][nSl] | [siId][nR] | [riId][nNee] | [neeId][neeDat] |
//ReadByte | [dt][nSl][siId] | [nR][ridId] | [nNee][neeId] | [neeDat] |
//len vs n: len can verify len = offs, n cannot.
//  n is more readable for human.
//  Since every reading method has checked len before BitConverter.ToXX(buf, ref offs),
//      the aggregation len is ok.
//  The scheme in which containers check n, leaf nodes check len can assure both.
//Because containers use dictionary to store items,
//  the item's id is read by containers to locate the item.

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
            string v = "('" + mDt.ToString(DT._) + "')";
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
            {
                eMsg = Txt.s._[(int)TxI.DB_NOK];
                return 0;
            }
            int n = DBConnect.Ins(conn, "sqz_board", "dt", v, out eMsg);
            DBConnect.Close(ref conn);
            if (n == -1062)
                eMsg = Txt.s._[(int)TxI.DB_EXCPT] + Txt.s._[(int)TxI.BOARD_EXIST];
            return n;
        }

        public static List<DateTime> DBSel(out string eMsg)
        {
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
            {
                eMsg = Txt.s._[(int)TxI.DB_NOK];
                return null;
            }
            string qry = DBConnect.mkQrySelect("sqz_board", null, null);
            MySqlDataReader reader = DBConnect.exeQrySelect(conn, qry, out eMsg);
            if(reader == null)
            {
                DBConnect.Close(ref conn);
                return null;
            }
            List<DateTime> r = new List<DateTime>();
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
            string qry = DBConnect.mkQrySelect("sqz_slot", "t",
                "dt='" + mDt.ToString(DT._) + "'");
            MySqlDataReader reader = DBConnect.exeQrySelect(conn, qry, out eMsg);
            if (reader == null)
            {
                DBConnect.Close(ref conn);
                return null;
            }
            List<DateTime> r = new List<DateTime>();
            while (reader.Read())
            {
                string s = reader.GetString(0);
                DateTime dt;
                DT.To_(mDt.ToString(DT._) + ' ' +
                    s, DT.HS, out dt);
                r.Add(dt);
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
            string v = "('" + mDt.ToString(DT._) + "','"
                + t.ToString(DT.h) + "')";
            int n = DBConnect.Ins(conn, "sqz_slot", "dt,t", v, out eMsg);
            DBConnect.Close(ref conn);
            if (n == -1062)
                eMsg = Txt.s._[(int)TxI.DB_EXCPT] + Txt.s._[(int)TxI.SLOT_EXIST];
            return n;
        }

        public List<DateTime> ListSl()
        {
            List<DateTime> r = new List<DateTime>();
            foreach (ExamSlot sl in vSl.Values)
                r.Add(sl.Dt);
            return r;
        }

        public byte[] ToByteSl1(int rId)
        {
            List<byte[]> l = new List<byte[]>();
            l.Add(BitConverter.GetBytes(vSl.Count));
            foreach (ExamSlot sl in vSl.Values)
                l.InsertRange(l.Count, sl.ToByteR1(rId));
            int sz = DT.BYTE_COUNT;
            foreach (byte[] x in l)
                sz += x.Length;
            byte[] buf = new byte[sz];
            sz = 0;
            DT.ToByte(buf, ref sz, mDt);
            foreach (byte[] x in l)
            {
                Buffer.BlockCopy(x, 0, buf, sz, x.Length);
                sz += x.Length;
            }
            return buf;
        }

        public bool ReadByteSl1(byte[] buf, ref int offs)
        {
            DateTime dt;
            if (DT.ReadByte(buf, ref offs, out mDt))
                return true;
            if (buf.Length - offs < 4)
                return true;
            int n = BitConverter.ToInt32(buf, offs);
            offs += 4;
            while(0 < n)
            {
                --n;
                if (ExamSlot.ReadByteDt(buf, ref offs, out dt))
                    return true;
                ExamSlot sl;
                if(vSl.TryGetValue(dt.ToString(DT.hh), out sl))
                {
                    if (sl.ReadByteR1(buf, ref offs))
                        return true;
                }
                else
                {
                    sl = new ExamSlot();
                    sl.Dt = dt;
                    if (sl.ReadByteR1(buf, ref offs))
                        return true;
                    vSl.Add(sl.Dt.ToString(DT.hh), sl);
                }
            }
            if (n == 0)
                return false;
            else
                return true;
        }

        public byte[] ToByteSl0()
        {
            List<byte[]> l = new List<byte[]>();
            byte[] b;
            int sz;
            foreach (ExamSlot sl in vSl.Values)
                l.InsertRange(l.Count - 1, sl.ToByteR0());
            sz = 0;
            foreach (byte[] a in l)
                sz += a.Length;
            b = new byte[DT.BYTE_COUNT + 4 + sz];
            sz = 0;
            
            Array.Copy(BitConverter.GetBytes(vSl.Count), 0, b, sz, 4);
            sz += 4;
            foreach (byte[] a in l)
            {
                Buffer.BlockCopy(a, 0, b, sz, a.Length);
                sz += a.Length;
            }
            return b;
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
                ExamSlot.ToByteDt(b, ref sz, sl.Dt);
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
                if(!vSl.TryGetValue(dt.ToString(DT.hh), out sl) ||
                    sl.ReadByteQPack(buf, ref offs))
                    return true;
            }
            if (offs == buf.Length)
                return false;
            else
                return true;
        }

        public byte[] ToByteKey()
        {
            List<byte[]> l = new List<byte[]>();
            int sz;
            byte[] b;
            foreach (ExamSlot sl in vSl.Values)
            {
                byte[] x = sl.ToByteKey();
                b = new byte[ExamSlot.BYTE_COUNT_DT + x.Length];
                sz = 0;
                ExamSlot.ToByteDt(b, ref sz, sl.Dt);
                Array.Copy(x, 0, b, sz, x.Length);
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

        public bool ReadByteKey(byte[] buf, ref int offs)
        {
            DateTime dt;
            while (ExamSlot.BYTE_COUNT_DT < buf.Length - offs)
            {
                if (ExamSlot.ReadByteDt(buf, ref offs, out dt))
                    return true;
                ExamSlot sl;
                if (!vSl.TryGetValue(dt.ToString(DT.hh), out sl) ||
                    sl.ReadByteKey(buf, ref offs))
                    return true;
            }
            if (offs == buf.Length)
                return false;
            else
                return true;
        }
    }
}
