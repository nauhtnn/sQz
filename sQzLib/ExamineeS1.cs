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
<<<<<<< HEAD
            l.Add(BitConverter.GetBytes(TestType));
            l.Add(BitConverter.GetBytes((int)eStt));
            if (eStt == NeeStt.Finished)
                l.Add(BitConverter.GetBytes(CorrectCount));
=======
            l.Add(BitConverter.GetBytes((int)mPhase));
            if (mPhase == ExamineePhase.Finished)
                l.Add(BitConverter.GetBytes(uGrade));
>>>>>>> master

            if (mPhase < ExamineePhase.Finished || bLog)
            {
                Utils.AppendBytesOfString(Birthdate, l);
                Utils.AppendBytesOfString(Name, l);
            }

            bLog = false;

            return Utils.ToArray_FromListOfBytes(l);
        }

        public bool ReadBytes_FromClient(byte[] buf, ref int offs)
        {
<<<<<<< HEAD
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
=======
            return false;
            //int l = buf.Length - offs;
            ////
            //if (l < 12)
            //    return true;
            //int x = BitConverter.ToInt32(buf, offs);
            //l -= 4;
            //offs += 4;
            //if (ParseLvId(x))
            //    return true;
            //if (Enum.IsDefined(typeof(NeeStt), x = BitConverter.ToInt32(buf, offs)))
            //    eStt = (NeeStt)x;
            //l -= 4;
            //offs += 4;
            //bLog = BitConverter.ToBoolean(buf, offs);
            //l -= 1;
            //offs += 1;
            ////
            //if (eStt < NeeStt.Examing || bLog)
            //{
            //    if (l < 4)
            //        return true;
            //    int sz = BitConverter.ToInt32(buf, offs);
            //    l -= 4;
            //    offs += 4;
            //    if (l < sz + 4)
            //        return true;
            //    tBirdate = Encoding.UTF8.GetString(buf, offs, sz);
            //    l -= sz;
            //    offs += sz;
            //    sz = BitConverter.ToInt32(buf, offs);
            //    l -= 4;
            //    offs += 4;
            //    if (l < sz)
            //        return true;
            //    tComp = Encoding.UTF8.GetString(buf, offs, sz);
            //    l -= sz;
            //    offs += sz;
            //}
>>>>>>> master

            //if (eStt < NeeStt.Examing)
            //    return false;

<<<<<<< HEAD
            if (l < 4)
                return true;
            AnswerSheet.QuestSheetID = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;
=======
            //if (l < 4)
            //    return true;
            //mAnsSh.uQSLvId = BitConverter.ToInt32(buf, offs);
            //l -= 4;
            //offs += 4;
>>>>>>> master

            //if (eStt < NeeStt.Submitting)
            //    return false;

<<<<<<< HEAD
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
=======
            //if (l < AnsSheet.LEN)
            //    return true;
            //mAnsSh.aAns = new byte[AnsSheet.LEN];
            //Buffer.BlockCopy(buf, offs, mAnsSh.aAns, 0, AnsSheet.LEN);
            //l -= AnsSheet.LEN;
            //offs += AnsSheet.LEN;
>>>>>>> master

            //return false;
        }

        public bool ReadBytes_FromS0(byte[] buf, ref int offs)
        {
            int l = buf.Length - offs;
            //
            ID = Utils.ReadBytesOfString(buf, ref offs, ref l);

<<<<<<< HEAD
            if (l < 4)
=======
            int x;
            if (Enum.IsDefined(typeof(Level), x = BitConverter.ToInt32(buf, offs)))
                Lv = (Level)x;
            else
>>>>>>> master
                return true;
            TestType = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;

<<<<<<< HEAD
            if (l < 4)
                return true;
            int stt;
            if (Enum.IsDefined(typeof(NeeStt), stt = BitConverter.ToInt32(buf, offs)))
                eStt = (NeeStt)stt;
=======
            uId = BitConverter.ToInt32(buf, offs);
            l -= 4;
            offs += 4;

            if (Enum.IsDefined(typeof(ExamineePhase), x = BitConverter.ToInt32(buf, offs)))
                mPhase = (ExamineePhase)x;
>>>>>>> master
            l -= 4;
            offs += 4;

            Birthdate = Utils.ReadBytesOfString(buf, ref offs, ref l);
            if (Birthdate == null)
                return true;
            
            Name = Utils.ReadBytesOfString(buf, ref offs, ref l);
            if (Name == null)
                return true;
            //
<<<<<<< HEAD
            if (eStt < NeeStt.Finished)
=======
            if (l < sz)
                return true;
            if (0 < sz)
            {
                tBirthplace = Encoding.UTF8.GetString(buf, offs, sz);
                l -= sz;
                offs += sz;
            }
            if (mPhase < ExamineePhase.Finished)
>>>>>>> master
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
<<<<<<< HEAD
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
=======
            l.Add(BitConverter.GetBytes((int)Lv));
            l.Add(BitConverter.GetBytes(uId));
            if (0 < tComp.Length)
            {
                byte[] x = Encoding.UTF8.GetBytes(tComp);
                l.Add(BitConverter.GetBytes(x.Length));
                l.Add(x);
            }
            else
                l.Add(BitConverter.GetBytes(0));
            l.Add(BitConverter.GetBytes(dtTim1.Hour));
            l.Add(BitConverter.GetBytes(dtTim1.Minute));
            l.Add(BitConverter.GetBytes(mAnsSheet.uQSId));
            l.Add(mAnsSheet.aAns);
            l.Add(BitConverter.GetBytes(dtTim2.Hour));
            l.Add(BitConverter.GetBytes(dtTim2.Minute));
            l.Add(BitConverter.GetBytes(uGrade));
>>>>>>> master
            return l;
        }

        public void MergeWithClient(ExamineeS1 e)
        {
            if (mPhase == ExamineePhase.Finished)
                return;
            bLog = e.bLog;
<<<<<<< HEAD
            if (eStt < NeeStt.Examing || bLog)
                ComputerName = e.ComputerName;
            if (e.eStt < NeeStt.Examing)
                eStt = NeeStt.Examing;
=======
            if (mPhase < ExamineePhase.Examing || bLog)
                tComp = e.tComp;
            if (e.mPhase < ExamineePhase.Examing)
                mPhase = ExamineePhase.Examing;
>>>>>>> master
            else
                mPhase = e.mPhase;
            if (mPhase < ExamineePhase.Examing)
                return;
<<<<<<< HEAD
            AnswerSheet = new AnswerSheet();
            AnswerSheet.QuestSheetID = e.AnswerSheet.QuestSheetID;
=======
            mAnsSheet = new AnsSheet();
            mAnsSheet.uQSLvId = e.mAnsSheet.uQSLvId;
>>>>>>> master

            if (mPhase < ExamineePhase.Submitting)
                return;
<<<<<<< HEAD
            AnswerSheet.BytesOfAnswer = e.AnswerSheet.BytesOfAnswer;
=======
            mAnsSheet.aAns = e.mAnsSheet.aAns;
>>>>>>> master
        }

        public void MergeWithS0(ExamineeA e)
        {
            //suppose e.eStt = NeeStt.Finished
<<<<<<< HEAD
            eStt = e.eStt;
            TestType = e.TestType;
=======
            mPhase = e.mPhase;
>>>>>>> master
            dtTim1 = e.dtTim1;
            dtTim2 = e.dtTim2;
            CorrectCount = e.CorrectCount;
        }
    }
}
