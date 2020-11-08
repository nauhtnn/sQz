using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sQzLib
{
    public abstract class QuestParser<T>
    {
        public QSheetSections SheetSections = new QSheetSections();
        public abstract Queue<Question> ParseLines(Queue<T> lines);
    }
}
