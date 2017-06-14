using System;
using System.Collections.Generic;
using System.Windows.Controls;
using MySql.Data.MySqlClient;
using System.Windows.Media;
using System.Windows;
using System.Text;

namespace sQzLib
{
    public class ExamSlot
    {
        DateTime mDt;
        public bool bOpen;
        public Dictionary<ExamLv, QuestPack> vQPack;

        public const int BYTE_COUNT_DT = 8;

        public AnsPack mKeyPack;

        public SortedList<int, ExamRoom> vRoom;

        public ExamSlot()
        {
            mDt = DT.INV_;
            vRoom = new SortedList<int, ExamRoom>();
            vQPack = new Dictionary<ExamLv, QuestPack>();
            QuestPack p = new QuestPack();
            p.eLv = ExamLv.A;
            vQPack.Add(p.eLv, p);
            p = new QuestPack();
            p.eLv = ExamLv.B;
            vQPack.Add(p.eLv, p);

            mKeyPack = new AnsPack();
        }

        public string DBSelRoom()
        {
            string emsg;
            List<int> rids = ExamRoom.DBSel(out emsg);
            if (rids == null)
                return emsg;
            foreach (int i in rids)
            {
                ExamRoom r = new ExamRoom();
                r.uId = i;
                if(!vRoom.ContainsKey(i))
                    vRoom.Add(i, r);
            }
            return null;
        }

        public static void ToByteDt(byte[] buf, ref int offs, DateTime dt)
        {
            Array.Copy(BitConverter.GetBytes(dt.Hour), 0, buf, offs, 4);
            offs += 4;
            Array.Copy(BitConverter.GetBytes(dt.Minute), 0, buf, offs, 4);
            offs += 4;
        }

        public static bool ReadByteDt(byte[] buf, ref int offs, out DateTime dt)
        {
            if (buf.Length - offs < BYTE_COUNT_DT)
            {
                dt = DT.INV_;
                return true;
            }
            int H = BitConverter.ToInt32(buf, offs);
            offs += 4;
            int m = BitConverter.ToInt32(buf, offs);
            offs += 4;
            if (DT.To_(H.ToString("d2") + ':' + m.ToString("d2"), DT.hh, out dt))
                return true;
            return false;
        }

        public DateTime Dt {
            get { return mDt; }
            set {
                mDt = value;
                foreach (QuestPack p in vQPack.Values)
                    p.mDt = value;
            }
        }

        public List<byte[]> ToByteR1(int rId)
        {
            List<byte[]> l = new List<byte[]>();
            ExamRoom r;
            if (rId == 0)
                foreach (ExamRoom i in vRoom.Values)
                {
                    byte[] a = i.ToByteS1();
                    if (3 < a.Length)
                    {
                        l.Add(BitConverter.GetBytes(i.uId));
                        l.Add(a);
                    }
                }
            else if (vRoom.TryGetValue(rId, out r))
            {
                l.Add(BitConverter.GetBytes(rId));
                l.Add(r.ToByteS1());
            }
            return l;
        }

        public string ReadF(string fp, ref ExamSlot o)
        {
            string buf = Utils.ReadFile(fp);
            if (buf == null)
                return null;
            string[] vs = buf.Split('\n');
            StringBuilder eline = new StringBuilder();
            StringBuilder dup = new StringBuilder();
            int i = 0;
            foreach (string s in vs)
            {
                ++i;
                ExamineeS0 e = new ExamineeS0();
                string[] v = s.Split('\t');
                if (v.Length == 5)
                {
                    if (v[0].Length < 2)
                    {
                        eline.Append(i.ToString() + ", ");
                        continue;
                    }
                    v[0] = v[0].ToUpper();
                    if(!Enum.TryParse(v[0].Substring(0, 1), out e.eLv))
                    {
                        eline.Append(i.ToString() + ", ");
                        continue;
                    }
                    int urid;
                    if (!int.TryParse(v[0].Substring(1), out e.uId)
                        || !int.TryParse(v[1], out urid) || !vRoom.ContainsKey(urid))
                    {
                        eline.Append(i.ToString() + ", ");
                        continue;
                    }
                    if(vRoom[urid].vExaminee.ContainsKey(e.uId) || o.vRoom[urid].vExaminee.ContainsKey(e.uId))
                    {
                        dup.Append(e.eLv.ToString() + e.uId + ", ");
                        continue;
                    }
                    e.mDt = mDt;
                    e.tName = v[2].Trim();
                    DateTime dt;
                    if(!DT.To_(v[3], DT._, out dt))
                    {
                        eline.Append(i.ToString() + ", ");
                        continue;
                    }
                    e.tBirdate = v[3];
                    e.tBirthplace = v[4].Trim();
                    if (e.tName.Length == 0 || e.tBirdate.Length == 0 || e.tBirthplace.Length == 0)
                    {
                        eline.Append(i.ToString() + ", ");
                        continue;
                    }
                    o.vRoom[urid].vExaminee.Add(e.uId, e);
                }
                else
                    eline.Append(i.ToString() + ", ");
            }
            StringBuilder r = new StringBuilder();
            if(0 < dup.Length)
            {
                dup.Remove(dup.Length - 2, 2);//remove the last comma
                r.Append("\n" + Txt.s._[(int)TxI.NEE_ID_EXIST]);
                r.Append(dup.ToString() + '.');
            }
            if (0 < eline.Length)
            {
                eline.Remove(eline.Length - 2, 2);//remove the last comma
                r.Append("\n" + Txt.s._[(int)TxI.NEE_ELINE]);
                r.Append(eline.ToString() + '.');
            }
            if (r.Length == 0)
                return null;
            else
                return Txt.s._[(int)TxI.NEE_FERR] + r.ToString();
        }

        public int DBInsNee(out string eMsg)
        {
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
            {
                eMsg = Txt.s._[(int)TxI.DB_NOK];
                return -1;
            }
            int v = 1;
            StringBuilder sb = new StringBuilder();
            foreach (ExamRoom r in vRoom.Values)
            {
                int n = DBConnect.Count(conn, "sqz_slot_room", "dt",
                    "dt='" + mDt.ToString(DT._) + "' AND t='" + mDt.ToString(DT.hh) +
                    "' AND rid=" + r.uId, out eMsg);
                if (n < 0)
                {
                    DBConnect.Close(ref conn);
                    return n;
                }
                else if (n == 0)
                    n = DBConnect.Ins(conn, "sqz_slot_room",
                        "dt,t,rid", "('" + mDt.ToString(DT._) + "','" + mDt.ToString(DT.hh) +
                        "'," + r.uId + ")", out eMsg);
                if(n < 0)
                {
                    DBConnect.Close(ref conn);
                    return n;
                }
                n = r.DBIns(conn, out eMsg);
                if (n < 0)
                {
                    string[] p = new string[2];
                    p[0] = r.uId.ToString();
                    if (n == -1062)
                        p[1] = Txt.s._[(int)TxI.NEE_EXIST];
                    else
                        p[1] = eMsg;
                    sb.AppendFormat(Txt.s._[(int)TxI.ROOM_DB_NOK] + '\n', p);
                    v = 0;
                }
            }
            eMsg = sb.ToString();
            DBConnect.Close(ref conn);
            return v;
        }

        public void DelNee()
        {
            StringBuilder sb = new StringBuilder();
            foreach (ExamRoom r in vRoom.Values)
                r.vExaminee.Clear();
        }

        public string DBDelNee()
        {
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return Txt.s._[(int)TxI.DB_NOK];
            StringBuilder sb = new StringBuilder();
            string eMsg;
            int n = DBConnect.Count(conn, "sqz_nee_qsheet AS a,sqz_examinee AS b",
                "a.dt", "a.dt='" + mDt.ToString(DT._) +
                "' AND t='" + mDt.ToString(DT.hh) + "' AND a.dt=b.dt AND a.neeid=b.id",
                out eMsg);
            sb.AppendFormat(Txt.s._[(int)TxI.SLOT], mDt.ToString(DT.hh));
            if (0 < n)
            {
                sb.Append(Txt.s._[(int)TxI.SLOT_DEL_GRD] + '\n');
                DBConnect.Close(ref conn);
                return sb.ToString();
            }
            else if (n < 0)
            {
                sb.Append(Txt.s._[(int)TxI.SLOT_DEL_ECPT] + eMsg);
                DBConnect.Close(ref conn);
                return sb.ToString();
            }
            n = DBConnect.Delete(conn, "sqz_examinee",
                "dt='" + mDt.ToString(DT._) + "' AND t='" + mDt.ToString(DT.hh) + "'", out eMsg);
            if (n < 0)
                sb.Append(Txt.s._[(int)TxI.SLOT_DEL_ECPT] + eMsg);
            else
                sb.AppendFormat(Txt.s._[(int)TxI.SLOT_DEL_N], n.ToString());
            DBConnect.Close(ref conn);
            return sb.ToString();
        }

        public void DBSelNee()
        {
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return;
            foreach (ExamRoom r in vRoom.Values)
            {
                r.vExaminee.Clear();
                string qry = DBConnect.mkQrySelect("sqz_slot_room AS a,sqz_examinee AS b",
                    "lv,id,name,birdate,birthplace", "a.rid=" + r.uId +
                    " AND a.dt='" + mDt.ToString(DT._) + "' AND a.t='" + mDt.ToString(DT.hh) +
                    "' AND a.dt=b.dt AND a.t=b.t AND a.rid=b.rid");
                string emsg;
                MySqlDataReader reader = DBConnect.exeQrySelect(conn, qry, out emsg);
                if (reader != null)
                {
                    while (reader.Read())
                    {
                        ExamineeS0 e = new ExamineeS0();
                        e.mDt = Dt;
                        if (!Enum.TryParse(reader.GetString(0), out e.eLv))
                            continue;
                        e.uId = reader.GetUInt16(1);
                        e.tName = reader.GetString(2);
                        e.tBirdate = reader.GetDateTime(3).ToString(DT.RR);
                        e.tBirthplace = reader.GetString(4);
                        r.vExaminee.Add(e.uId, e);
                    }
                    reader.Close();
                }
            }
            DBConnect.Close(ref conn);
        }

        public void ReadByteR0(byte[] buf, ref int offs)
        {
            List<ExamineeA> v = new List<ExamineeA>();
            List<ExamineeA> l = new List<ExamineeA>();
            while (true)
            {
                if (buf.Length - offs < 4)
                    break;
                int rId = BitConverter.ToInt32(buf, offs);
                offs += 4;
                ExamRoom r;
                if (!vRoom.TryGetValue(rId, out r))
                    break;
                if (r.ReadByteS0(buf, ref offs, ref v))
                    break;
                foreach (ExamineeS0 e in v)
                {
                    ExamineeA o;
                    bool unfound = true;
                    foreach (ExamRoom i in vRoom.Values)
                        if (i.uId != rId && i.vExaminee.TryGetValue(e.uId, out o))
                        {
                            unfound = false;
                            //o.bFromC = false;
                            o.Merge(e);
                            break;
                        }
                    if (unfound)
                        l.Add(e);
                }
                v.Clear();
            }
        }

        public bool ReadByteR1(byte[] buf, ref int offs)
        {
            if (buf.Length - offs < 4)
                return true;
            int l = BitConverter.ToInt32(buf, offs);
            offs += 4;
            l += offs;
            while (offs < l)
            {
                int rId = BitConverter.ToInt32(buf, offs);
                offs += 4;
                ExamRoom r;
                if (vRoom.TryGetValue(rId, out r))
                    r.ReadByteS1(buf, ref offs);
                else
                {
                    r = new ExamRoom();
                    r.uId = rId;
                    r.ReadByteS1(buf, ref offs);
                    vRoom.Add(rId, r);
                }
            }
            if (offs == l)
                return false;
            else
                return true;
        }

        public void DBUpdateRs()
        {
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return;
            foreach (ExamRoom r in vRoom.Values)
                r.DBUpdateRs(conn);
            DBConnect.Close(ref conn);
        }

        public bool GenQPack(int n, ExamLv lv, int[] vn)
        {
            List<QuestSheet> l = vQPack[lv].GenQPack(n, vn);
            mKeyPack.ExtractKey(l);
            return false;
        }

        public List<byte[]> ToByteQPack()
        {
            List<byte[]> l = new List<byte[]>();
            foreach (QuestPack p in vQPack.Values)
            {
                l.Add(BitConverter.GetBytes((int)p.eLv));
                l.Add(p.ToByte());
            }
            //int sz = 0;
            //foreach (byte[] x in l)
            //    sz += x.Length;
            //byte[] buf = new byte[sz];
            //int offs = 0;
            //foreach(byte[] x in l)
            //{
            //    Array.Copy(x, 0, buf, offs, x.Length);
            //    offs += x.Length;
            //}
            //return buf;
            return l;
        }

        public bool ReadByteQPack(byte[] buf, ref int offs)
        {
            if (buf.Length - offs < 4)
                return true;
            int l = BitConverter.ToInt32(buf, offs);
            offs += 4;
            l += offs;
            while (offs < l)
            {
                int x;
                ExamLv lv;
                if (Enum.IsDefined(typeof(ExamLv), x = BitConverter.ToInt32(buf, offs)))
                    lv = (ExamLv)x;
                else
                    return true;
                offs += 4;
                if (vQPack[lv].ReadByte(buf, ref offs))
                    return true;
            }
            if (offs == l)
                return false;
            else
                return true;
        }

        public bool ReadByteQPack1(ExamLv lv, byte[] buf, ref int offs)
        {
            if (vQPack[lv].ReadByte1(buf, ref offs))
                return true;
            return false;
        }

        public byte[] ToByteKey()
        {
            return mKeyPack.ToByte();
        }

        public bool ReadByteKey(byte[] buf, ref int offs)
        {
            return mKeyPack.ReadByte(buf, ref offs);
        }

        public ExamineeA Signin(ExamineeA e)
        {
            ExamineeA o;
            foreach (ExamRoom r in vRoom.Values)
                if ((o = r.Signin(e)) != null)
                    return o;
            return null;
        }

        public ExamineeA Find(int lvid)
        {
            ExamineeA o;
            foreach (ExamRoom r in vRoom.Values)
                if (r.vExaminee.TryGetValue(lvid, out o))
                    return o;
            return null;
        }

        public byte[] ToByteR0(byte[] pre)
        {
            List<byte[]> l = new List<byte[]>();
            foreach (ExamRoom r in vRoom.Values)
            {
                byte[] prefx = BitConverter.GetBytes(r.uId);
                byte[] b;
                r.ToByteS0(prefx, out b);
                l.Add(b);
            }
            int sz = pre.Length;
            foreach (byte[] b in l)
                sz += b.Length;
            //byte[] buf = new byte[sz + 4];
            byte[] buf = new byte[sz];
            sz = 0;
            //Buffer.BlockCopy(BitConverter.GetBytes(vRoom.Count), 0, buf, sz, 4);
            Buffer.BlockCopy(pre, 0, buf, sz, pre.Length);
            sz += pre.Length;
            foreach (byte[] i in l)
            {
                Buffer.BlockCopy(i, 0, buf, sz, i.Length);
                sz += i.Length;
            }
            return buf;
        }
    }
}
