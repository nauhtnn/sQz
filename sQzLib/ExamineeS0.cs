using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace sQzLib
{
    public sealed class ExamineeS0: ExamineeA
    {
        public bool bToDB;
        public bool bToVw;
        public ExamineeS0()
        {
            Reset();
        }

        public override void Reset()
        {
            _Reset();
            bToVw = bToDB = false;
        }

        public List<byte[]> GetBytes_ClientSendingToS1()
        {
            List<byte[]> l = new List<byte[]>();
            Utils.AppendBytesOfString(ID, l);
            l.Add(BitConverter.GetBytes((int)eStt));

            Utils.AppendBytesOfString(Birthdate, l);
            Utils.AppendBytesOfString(Name, l);
            Utils.AppendBytesOfString(Birthplace, l);

            if (eStt < NeeStt.Finished)
                return l;

            l.Add(DT.GetBytes(dtTim1));

            l.Add(DT.GetBytes(dtTim2));
            l.Add(BitConverter.GetBytes(Grade));
            if(0 < ComputerName.Length)
                Utils.AppendBytesOfString(ComputerName, l);
            else
                l.Add(BitConverter.GetBytes(0));

            return l;
        }

        public bool ReadByte_FromS1(byte[] buf, ref int offs)
        {
            //suppose eStt == NeeStt.Finished
            int l = buf.Length - offs;
            //
            if (l < 4)
                return true;
            int x = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            //
            if (l < x || x < 1)
                return true;
            ID = Encoding.UTF8.GetString(buf, offs, x);
            l -= x;
            offs += x;

            if (l < 4)
                return true;

            x = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            //
            if (l < x)
                return true;
            if (0 < x)
            {
                ComputerName = Encoding.UTF8.GetString(buf, offs, x);
                l -= x;
                offs += x;
            }
            //
            if (l < AnsSheet.LEN + 24)
                return true;
            int h = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            int m = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            if (!DateTime.TryParse(h.ToString() + ':' + m, out dtTim1))
            {
                dtTim1 = DT.INVALID;
                return true;
            }
            mAnsSh.questSheetID = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            mAnsSh.aAns = new byte[AnsSheet.LEN];
            Array.Copy(buf, offs, mAnsSh.aAns, 0, AnsSheet.LEN);
            l -= AnsSheet.LEN;
            offs += AnsSheet.LEN;

            h = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            m = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            if (!DateTime.TryParse(h.ToString() + ':' + m, out dtTim2))
            {
                dtTim2 = DT.INVALID;
                return true;
            }
            Grade = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            //
            return false;
        }

        public void MergeWithS1(ExamineeA e)
        {
            if (eStt == NeeStt.Finished)
                return;
            //suppose eStt = eINFO and e.eStt = NeeStt.Finished
            eStt = NeeStt.Finished;
            bToVw = bToDB = true;
            ComputerName = e.ComputerName;
            mAnsSh = e.mAnsSh;
            dtTim1 = e.dtTim1;
            Grade = e.Grade;
            dtTim2 = e.dtTim2;
        }

        public int DBGetQSId()
        {
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return -1;
            string qry = DBConnect.mkQrySelect("sqz_examinee", "qsid",
                "dt='" + mDt.ToString(DT._) + "' AND id=" + ID);
            string eMsg;
            MySqlDataReader reader = DBConnect.exeQrySelect(conn, qry, out eMsg);
            if (reader == null)
            {
                DBConnect.Close(ref conn);
                return -1;
            }
            int qsid = -1;
            if (reader.Read())
                qsid = reader.GetInt32(0);
            reader.Close();
            DBConnect.Close(ref conn);
            return qsid;
        }

        public char[] DBGetAns()
        {
            char[] noans = new char[AnsSheet.LEN];
            for (int i = 0; i < AnsSheet.LEN; ++i)
                noans[i] = Question.C0;
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return noans;
            string qry = DBConnect.mkQrySelect("sqz_examinee", "ans",
                "dt='" + mDt.ToString(DT._) + "' AND id=" + ID);
            string eMsg;
            MySqlDataReader reader = DBConnect.exeQrySelect(conn, qry, out eMsg);
            if (reader == null)
            {
                DBConnect.Close(ref conn);
                return noans;
            }
            string ans = noans.ToString();
            if (reader.Read())
                ans = reader.GetString(0);
            reader.Close();
            DBConnect.Close(ref conn);
            return ans.ToCharArray();
        }

        public bool DBSelGrade()
        {
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return true;
            string qry = DBConnect.mkQrySelect("sqz_examinee", "grade",
                "dt='" + mDt.ToString(DT._) + "' AND id=" + ID);
            string eMsg;
            MySqlDataReader reader = DBConnect.exeQrySelect(conn, qry, out eMsg);
            if (reader == null)
            {
                DBConnect.Close(ref conn);
                return true;
            }
            if (reader.Read())
                Grade = reader.GetInt16(0);
            reader.Close();
            DBConnect.Close(ref conn);
            return false;
        }

        public string DBGetT()
        {
            throw new NotImplementedException();
            //MySqlConnection conn = DBConnect.Init();
            //string t = DT.INV_H.ToString(DT.hh);
            //if (conn == null)
            //    return t;
            //string qry = DBConnect.mkQrySelect("sqz_examinee",
            //    "t", "dt='" + mDt.ToString(DT._) + "' AND lv='" + eLv.ToString() +
            //    "' AND id=" + uId);
            //string eMsg;
            //MySqlDataReader reader = DBConnect.exeQrySelect(conn, qry, out eMsg);
            //if (reader == null)
            //{
            //    DBConnect.Close(ref conn);
            //    return t;
            //}
            //if (reader.Read())
            //{
            //    t = reader.GetString(0);
            //}
            //reader.Close();
            //DBConnect.Close(ref conn);
            //return t;
        }
    }
}
