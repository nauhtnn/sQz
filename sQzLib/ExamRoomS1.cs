using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sQzLib
{
    class ExamRoomS1: ExamRoomA
    {
        public ExamineeA Signin(ExamineeA e)
        {
            ExamineeA o;
            if (Examinees.TryGetValue(e.ID, out o) && o.Birthdate == e.Birthdate)
            {
                o.bFromC = true;
                o.Merge(e);
                return o;
            }
            return null;
        }

        public bool ReadBytes(byte[] buf, ref int offs)
        {
            return ReadBytes(buf, ref offs, new ExamineeS1(), true);
        }
    }
}
