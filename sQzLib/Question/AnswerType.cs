using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sQzLib
{
    public enum AnswerType
    {
        Undefined = 0,
        SingleAnswer,
        MultipleTrueFalse,
        MultipleResponse
    }

    public enum ContentType
    {
        Raw = 1,
        Image = 2,
        Audio = 4,
        Video = 8
    }

    public enum TokenType
    {
        Requirement = 0,
        Stem = 1,
        Ans = 2,
        Both = 3
    }
}
