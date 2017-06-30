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

        public List<DateTime> DBSelSl(bool arch, out string eMsg)
        {
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
            {
                eMsg = Txt.s._[(int)TxI.DB_NOK];
                return null;
            }
            string qry = "dt='" + mDt.ToString(DT._) + "' AND stt";
            if (arch)
                qry = qry + "=" + (int)ExamStt.Arch;
            else
                qry = qry + "!=" + (int)ExamStt.Arch;
            qry = DBConnect.mkQrySelect("sqz_slot", "t", qry);
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
                + t.ToString(DT.h) + "'," + (int)ExamStt.Prep + ")";
            int n = DBConnect.Ins(conn, "sqz_slot", "dt,t,stt", v, out eMsg);
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
            if (DT.ReadByte(buf, ref offs, out mDt))
                return true;
            if (buf.Length - offs < 4)
                return true;
            int n = BitConverter.ToInt32(buf, offs);
            offs += 4;
            while(0 < n)
            {
                --n;
                DateTime dt;
                if (DT.ReadByteh(buf, ref offs, out dt))
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

        public byte[] ToByteSl0(byte[] prefx)
        {
            List<byte[]> l = new List<byte[]>();
            l.Add(BitConverter.GetBytes(vSl.Count));
            foreach (ExamSlot sl in vSl.Values)
                l.InsertRange(l.Count, sl.ToByteR0());
            int sz = prefx.Length + DT.BYTE_COUNT;
            foreach (byte[] x in l)
                sz += x.Length;
            byte[] buf = new byte[sz];
            sz = 0;
            if(prefx != null)
            {
                Buffer.BlockCopy(prefx, 0, buf, sz, prefx.Length);
                sz += prefx.Length;
            }
            DT.ToByte(buf, ref sz, mDt);
            foreach (byte[] x in l)
            {
                Buffer.BlockCopy(x, 0, buf, sz, x.Length);
                sz += x.Length;
            }
            return buf;
        }

        public int ReadByteSl0(byte[] buf, ref int offs)
        {
            DateTime dt;
            if (DT.ReadByte(buf, ref offs, out dt) || dt != mDt)
                return -1;
            if (buf.Length - offs < 4)
                return -1;
            int n = BitConverter.ToInt32(buf, offs);
            offs += 4;
            int rid = -1;
            while (0 < n)
            {
                --n;
                if (DT.ReadByteh(buf, ref offs, out dt))
                    return -1;
                ExamSlot sl;
                if (vSl.TryGetValue(dt.ToString(DT.hh), out sl) &&
                    (rid = sl.ReadByteR0(buf, ref offs)) < 0)
                        return -1;
            }
            if (n == 0)
                return rid;
            else
                return -1;
        }

        public byte[] ToByteQPack(int rid)
        {
            List<byte[]> l = new List<byte[]>();
            l.Add(BitConverter.GetBytes(vSl.Count));
            foreach (ExamSlot sl in vSl.Values)
                l.InsertRange(l.Count, sl.ToByteQPack(rid));
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

        public bool ReadByteQPack(byte[] buf, ref int offs)
        {
            DateTime dt;
            if (DT.ReadByte(buf, ref offs, out dt) || dt != mDt)
                return true;
            if (buf.Length - offs < 4)
                return true;
            int n = BitConverter.ToInt32(buf, offs);
            offs += 4;
            while (0 < n)
            {
                --n;
                if (DT.ReadByteh(buf, ref offs, out dt))
                    return true;
                ExamSlot sl;
                if(!vSl.TryGetValue(dt.ToString(DT.hh), out sl) ||
                    sl.ReadByteQPack(buf, ref offs))
                    return true;
            }
            if (n == 0)
                return false;
            else
                return true;
        }

        public byte[] ToByteKey()
        {
            List<byte[]> l = new List<byte[]>();
            l.Add(BitConverter.GetBytes(vSl.Count));
            foreach (ExamSlot sl in vSl.Values)
                l.InsertRange(l.Count, sl.ToByteKey());
            int sz = DT.BYTE_COUNT;
            foreach (byte[] a in l)
                sz += a.Length;
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

        public bool ReadByteKey(byte[] buf, ref int offs)
        {
            DateTime dt;
            if (DT.ReadByte(buf, ref offs, out dt) || dt != mDt)
                return true;
            if (buf.Length - offs < 4)
                return true;
            int n = BitConverter.ToInt32(buf, offs);
            offs += 4;
            while (0 < n)
            {
                --n;
                if (DT.ReadByteh(buf, ref offs, out dt))
                    return true;
                ExamSlot sl;
                if (!vSl.TryGetValue(dt.ToString(DT.hh), out sl) ||
                    sl.ReadByteKey(buf, ref offs))
                    return true;
            }
            if (n == 0)
                return false;
            else
                return true;
        }

        public bool DBUpdateRs(int rid, out string eMsg)
        {
            MySqlConnection conn = DBConnect.Init();
            if(conn == null)
            {
                eMsg = Txt.s._[(int)TxI.DB_NOK];
                return true;
            }
            StringBuilder vals = new StringBuilder();
            foreach (ExamSlot sl in vSl.Values)
                sl.DBUpdateRs(rid, vals);
            bool rval;
            if (0 < vals.Length)
            {
                vals.Remove(vals.Length - 1, 1);//remove the last comma
                int rs;
                rval = (rs = DBConnect.Ins(conn, "sqz_nee_qsheet",
                    "dt,lv,neeid,qsid,t1,t2,grade,comp,ans", vals.ToString(), out eMsg)) < 0;
                if (rs == DBConnect.PRI_KEY_EXISTS)
                    eMsg = Txt.s._[(int)TxI.RS_UP_EXISTS];
            }
            else
            {
                rval = true;
                eMsg = Txt.s._[(int)TxI.RS_UP_NOTHING];
            }
            if(!rval)
            {
                foreach (ExamSlot sl in vSl.Values)
                    sl.DBUpT2(conn, rid, out eMsg);
            }
            DBConnect.Close(ref conn);
            return rval;
        }

        public void DBUpStt()
        {
            foreach(ExamSlot sl in vSl.Values)
            {
                bool bArch = true;
                foreach (ExamRoom r in sl.vRoom.Values)
                    if (0 < r.vExaminee.Count && r.t2.Hour == DT.INV)
                    {
                        bArch = false;
                        break;
                    }
                if (bArch)
                {
                    sl.eStt = ExamStt.Arch;
                    sl.DBUpStt();
                }
            }
        }
    }
}
