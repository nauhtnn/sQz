using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sQzLib
{
    class ExamSlotS1: ExamSlotA
    {
        public ExamSlotS1()
        {
            
        }

        public ExamineeA Signin(ExamineeA e)
        {
            ExamineeA o;
            foreach (ExamRoomA r in Rooms.Values)
                if ((o = (r as ExamRoomS1).Signin(e)) != null)
                    return o;
            return null;
        }

        public ExamineeA Find(string neeID)
        {
            ExamineeA o;
            foreach (ExamRoomA r in Rooms.Values)
                if (r.Examinees.TryGetValue(neeID, out o))
                    return o;
            return null;
        }
    }
}
