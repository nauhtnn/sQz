using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sQzLib
{
    interface QuestParser<T>
    {
        Queue<Question> ParseLines(Queue<T> lines);
    }
}
