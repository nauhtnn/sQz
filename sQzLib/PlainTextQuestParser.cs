using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sQzLib
{
    class PlainTextQuestParser<T> : QuestParser<T>
    {
        override public Queue<Question> ParseLines(Queue<T> lines)
        {
            Type listType = typeof(T);
            if (listType == typeof(string))
            {
                System.Windows.MessageBox.Show("Cannot parse question file: Plain text only!");
                return new Queue<Question>();
            }
            string[] plainTexts = lines.Cast<string>().ToArray();
            Queue<Question> questions = new Queue<Question>();
            for(int i = 0; i < plainTexts.Length;)
            {
                string sectionHeader = ParseSectionHeader(plainTexts[i]);
                if(sectionHeader != null)
                    SheetSections.AddSection(i++, sectionHeader);
                if (i + Question.N_ANS > plainTexts.Length)
                {
                    System.Windows.MessageBox.Show("Line " + i + " doesn't have 1 stem 4 options! Only " + questions.Count + " are read.");
                    return questions;
                }

                Question q = new Question();
                q.Stmt = plainTexts[i++];
                q.vAns = new string[Question.N_ANS];
                for (int j = 0; j < Question.N_ANS;)
                    q.vAns[j++] = plainTexts[i++];
                q.vKeys = new bool[Question.N_ANS];
                for (int j = 0; j < Question.N_ANS; ++j)
                    q.vKeys[i] = false;
                if (q.tStmt[0] == '*' && 1 < q.tStmt.Length)
                {
                    q.bDiff = true;
                    q.tStmt = q.tStmt.Substring(1);
                }
                else if (q.tStmt[0] == '\\' && 1 < q.tStmt.Length
                    && (q.tStmt[1] == '*' || q.tStmt[1] == '\\'))
                    q.tStmt = q.tStmt.Substring(1);
                int nKey = 0;
                for (int j = 0; j < Question.N_ANS; ++j)
                {
                    if (q.vAns[j][0] == '\\' && 1 < q.vAns[j].Length)
                    {
                        if (q.vAns[j][1] != '\\')
                        {
                            q.vKeys[j] = true;
                            q.vAns[j] = Utils.CleanFront(q.vAns[j].Substring(1));
                            ++nKey;
                        }
                        else
                            q.vAns[j] = q.vAns[j].Substring(1);
                    }
                }
                if (nKey != 1)
                {
                    System.Windows.MessageBox.Show("Line " + i + " has " + nKey + " key! Only " + questions.Count + " are read.");
                    return questions;
                }
            }
            return questions;
        }

        string ParseSectionHeader(string line)
        {
            if(line.IndexOf(QSheetSections.SECTION_HEADER) == 0)
            {
                if (line.Length > QSheetSections.SECTION_HEADER.Length)
                    return Utils.CleanFront(line.Substring(QSheetSections.SECTION_HEADER.Length - 1));
                else
                    return string.Empty;
            }
            return null;
        }
    }
}
