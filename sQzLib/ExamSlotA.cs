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

        public bool ReadBytes_S1RecevingFromS0(byte[] buf, ref int offs)
        {
            if ((Dt = DT.ReadByte(buf, ref offs)) == DT.INVALID)
                return true;
            if (buf.Length - offs < 4)
                return true;
            int rId;
            if ((rId = BitConverter.ToInt32(buf, offs)) < 0)
                return true;
            offs += 4;
            ExamRoomA r;
            if (Rooms.TryGetValue(rId, out r))
            {
                if (r.ReadBytes(buf, ref offs, new ExamineeS1(), true))
                    return true;
            }
            else
            {
                r = new ExamRoomA();
                r.uId = rId;
                if (r.ReadBytes(buf, ref offs, new ExamineeS1(), true))
                    return true;
                Rooms.Add(rId, r);
            }
            return false;
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

        public List<byte[]> GetBytesRoom_S1SendingToS0()
        {
            List<byte[]> l = new List<byte[]>();
            l.Add(DT.GetBytes(mDt));
            if (Rooms.Values.Count == 1)//either 0 or 1
            {
                foreach (ExamRoomA r in Rooms.Values)
                    l.InsertRange(l.Count, r.GetBytes_S1SendingToS0());
            }
            else
                l.Add(BitConverter.GetBytes((int)0));
            return l;
        }
    }
}
