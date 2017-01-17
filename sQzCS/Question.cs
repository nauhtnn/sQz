using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace sQzCS
{

    enum QuestType
    {
        Single = 1,
        Multiple = 2,
        Insertion = 4,
        Selection = 8,
        Matching = 16
    }

    enum ContentType
    {
        Raw = 1,
        Image = 2,
        Audio = 4,
        Video = 8
    }

    class Question
    {
        string mStmt; //statement
        int nChoices;
        string[] vChoices;
        bool[] vKeys;
        bool bChoiceSort;
        QuestType qType;
        ContentType cType;

        public Question() {
	        nChoices = 0;
	        bChoiceSort = true;
            qType = QuestType.Single;
            cType = ContentType.Raw;
        }

        public void read(string[] v, ref int i, Settings deftSt)
        {
            int e = v.Length, s = 0;
            System.Text.RegularExpressions.Match m =
                    System.Text.RegularExpressions.Regex.Match(v[i], "\\\\[0-9]+ ");
            if (m.Success)
            {
                int nc = int.Parse(v[i].Substring(m.Index + 1, m.Length - 1));
                if (1 < nc)
                    nChoices = nc;
                else
                    nChoices = deftSt.nChoices;
                s = m.Index + m.Length;
            }
            else
                nChoices = deftSt.nChoices;
            m = System.Text.RegularExpressions.Regex.Match(v[i], "\\\\[cC] ");
            if (m.Success)
            {
                bChoiceSort = false;
                s = m.Index + m.Length;
            }
            mStmt = Utils.HTML(v[i++].Substring(s), ref cType);
            vChoices = new string[nChoices];
            vKeys = new bool[nChoices];
            for (int ki = 0; ki < nChoices; ++ki)
                vKeys[ki] = false;
            int ci = 0, keyC = 0;
            for (; ci < nChoices && i != e; ++ci) {
                vChoices[ci] = Utils.HTML(v[i++], ref cType);
                if (vChoices[ci][0] == '\\')
                {
                    vKeys[ci] = true;
                    ++keyC;
                    vChoices[ci] = vChoices[ci].Substring(1);
                }
            }
            if (ci < nChoices)
            {
                for (int cj = ci; cj < nChoices; ++cj)
                    vChoices[cj] = null;
                nChoices = ci;
            }
            if (1 < keyC && qType == QuestType.Single)
                qType = QuestType.Multiple;
        }
        public void write(System.IO.StreamWriter os, int idx, ref int col)
        {
            if (cType == ContentType.Image)
            {
                if (col == 1)
                    os.Write("<div class='cl'></div><div class='cl1'></div>");
                col = Program.MAX_COLUMN;
            }
            else
                ++col;
            if (cType == ContentType.Image)
                os.Write("<div class='cl2'");
            else
                os.Write("<div class='cl'");
            os.Write("><div class='qid'>" + idx +
                "</div><div class='q'><div class='stmt'>");
            if (qType == QuestType.Multiple)
                os.Write("<i>(Câu hỏi nhiều lựa chọn)</i><br>");
            os.Write(mStmt);
            os.Write("</div>\n");
            if (qType == QuestType.Single ||
                qType == QuestType.Multiple)
                wrtChoices(os, idx);
        }
        void wrtChoices(System.IO.StreamWriter os, int idx)
        {
            string header = "<div name='" + idx + "'class='c'><span class='cid'>(", middle;
            if (qType == QuestType.Single)
                middle = ")</span><input type='radio'";
            else //Multiple
                middle = ")</span><input type='checkbox'";
            middle = middle + " name='-" + idx + "' value='";
            char j = 'A';
            List<string> choices = new List<string>(vChoices);
            List<bool> keys = new List<bool>(vKeys);
            Random r = new Random();
            while (0 < choices.Count)
            {
                int i = 0;
                if (bChoiceSort && 1 < choices.Count)
                    i = r.Next(choices.Count);
                os.Write(header + j + middle);
                if (keys[i])
                {
                    char k = (char)(j - 'A' + '0');
                    os.Write(k + "'>");
                }
                else
                    os.Write("#'>");
                os.WriteLine(choices[i] + "</div>");
                choices.RemoveAt(i);
                keys.RemoveAt(i);
                ++j;
            }
            os.Write("</div></div>");
        }
    }
}
