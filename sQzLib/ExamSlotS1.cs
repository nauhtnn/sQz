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
    }
}
