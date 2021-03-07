using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sQzLib
{
    public class IndependentQSection : QSheetSection
    {
        public override bool Parse(Queue<BasicRich_PlainText> tokens)
        {
            if(QSheetSection.SECTION_MAGIC_PREFIX.Length > 0)
            {
                if (tokens.Count < 3 + Question.NUMBER_OF_OPTIONS)
                {
                    System.Windows.MessageBox.Show("IndependentQSection: From the end, line " +
                        tokens.Count + " doesn't have 1 requirement 1 stem 4 options 1 answer!");
                    return false;
                }

                Requirements = tokens.Dequeue().ToString();
            }
            else
            {
                if (tokens.Count < 2 + Question.NUMBER_OF_OPTIONS)
                {
                    System.Windows.MessageBox.Show("IndependentQSection: From the end, line " +
                        tokens.Count + " doesn't have 0 requirement 1 stem 4 options 1 answer!");
                    return false;
                }

                Requirements = string.Empty;
            }

            return ParseQuestions(tokens);
        }

        public override void DBAppendQryIns(string prefx, ref int idx, int qSheetID, StringBuilder vals)
        {
            foreach (Question q in Questions)
            {
                vals.Append(prefx +
                    qSheetID + "," + q.uId + ",'");
                foreach (int i in q.vAnsSort)
                    vals.Append(i.ToString());
                vals.Append("'," + ++idx + "),");
            }
        }
    }
}
