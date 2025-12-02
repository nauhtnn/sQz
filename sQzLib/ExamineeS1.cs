using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sQzLib
{
    public sealed class ExamineeS1: ExamineeA
    {
        public bool bLog;

        bool bNRecd;
        public bool NRecd { get { return bNRecd; } }
        public ExamineeS1() {
            Reset();
        }

        public override void Reset()
        {
            _Reset();
            bNRecd = true;
            bLog = false;
        }

        public byte[] GetBytes_SendingToClient()
        {
            List<byte[]> l = new List<byte[]>();
            l.Add(BitConverter.GetBytes(TestType));
            l.Add(BitConverter.GetBytes((int)eStt));
            if (eStt == NeeStt.Finished)
                l.Add(BitConverter.GetBytes(CorrectCount));

            if (eStt < NeeStt.Finished || bLog)
            {
                Utils.AppendBytesOfString(Birthdate, l);
                Utils.AppendBytesOfString(Name, l);
            }

            bLog = false;

            return Utils.ToArray_FromListOfBytes(l);
        }

        public bool ReadBytes_FromClient(byte[] buf, ref int offs)
        {
            int l = buf.Length - offs;

            ID = Utils.ReadBytesOfString(buf, ref offs, ref l);
            if (ID == null)
                return true;

            if (l < 4)
                return true;
            TestType = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;

            if (l < 4)
                return true;
            int stt;
            if (Enum.IsDefined(typeof(NeeStt), stt = BitConverter.ToInt32(buf, offs)))
                eStt = (NeeStt)stt;
            l -= 4;
            offs += 4;

            if (l < 4)
                return true;
            bLog = BitConverter.ToBoolean(buf, offs);
            l -= 1;
            offs += 1;
            //
            if (eStt < NeeStt.Examing || bLog)
            {
                Birthdate = Utils.ReadBytesOfString(buf, ref offs, ref l);
                if (Birthdate == null)
                    return true;
                ComputerName = Utils.ReadBytesOfString(buf, ref offs, ref l);
                if (ComputerName == null)
                    return true;
            }

            if (eStt < NeeStt.Examing)
                return false;

            if (l < 4)
                return true;
            AnswerSheet.QuestSheetID = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;

            if (eStt < NeeStt.Submitting)
                return false;

            if (l < 4)
                return true;
            AnswerSheet.BytesOfAnswer_Length = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;

            if (l < AnswerSheet.BytesOfAnswer_Length)
                return true;
            AnswerSheet.BytesOfAnswer = new byte[AnswerSheet.BytesOfAnswer_Length];
            Buffer.BlockCopy(buf, offs, AnswerSheet.BytesOfAnswer, 0, AnswerSheet.BytesOfAnswer_Length);
            l -= AnswerSheet.BytesOfAnswer_Length;
            offs += AnswerSheet.BytesOfAnswer_Length;

            return false;
        }

        public bool ReadBytes_FromS0(byte[] buf, ref int offs)
        {
            int l = buf.Length - offs;
            //
            ID = Utils.ReadBytesOfString(buf, ref offs, ref l);

            if (l < 4)
                return true;
            TestType = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;

            if (l < 4)
                return true;
            int stt;
            if (Enum.IsDefined(typeof(NeeStt), stt = BitConverter.ToInt32(buf, offs)))
                eStt = (NeeStt)stt;
            l -= 4;
            offs += 4;

            Birthdate = Utils.ReadBytesOfString(buf, ref offs, ref l);
            if (Birthdate == null)
                return true;
            
            Name = Utils.ReadBytesOfString(buf, ref offs, ref l);
            if (Name == null)
                return true;
            //
            if (eStt < NeeStt.Finished)
                return false;
            bNRecd = false;
            //
            if (l < sizeof(long))
                return true;
            if ((dtTim1 = DT.ReadByte(buf, ref offs)) == DT.INVALID)
                return true;
            l -= sizeof(long);

            if (l < sizeof(long))
                return true;
            if ((dtTim2 = DT.ReadByte(buf, ref offs)) == DT.INVALID)
                return true;
            l -= sizeof(long);

            if (l < 4)
                return true;
            CorrectCount = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;

            ComputerName = Utils.ReadBytesOfString(buf, ref offs, ref l);
            //
            return false;
        }

        public List<byte[]> GetBytes_SendingToS0()
        {
            //suppose eStt == NeeStt.Finished
            List<byte[]> l = new List<byte[]>();
            Utils.AppendBytesOfString(ID, l);
            if (0 < ComputerName.Length)
                Utils.AppendBytesOfString(ComputerName, l);
            else
                l.Add(BitConverter.GetBytes(0));
            l.Add(BitConverter.GetBytes(dtTim1.ToBinary()));
            l.Add(BitConverter.GetBytes(AnswerSheet.QuestSheetID));
            l.Add(BitConverter.GetBytes(AnswerSheet.BytesOfAnswer.Length));
            l.Add(AnswerSheet.BytesOfAnswer);
            l.Add(BitConverter.GetBytes(dtTim2.ToBinary()));
            l.Add(BitConverter.GetBytes(CorrectCount));
            return l;
        }

        public void MergeWithClient(ExamineeS1 e)
        {
            if (eStt == NeeStt.Finished)
                return;
            bLog = e.bLog;
            if (eStt < NeeStt.Examing || bLog)
                ComputerName = e.ComputerName;
            if (e.eStt < NeeStt.Examing)
                eStt = NeeStt.Examing;
            else
                eStt = e.eStt;
            if (eStt < NeeStt.Examing)
                return;
            AnswerSheet = new AnswerSheet();
            AnswerSheet.QuestSheetID = e.AnswerSheet.QuestSheetID;

            if (eStt < NeeStt.Submitting)
                return;
            AnswerSheet.BytesOfAnswer = e.AnswerSheet.BytesOfAnswer;
        }

        public void MergeWithS0(ExamineeA e)
        {
            //suppose e.eStt = NeeStt.Finished
            eStt = e.eStt;
            TestType = e.TestType;
            dtTim1 = e.dtTim1;
            dtTim2 = e.dtTim2;
            CorrectCount = e.CorrectCount;
        }
    }
}
