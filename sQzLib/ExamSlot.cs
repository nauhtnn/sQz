using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Globalization;
using MySql.Data.MySqlClient;
using System.Windows.Media;
using System.Windows;

/*
CREATE TABLE IF NOT EXISTS `slot` (`id` INT(4) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
 `dt` DATETIME);
*/

namespace sQzLib
{
    public class ExamSlot
    {
        public DateTime mDt;
        static CultureInfo sCultInfo = null;
        public uint uId;
        public const int INVALID = 0;
        public static DateTime INVALID_DT = DateTime.Parse("2016/01/01 00:00");//h = m = INVALID
        public const string MYSQL_INVALID = "2016-01-01";
        public const string FORM_h = "H:m";
        public const string FORM_H = "yyyy/MM/dd HH:mm";
        public const string FORM = "yyyy/MM/dd";
        public const string FORM_RH = "dd/MM/yyyy HH:mm";
        public const string FORM_R = "dd/MM/yyyy";
        public const string FORM_MYSQL = "yyyy-MM-dd HH:00";

        public Dictionary<int, TextBlock> vGrade;
        public Dictionary<int, TextBlock> vDt1;
        public Dictionary<int, TextBlock> vDt2;
        public Dictionary<int, TextBlock> vComp;
        public Dictionary<ExamLv, QuestPack> vQPack;
        public AnsPack mKeyPack;

        public Dictionary<int, ExamRoom> vRoom;

        public ExamSlot()
        {
            mDt = INVALID_DT;
            uId = uint.MaxValue;

            vRoom = new Dictionary<int, ExamRoom>();
            for (int i = 1; i < 7; ++i)//todo: read from db
            {
                ExamRoom r = new ExamRoom();
                r.uId = i;
                vRoom.Add(i, r);
            }
            vGrade = new Dictionary<int, TextBlock>();
            vDt1 = new Dictionary<int, TextBlock>();
            vDt2 = new Dictionary<int, TextBlock>();
            vComp = new Dictionary<int, TextBlock>();
            vQPack = new Dictionary<ExamLv, QuestPack>();
            QuestPack p = new QuestPack();
            p.eLv = ExamLv.A;
            vQPack.Add(p.eLv, p);
            p = new QuestPack();
            p.eLv = ExamLv.B;
            vQPack.Add(p.eLv, p);

            mKeyPack = new AnsPack();
        }

        public void DBInsert()
        {
            string v = "('" + mDt.ToString(FORM_MYSQL) + "')";
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return;
            DBConnect.Ins(conn, "slot", "dt", v);
            DBConnect.Close(ref conn);
        }

        public Dictionary<uint, DateTime> DBSelect()
        {
            Dictionary<uint, DateTime> r = new Dictionary<uint, DateTime>();
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return r;
            string qry = DBConnect.mkQrySelect("slot", null, null, null);
            MySqlDataReader reader = DBConnect.exeQrySelect(conn, qry);
            while (reader.Read())
                r.Add(reader.GetUInt32(0), reader.GetDateTime(1));
            reader.Close();
            DBConnect.Close(ref conn);
            return r;
        }

        public int GetByteCountDt()
        {
            return 20;
        }

        public static void ToByteDt(byte[] buf, ref int offs, DateTime dt)
        {
            Array.Copy(BitConverter.GetBytes(dt.Year), 0, buf, offs, 4);
            offs += 4;
            Array.Copy(BitConverter.GetBytes(dt.Month), 0, buf, offs, 4);
            offs += 4;
            Array.Copy(BitConverter.GetBytes(dt.Day), 0, buf, offs, 4);
            offs += 4;
            Array.Copy(BitConverter.GetBytes(dt.Hour), 0, buf, offs, 4);
            offs += 4;
            Array.Copy(BitConverter.GetBytes(dt.Minute), 0, buf, offs, 4);
            offs += 4;
        }

        public static bool ReadByteDt(byte[] buf, ref int offs, out DateTime dt)
        {
            if (buf.Length - offs < 20)
            {
                dt = INVALID_DT;
                return true;
            }
            int y = BitConverter.ToInt32(buf, offs);
            offs += 4;
            int M = BitConverter.ToInt32(buf, offs);
            offs += 4;
            int d = BitConverter.ToInt32(buf, offs);
            offs += 4;
            int H = BitConverter.ToInt32(buf, offs);
            offs += 4;
            int m = BitConverter.ToInt32(buf, offs);
            offs += 4;
            if (Parse(y.ToString("d4") + '/' + M.ToString("d2") + '/' + d.ToString("d2") +
                ' ' + H.ToString("d2") + ':' + m.ToString("d2"), FORM_H, out dt))
                return true;
            return false;
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
                        l.Add(BitConverter.GetBytes(i.uId));//todo: optmz duplication
                        l.Add(a);
                    }
                }
            else if (vRoom.TryGetValue(rId, out r))
            {
                l.Add(BitConverter.GetBytes(rId));//todo: optmz duplication
                l.Add(r.ToByteS1());
            }
            return l;
        }

        public static bool Parse(string s, string form, out DateTime dt)
        {
            if (sCultInfo == null)
                sCultInfo = CultureInfo.CreateSpecificCulture("en-US");
            if (DateTime.TryParseExact(s, form, sCultInfo, DateTimeStyles.None, out dt))
                return false;
            return true;
        }

        public static string ToMysqlForm(string s, string curForm)
        {
            DateTime dt;
            if (!Parse(s, curForm, out dt))
                return dt.ToString(FORM_MYSQL);
            return MYSQL_INVALID;
        }

        public void ReadF(string fp)
        {
            string buf = Utils.ReadFile(fp);
            if (buf == null)
                return;
            string[] vs = buf.Split('\n');
            foreach (string s in vs)
            {
                ExamineeS0 e = new ExamineeS0();
                string[] v = s.Split('\t');
                if (v.Length == 5)
                {
                    if (v[0].Length < 2)
                        continue;
                    v[0] = v[0].ToUpper();
                    ExamLv x;
                    if(Enum.TryParse(v[0].Substring(0, 1), out x))
                        e.eLv = x;
                    else
                        continue;
                    int uRId;
                    if (!int.TryParse(v[0].Substring(1), out e.uId)
                        || !int.TryParse(v[1], out uRId) || !vRoom.ContainsKey(uRId))
                        continue;
                    e.uSlId = uId;
                    e.tName = v[2].Trim();
                    e.tBirdate = v[3];
                    e.tBirthplace = v[4].Trim();
                    vRoom[uRId].vExaminee.Add(e.mLv + e.uId, e);
                }
            }
        }

        public void DBInsertNee()
        {
            foreach (ExamRoom r in vRoom.Values)
                r.DBInsert();
        }

        public void DBSelectNee()
        {
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return;
            foreach (ExamRoom r in vRoom.Values)
            {
                r.vExaminee.Clear();
                string qry = DBConnect.mkQrySelect(ExamineeS0.tDBtbl + r.uId,
                    "lv,id,name,birdate,birthplace,t1,t2,grd,comp,qId,anssh",
                    "slId=" + uId, null);
                MySqlDataReader reader = DBConnect.exeQrySelect(conn, qry);
                if (reader != null)
                {
                    while (reader.Read())
                    {
                        ExamineeS0 e = new ExamineeS0();
                        e.uSlId = uId;
                        int lv;
                        if (Enum.IsDefined(typeof(ExamLv), lv = reader.GetInt16(0)))
                            e.eLv = (ExamLv)lv;
                        e.uId = reader.GetInt32(1);
                        e.tName = reader.GetString(2);
                        e.tBirdate = reader.GetDateTime(3).ToString(FORM_R);
                        e.tBirthplace = reader.GetString(4);
                        e.dtTim1 = (reader.IsDBNull(5)) ? INVALID_DT :
                            DateTime.Parse(reader.GetString(5));
                        e.dtTim2 = (reader.IsDBNull(6)) ? INVALID_DT :
                            DateTime.Parse(reader.GetString(6));
                        if (!reader.IsDBNull(7))
                        {
                            e.eStt = ExamStt.Finished;
                            e.uGrade = reader.GetInt32(7);
                        }
                        else
                            e.eStt = ExamStt.Info;
                        if (!reader.IsDBNull(8))
                            e.tComp = reader.GetString(8);
                        else
                            e.tComp = "unknown";//todo
                        r.vExaminee.Add(e.mLv + e.uId, e);
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
                        if (i.uId != rId && i.vExaminee.TryGetValue(e.mLv + e.uId, out o))
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

        public void ReadByteR1(byte[] buf, ref int offs)
        {
            while (3 < buf.Length - offs)
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

        public void LoadExaminees(Grid grdNee) //same as Prep0.xaml
        {
            Color c = new Color();
            c.A = 0xff;
            c.B = c.G = c.R = 0xf0;
            bool dark = false;
            int rid = -1;
            vGrade.Clear();
            vDt1.Clear();
            vDt2.Clear();
            vComp.Clear();
            grdNee.Children.Clear();
            foreach (ExamRoom r in vRoom.Values)
                foreach (ExamineeA e in r.vExaminee.Values)
                {
                    rid++;
                    RowDefinition rd = new RowDefinition();
                    rd.Height = new GridLength(20);
                    grdNee.RowDefinitions.Add(rd);
                    TextBlock t = new TextBlock();
                    t.Text = e.tId;
                    if (dark)
                        t.Background = new SolidColorBrush(c);
                    Grid.SetRow(t, rid);
                    grdNee.Children.Add(t);
                    t = new TextBlock();
                    t.Text = e.tName;
                    if (dark)
                        t.Background = new SolidColorBrush(c);
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 1);
                    grdNee.Children.Add(t);
                    t = new TextBlock();
                    t.Text = e.tBirdate;
                    if (dark)
                        t.Background = new SolidColorBrush(c);
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 2);
                    grdNee.Children.Add(t);
                    t = new TextBlock();
                    t.Text = e.tBirthplace;
                    if (dark)
                        t.Background = new SolidColorBrush(c);
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 3);
                    grdNee.Children.Add(t);
                    t = new TextBlock();
                    if (dark)
                        t.Background = new SolidColorBrush(c);
                    vGrade.Add(e.mLv + e.uId, t);
                    if (e.uGrade != ushort.MaxValue)
                        t.Text = e.uGrade.ToString();
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 4);
                    grdNee.Children.Add(t);
                    t = new TextBlock();
                    if (dark)
                        t.Background = new SolidColorBrush(c);
                    vDt1.Add(e.mLv + e.uId, t);
                    if (e.dtTim1.Year != ExamSlot.INVALID)
                        t.Text = e.dtTim1.ToString("HH:mm");
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 5);
                    grdNee.Children.Add(t);
                    t = new TextBlock();
                    if (dark)
                        t.Background = new SolidColorBrush(c);
                    vDt2.Add(e.mLv + e.uId, t);
                    if (e.dtTim2.Year != ExamSlot.INVALID)
                        t.Text = e.dtTim2.ToString("HH:mm");
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 6);
                    grdNee.Children.Add(t);
                    t = new TextBlock();
                    if (dark)
                        t.Background = new SolidColorBrush(c);
                    vComp.Add(e.mLv + e.uId, t);
                    if (e.tComp != null)
                        t.Text = e.tComp;
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 7);
                    grdNee.Children.Add(t);
                    dark = !dark;
                }
        }

        public void UpdateRsView()
        {
            TextBlock t;
            foreach (ExamRoom r in vRoom.Values)
                foreach (ExamineeA e in r.vExaminee.Values)
                {
                    if (e.uGrade != ushort.MaxValue && vGrade.TryGetValue(e.mLv + e.uId, out t))
                        t.Text = e.uGrade.ToString();
                    if (e.dtTim1.Hour != INVALID && vDt1.TryGetValue(e.mLv + e.uId, out t))
                        t.Text = e.dtTim1.ToString("HH:mm");
                    if (e.dtTim2.Hour != INVALID && vDt2.TryGetValue(e.mLv + e.uId, out t))
                        t.Text = e.dtTim2.ToString("HH:mm");
                    if (e.tComp != null && vComp.TryGetValue(e.mLv + e.uId, out t))
                        t.Text = e.tComp;
                }
        }

        public bool GenQPack(int n, ExamLv lv, int[] vn)
        {
            vQPack[lv].uSlId = uId;
            List<QuestSheet> l = vQPack[lv].GenQPack(n, vn);
            mKeyPack.ExtractKey(l);
            return false;
        }

        public byte[] ToByteQPack()
        {
            List<byte[]> l = new List<byte[]>();
            foreach (QuestPack p in vQPack.Values)
            {
                l.Add(BitConverter.GetBytes((int)p.eLv));
                l.Add(BitConverter.GetBytes(p.vSheet.Count));
                l.Add(p.ToByte());
            }
            int sz = 0;
            foreach (byte[] x in l)
                sz += x.Length;
            byte[] rval = new byte[sz];
            int offs = 0;
            foreach(byte[] x in l)
            {
                Array.Copy(x, 0, rval, offs, x.Length);
                offs += x.Length;
            }
            return rval;
        }

        public bool ReadByteQPack(byte[] buf, ref int offs)
        {
            int l = buf.Length - offs;

            if (l < 4)
                return true;

            while(0 < l)
            {
                int x;
                ExamLv lv;
                if (Enum.IsDefined(typeof(ExamLv), x = BitConverter.ToInt32(buf, offs)))
                    lv = (ExamLv)x;
                else
                    return true;
                l -= 4;
                offs += 4;
                if (l < 4)
                    return true;
                x = BitConverter.ToInt32(buf, offs);
                l -= 4;
                offs += 4;
                while (0 < x)
                {
                    --x;
                    QuestSheet qs = new QuestSheet();
                    if (qs.ReadByte(buf, ref offs))
                        return true;
                    vQPack[lv].vSheet.Add(qs.uId, qs);
                }
            }
            return false;
        }

        public byte[] ToByteKey()
        {
            return mKeyPack.ToByte();
        }
    }
}
