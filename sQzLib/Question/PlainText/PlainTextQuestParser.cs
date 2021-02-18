using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sQzLib
{
    class PlainTextQuestParser
    {
        public Tuple<List<Question>, List<PassageQuestion>> ParseLines(Queue<string> tokens)
        {
            List<Question> independentQuestions = new List<Question>();
            List<PassageQuestion> passageQuestions = new List<PassageQuestion>();
            while(tokens.Count > 0)
            {
                if (sQzLib.Utils.CleanFront(tokens.Peek()).IndexOf(PassageQuestion.MAGIC_WORD) == 0)
                {
                    tokens.Dequeue();
                    break;
                }
                Question question = Parse1Question(tokens);
                if(question == null)
                {
                    System.Windows.MessageBox.Show("Number of independent questions " + independentQuestions.Count + " are read.");
                    break;
                }
                independentQuestions.Add(question);
            }
            return new Tuple<List<Question>, List<PassageQuestion>>(independentQuestions, passageQuestions);
        }

        List<PassageQuestion> ParsePassageQuestion(Queue<string> lines)
        {
            return null;
            //List<PassageQuestion> passageQuestions = new List<PassageQuestion>();
            //PassageQuestion passageQuestion = null;
            //while(index < plainTexts.Length)
            //{
            //    string passage = ParsePassage(plainTexts[index]);
            //    if (passage != null)
            //    {
            //        if (passageQuestion != null)
            //            passageQuestions.Add(passageQuestion);
            //        passageQuestion = new PassageQuestion();
            //        passageQuestion.Passage = passage;
            //    }
            //    Question question = Parse1Question(lines);
            //    if (question == null)
            //    {
            //        System.Windows.MessageBox.Show("Only " + passageQuestions.Count + " are read.");
            //        break;
            //    }
            //    passageQuestion.Questions.Add(question);
            //}
            //return passageQuestions;
        }

        Question Parse1Question(Queue<string> tokens)
        {
            if (tokens.Count < Question.N_ANS + 1)
            {
                System.Windows.MessageBox.Show("From the end, line " + tokens.Count + " doesn't have 1 stem 4 options!");
                return null;
            }

            Question question = new Question();
            question.Stmt = tokens.Dequeue();
            question.vAns = new string[Question.N_ANS];
            for (int j = 0; j < Question.N_ANS;)
                question.vAns[j++] = tokens.Dequeue();
            question.vKeys = new bool[Question.N_ANS];
            for (int j = 0; j < Question.N_ANS; ++j)
                question.vKeys[j] = false;
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
                System.Windows.MessageBox.Show("From the end, line " + tokens.Count + " has " + nKey + " key!");
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
