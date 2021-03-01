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
            l.Add(BitConverter.GetBytes(CorrectCount));
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
            if (l < AnswerSheet.BytesOfAnswer_Length + 24)
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
            AnswerSheet.QuestSheetID = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
            AnswerSheet.BytesOfAnswer = new byte[AnswerSheet.BytesOfAnswer_Length];
            Array.Copy(buf, offs, AnswerSheet.BytesOfAnswer, 0, AnswerSheet.BytesOfAnswer_Length);
            l -= AnswerSheet.BytesOfAnswer_Length;
            offs += AnswerSheet.BytesOfAnswer_Length;

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
            CorrectCount = BitConverter.ToInt32(buf, offs);
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
            AnswerSheet = e.AnswerSheet;
            dtTim1 = e.dtTim1;
            CorrectCount = e.CorrectCount;
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
            char[] noans = new char[AnswerSheet.BytesOfAnswer_Length];
            for (int i = 0; i < AnswerSheet.BytesOfAnswer_Length; ++i)
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
                CorrectCount = reader.GetInt16(0);
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
