﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sQzLib
{
    public class BasicPassageSection: QSheetSection
    {
        public string Passage;

        public BasicPassageSection()
        {
            Init();
        }

        public BasicPassageSection(int id)
        {
            Init(id);
        }

        public override bool Parse(Queue<BasicRich_PlainText> tokens)
        {
            if (tokens.Count < 4 + Question.NUMBER_OF_OPTIONS)
            {
                System.Windows.MessageBox.Show("BasicPassageSection: From the end, line " +
                    tokens.Count + " doesn't have 1 requirement 1 passage 1 stem 4 options 1 answer!");
                return false;
            }

            Requirements = tokens.Dequeue().ToString();

            Passage = tokens.Dequeue().ToString();

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

        public void Randomize_KeepQuestionOrder(Random rand)
        {
            List<Question> newList = new List<Question>();
            foreach (Question q in Questions)
                newList.Add(q.RandomizeDeepCopy(rand));
            Questions = newList;
        }

        public override object Clone()
        {
            BasicPassageSection newSection = new BasicPassageSection(ID);
            newSection.Requirements = Requirements;
            newSection.Passage = Passage;
            foreach(Question q in Questions)
                newSection.Questions.Add(q.DeepCopy());
            return newSection;
        }
    }
}
