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

        public List<byte[]> GetBytes_SendingToS1()
        {
            List<byte[]> l = new List<byte[]>();
            Utils.AppendBytesOfString(ID, l);
            l.Add(BitConverter.GetBytes(TestType));
            l.Add(BitConverter.GetBytes((int)eStt));

            Utils.AppendBytesOfString(Birthdate, l);
            Utils.AppendBytesOfString(Name, l);

            if (eStt < NeeStt.Finished)
                return l;

            l.Add(BitConverter.GetBytes(dtTim1.ToBinary()));

            l.Add(BitConverter.GetBytes(dtTim2.ToBinary()));
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
            ID = Utils.ReadBytesOfString(buf, ref offs, ref l);
            if (ID.Length == 0)
                return true;

            ComputerName = Utils.ReadBytesOfString(buf, ref offs, ref l);
            //
            if (l < sizeof(long))
                return true;
            dtTim1 = DateTime.FromBinary(BitConverter.ToInt64(buf, offs));
            l -= sizeof(long);
            offs += sizeof(long);

            if (l < 4)
                return true;
            AnswerSheet.QuestSheetID = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;

            if (l < 4)
                return true;
            AnswerSheet.BytesOfAnswer_Length = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;

            if (l < AnswerSheet.BytesOfAnswer_Length)
                return true;
            AnswerSheet.BytesOfAnswer = new byte[AnswerSheet.BytesOfAnswer_Length];
            Array.Copy(buf, offs, AnswerSheet.BytesOfAnswer, 0, AnswerSheet.BytesOfAnswer.Length);
            l -= AnswerSheet.BytesOfAnswer.Length;
            offs += AnswerSheet.BytesOfAnswer.Length;

            if (l < sizeof(long))
                return true;
            dtTim2 = DateTime.FromBinary(BitConverter.ToInt64(buf, offs));
            l -= sizeof(long);
            offs += sizeof(long);

            if (l < 4)
                return true;
            CorrectCount = BitConverter.ToInt32(buf, offs);
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

        public void DBGetQSId()
        {
            MySqlConnection conn = DBConnect.OpenNewConnection();
            if (conn == null)
            {
                TestType = -1;
                AnswerSheet.QuestSheetID = -1;
                return;
            }
            string qry = DBConnect.mkQrySelect("sqz_examinee AS a, sqz_nee_qsheet AS b", "t_type, qsid",
                "a.dt='" + mDt.ToString(DT._) + "' AND a.id='" + ID + "' AND a.dt=b.dt AND a.id=b.neeid");
            string eMsg;
            MySqlDataReader reader = DBConnect.exeQrySelect(conn, qry, out eMsg);
            if (reader == null)
            {
                DBConnect.Close(ref conn);
                TestType = -1;
                AnswerSheet.QuestSheetID = -1;
                return;
            }
            if (reader.Read())
            {
                TestType = reader.GetInt32(0);
                AnswerSheet.QuestSheetID = reader.GetInt32(1);
            }
            else
            {
                TestType = -1;
                AnswerSheet.QuestSheetID = -1;
            }
            reader.Close();
            DBConnect.Close(ref conn);
        }

        public void DBGetAns()
        {
            AnswerSheet.BytesOfAnswer = null;
            AnswerSheet.BytesOfAnswer_Length = 0;
            MySqlConnection conn = DBConnect.OpenNewConnection();
            if (conn == null)
                return;
            string qry = DBConnect.mkQrySelect("sqz_nee_qsheet", "ans",
                "dt='" + mDt.ToString(DT._) + "' AND neeid='" + ID + "'");
            string eMsg;
            MySqlDataReader reader = DBConnect.exeQrySelect(conn, qry, out eMsg);
            if (reader == null)
            {
                DBConnect.Close(ref conn);
                return;
            }
            char[] ans = null;
            if (reader.Read())
                ans = reader.GetString(0).ToCharArray();
            reader.Close();
            DBConnect.Close(ref conn);
            if(ans != null && ans.Length > 0)
            {
                AnswerSheet.BytesOfAnswer = new byte[ans.Length];
                AnswerSheet.BytesOfAnswer_Length = AnswerSheet.BytesOfAnswer.Length;
                for (int i = 0; i < ans.Length; ++i)
                    if (ans[i] == Question.C1)
                        AnswerSheet.BytesOfAnswer[i] = 1;
            }
        }

        public bool DBSelGrade()
        {
            MySqlConnection conn = DBConnect.OpenNewConnection();
            if (conn == null)
                return true;
            string qry = DBConnect.mkQrySelect("sqz_nee_qsheet", "grade",
                "dt='" + mDt.ToString(DT._) + "' AND neeid='" + ID + "'");
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
