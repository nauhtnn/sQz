using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sQzLib
{
    public class MTFIndependentQSection: IndependentQSection
    {
        public MTFIndependentQSection()
        {
            Init();
        }

        public MTFIndependentQSection(int id)
        {
            Init(id);
        }
        public override object Clone()
        {
            MTFIndependentQSection newSection = new MTFIndependentQSection(ID);
            newSection.Requirements = Requirements;
            foreach (Question q in Questions)
                newSection.Questions.Add(q.DeepCopy());
            return newSection;
        }
    }
}
