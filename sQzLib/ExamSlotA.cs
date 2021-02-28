using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using System.Text;

namespace sQzLib
{
    public enum ExamStt
    {
        Prep = 0,
        Oper,
        Arch
    }

    public abstract class ExamSlotA
    {
        public DateTime mDt;
        public QuestPack QuestionPack;

        public AnsPack mKeyPack;

        public Dictionary<int, DateTime> vT1;
        public Dictionary<int, DateTime> vT2;
        public ExamStt eStt;

        public ExamSlotA()
        {
            mDt = DT.INVALID;
            eStt = ExamStt.Prep;
            QuestionPack = new QuestPack();

            mKeyPack = new AnsPack();
        }

        public DateTime Dt {
            get { return mDt; }
            set {
                mDt = value;
                QuestionPack.mDt = value;
            }
        }

        public bool ReadBytesQPack_NoDateTime(byte[] buf, ref int offs)
        {
            if (QuestionPack.ReadByte(buf, ref offs))
                return true;
            return false;
        }

        public byte[] ToByteNextQS()
        {
            return QuestionPack.ToByteNextQS();
        }

        public byte[] GetBytesKey_WithDateTime()
        {
            List<byte[]> l = mKeyPack.ToByte();
            l.Insert(0, DT.GetBytes(mDt));
            return Utils.ListOfBytes_ToArray(l);
        }

        public bool ReadByteKey_NoDateTime(byte[] buf, ref int offs)
        {
            return mKeyPack.ReadByte(buf, ref offs);
        }
    }
}
