using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace sQzCS
{
    class Question
    {
        string stmt; //statement
        int nChoices;
        string[] choices;
        bool bChoiceSort;

        public Question() {
	        nChoices = 0;
	        bChoiceSort = true;
        }

        public void read(string[] v, ref int i, Settings deftSt)
        {
            int e = v.Length, ss = 0;
            stmt = v[i];
            System.Text.RegularExpressions.Match m =
                    System.Text.RegularExpressions.Regex.Match(v[i], "\\\\[0-9]+");
            if (m.Success)
            {
                int nc = int.Parse(v[i].Substring(m.Index + 1, m.Length - 1));
                if (1 < nc)
                    nChoices = nc;
                else
                    nChoices = deftSt.nChoices;
                ss = m.Index + m.Length + 1;
            }
            else
                nChoices = deftSt.nChoices;
            m = System.Text.RegularExpressions.Regex.Match(v[i], "\\\\[cC]");
            if (m.Success)
            {
                bChoiceSort = false;
                ss = m.Index + m.Length + 1;
            }
            if (0 < ss)
                stmt = v[i].Substring(ss);
            ++i;
            choices = new string[nChoices];
            int ci = 0;
            for (; ci < nChoices && i != e; ++ci)
                choices[ci] = v[i++];
            if (ci < nChoices)
            {
                for (int cj = ci; cj < nChoices; ++cj)
                    choices[cj] = null;
                nChoices = ci;
            }
        }
        //void Question::print()
        //{
        //    cout << "Stmt_" << stmt << '_' << nChoices << " choices\n";
        //    for (int i = 0; i < nChoices; ++i)
        //        cout << '_' << choices[i] << "_\n";
        //}
        public void write(System.IO.StreamWriter os, int idx)
        {
            //char* ix = new char[sizeof(int) * 8 + 1];
            //sprintf(ix, "%d", idx);
            os.Write("<div class='cl'><div class='qid'>" + idx +
                "</div><div class='q'><div class='stmt'>");
            stmt = Utils.HTMLspecialChars(stmt);
            os.Write(stmt + "</div>\n");
            string header = "<div name='" + idx + "'class='c'><span class='cid'>(",
                middle = ")</span><input type='radio' name='-" + idx + "' value='";
            char j = 'A';
            List<string> vChoices = new List<string>(choices);
            Random r = new Random();
            while (0 < vChoices.Count)
            {
                int i = 0;
                if (bChoiceSort && 1 < vChoices.Count)
                    i = r.Next(vChoices.Count - 1);
                os.Write(header + j + middle);
                string s = vChoices[i];
                vChoices.RemoveAt(i);
                if (s[0] == '\\')
                {
                    char k = (char)(j - 'A' + '0');
                    os.Write(k + "'>" +
                        Utils.HTMLspecialChars(s).Substring(1));
                }
                else
                    os.Write("#'>" + Utils.HTMLspecialChars(s));
                os.WriteLine("</div>");
                ++j;
            }
            os.Write("</div></div>");
        }
    }
}
