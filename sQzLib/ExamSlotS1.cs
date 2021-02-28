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

        public List<byte[]> GetBytesRoom_SendingToS0()
        {
            List<byte[]> l = new List<byte[]>();
            l.Add(DT.GetBytes(mDt));
            if (Rooms.Values.Count == 1)//either 0 or 1
            {
                foreach (ExamRoomS1 r in Rooms.Values)
                    l.InsertRange(l.Count, r.GetBytes_SendingToS0());
            }
            else
                l.Add(BitConverter.GetBytes((int)0));
            return l;
        }
    }
}
