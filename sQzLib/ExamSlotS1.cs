using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sQzLib
{
    public class ExamSlotS1: ExamSlotA
    {
        public Dictionary<int, ExamRoomS1> Rooms;

        public ExamSlotS1()
        {
            Rooms = new Dictionary<int, ExamRoomS1>();
        }

        public ExamineeS1 Signin(ExamineeS1 e)
        {
            ExamineeS1 o;
            foreach (ExamRoomS1 r in Rooms.Values)
                if ((o = r.Signin(e)) != null)
                    return o;
            return null;
        }

        public ExamineeS1 Find(string neeID)
        {
            ExamineeS1 o;
            foreach (ExamRoomS1 r in Rooms.Values)
                if (r.Examinees.TryGetValue(neeID, out o))
                    return o;
            return null;
        }

        public bool ReadBytes_FromS0(byte[] buf, ref int offs)
        {
            if ((Dt = DT.ReadByte(buf, ref offs)) == DT.INVALID)
                return true;
            if (buf.Length - offs < 4)
                return true;
            int rId;
            if ((rId = BitConverter.ToInt32(buf, offs)) < 0)
                return true;
            offs += 4;
            ExamRoomS1 r;
            if (Rooms.TryGetValue(rId, out r))
            {
                if (r.ReadBytes_FromS0(buf, ref offs))
                    return true;
            }
            else
            {
                r = new ExamRoomS1();
                r.uId = rId;
                if (r.ReadBytes_FromS0(buf, ref offs))
                    return true;
                Rooms.Add(rId, r);
            }
            return false;
        }

        public List<byte[]> GetBytes_RoomSendingToS0()
        {
            List<byte[]> l = new List<byte[]>();
            l.Add(BitConverter.GetBytes(mDt.ToBinary()));
            if (Rooms.Values.Count == 1)//either 0 or 1
            {
                foreach (ExamRoomS1 r in Rooms.Values)
                    l.InsertRange(l.Count, r.GetBytes_SendingToS0());
            }
            else
                l.Add(BitConverter.GetBytes((int)0));
            return l;
        }

        public bool ReadBytes_QPacksNoDateTime(byte[] buf, ref int offs)
        {
            if (buf == null)
                return false;
            if (buf.Length - offs < 0)
                return false;
            int n = BitConverter.ToInt32(buf, offs);
            offs += 4;
            if (n < 0)
                return false;
            while (0 < n)
            {
                --n;
                QuestPack pack = new QuestPack(-1);
                if (!pack.ReadByte(buf, ref offs))
                    return false;
                Safe_AddToQuestionPacks(pack);
            }
            return true;
        }

        public byte[] GetBytes_NextQSheet(int testType)
        {
            if(QuestionPacks.ContainsKey(testType))
                return QuestionPacks[testType].GetBytes_NextQSheet();
            else
            {
                System.Windows.MessageBox.Show("GetBytes_NextQSheet: key not found: " + testType);
                return null;
            }
        }

        public bool ReadBytesKey_NoDateTime(byte[] buf, ref int offs)
        {
            AnswerKeyPacks.Clear();
            AnswerPack answerPack = new AnswerPack(-1);
            while (answerPack.ReadBytes_S1ReceivingFromS0(buf, ref offs))
            {
                Safe_AddToAnswerPacks(answerPack);
                answerPack = new AnswerPack(-1);
            }
            return AnswerKeyPacks.Count > 0;
        }

        public int GetTestTypeOfExaminee(string neeID)
        {
            foreach (ExamRoomS1 room in Rooms.Values)
            {
                if (room.Examinees.ContainsKey(neeID))
                    return room.Examinees[neeID].TestType;
            }
            return -1;
        }
    }
}
