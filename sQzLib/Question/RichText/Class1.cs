using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace sQzLib
{
    public class PassageContainingBlanks: PassageWithQuestions
    {
        
        string Replaced_StartQuestionID;//abstract
        string Replaced_EndQuestionID;//abstract
        List<string> Replaced_QuestionIDs_Ordered;

        public bool AutoDetectReplacedText()//abstract
        {
            Regex.m
            return true;
        }

        public bool LoadManualReplacedText()
        {
            return true;
        }
    }
}
