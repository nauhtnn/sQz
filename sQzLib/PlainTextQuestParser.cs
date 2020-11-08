using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sQzLib
{
    class PlainTextQuestParser<T> : QuestParser<T>
    {
        public Queue<Question> ParseLines(Queue<T> lines)
        {
            Type listType = typeof(T);
            if (listType == typeof(int)) {...}
            Queue<string> plainTexts = lines.Cast<string>() as Queue<string>;
            Question q = new Question();
            while (!q.Read())
            {
                vQuest.Add(q);
                q = new Question();
            }
            return null;
        }

        bool readStmt()
        {
            if (svToken.Length == siToken)
                return true;
            tStmt = svToken[siToken++];
            return false;
        }

        bool readAns()
        {
            if (svToken.Length == siToken)
                return true;
            vAns = new string[N_ANS];
            vKeys = new bool[N_ANS];
            int n = 0;
            while (n < N_ANS && siToken < svToken.Length)
            {
                vAns[n++] = svToken[siToken++];
            }
            if (n < N_ANS)
                return true;
            return false;
        }

        public bool Read()
        {
            if (readStmt())
                return true;
            if (readAns())
                return true;
            vKeys = new bool[N_ANS];
            for (int i = 0; i < N_ANS; ++i)
                vKeys[i] = false;
            if (tStmt[0] == '*' && 1 < tStmt.Length)
            {
                bDiff = true;
                tStmt = tStmt.Substring(1);
            }
            else if (tStmt[0] == '\\' && 1 < tStmt.Length
                && (tStmt[1] == '*' || tStmt[1] == '\\'))
                tStmt = tStmt.Substring(1);
            bool na = true;
            for (int i = 0; i < N_ANS; ++i)
            {
                if (vAns[i][0] == '\\' && 1 < vAns[i].Length)
                {
                    if (vAns[i][1] != '\\')
                    {
                        vKeys[i] = true;
                        vAns[i] = Utils.CleanFront(vAns[i], 1);
                        na = false;
                    }
                    else
                        vAns[i] = vAns[i].Substring(1);
                }
            }
            if (na)
                return true;
            return false;
        }
    }
}
