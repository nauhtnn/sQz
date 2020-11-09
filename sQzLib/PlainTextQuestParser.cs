using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sQzLib
{
    class PlainTextQuestParser
    {
        public Tuple<List<Question>, List<PassageQuestion>> ParseLines(Queue<string> lines)
        {
            string[] plainTexts = lines.Cast<string>().ToArray();
            List<Question> singleQuestions = new List<Question>();
            List<PassageQuestion> passageQuestions = new List<PassageQuestion>();
            for(int index = 0; index < plainTexts.Length;)
            {
                string sectionHeader = ParsePassage(plainTexts[index]);
                if(sectionHeader != null)
                {
                    passageQuestions = ParsePassageQuestion(plainTexts, ref index);
                    break;
                }
                Question question = Parse1Question(plainTexts, ref index);
                if(question == null)
                {
                    System.Windows.MessageBox.Show("Only " + singleQuestions.Count + " are read.");
                    break;
                }
                singleQuestions.Add(question);
            }
            return new Tuple<List<Question>, List<PassageQuestion>>(singleQuestions, passageQuestions);
        }

        List<PassageQuestion> ParsePassageQuestion(string[] plainTexts, ref int index)
        {
            List<PassageQuestion> passageQuestions = new List<PassageQuestion>();
            PassageQuestion passageQuestion = null;
            while(index < plainTexts.Length)
            {
                string passage = ParsePassage(plainTexts[index]);
                if (passage != null)
                {
                    if (passageQuestion != null)
                        passageQuestions.Add(passageQuestion);
                    passageQuestion = new PassageQuestion();
                    passageQuestion.Passage = passage;
                }
                Question question = Parse1Question(plainTexts, ref index);
                if (question == null)
                {
                    System.Windows.MessageBox.Show("Only " + passageQuestions.Count + " are read.");
                    break;
                }
                passageQuestion.Questions.Add(question);
            }
            return passageQuestions;
        }

        Question Parse1Question(string[] plainTexts, ref int index)
        {
            if (index + Question.N_ANS > plainTexts.Length)
            {
                System.Windows.MessageBox.Show("Line " + index + " doesn't have 1 stem 4 options!");
                return null;
            }

            Question question = new Question();
            question.Stmt = plainTexts[index++];
            question.vAns = new string[Question.N_ANS];
            for (int j = 0; j < Question.N_ANS;)
                question.vAns[j++] = plainTexts[index++];
            question.vKeys = new bool[Question.N_ANS];
            for (int j = 0; j < Question.N_ANS; ++j)
                question.vKeys[index] = false;
            if (question.tStmt[0] == '*' && 1 < question.tStmt.Length)
            {
                question.bDiff = true;
                question.tStmt = question.tStmt.Substring(1);
            }
            else if (question.tStmt[0] == '\\' && 1 < question.tStmt.Length
                && (question.tStmt[1] == '*' || question.tStmt[1] == '\\'))
                question.tStmt = question.tStmt.Substring(1);
            int nKey = 0;
            for (int j = 0; j < Question.N_ANS; ++j)
            {
                if (question.vAns[j][0] == '\\' && 1 < question.vAns[j].Length)
                {
                    if (question.vAns[j][1] != '\\')
                    {
                        question.vKeys[j] = true;
                        question.vAns[j] = Utils.CleanFront(question.vAns[j].Substring(1));
                        ++nKey;
                    }
                    else
                        question.vAns[j] = question.vAns[j].Substring(1);
                }
            }
            if (nKey != 1)
            {
                System.Windows.MessageBox.Show("Line " + index + " has " + nKey + " key!");
                return null;
            }
            return question;
        }

        string ParsePassage(string line)
        {
            if(line.IndexOf(PassageQuestion.MAGIC_WORD) == 0)
            {
                string s = string.Empty;
                if (line.Length > PassageQuestion.MAGIC_WORD.Length)
                    s = Utils.CleanFront(line.Substring(PassageQuestion.MAGIC_WORD.Length - 1));
                if(s.Length == 0)
                {
                    System.Windows.MessageBox.Show("Passage is empty!");
                    return string.Empty;
                }
                return s;
            }
            return null;
        }
    }
}
