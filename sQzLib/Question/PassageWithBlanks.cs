using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace sQzLib
{
    public class PassageWithBlanks: BasicPassageSection
    {
        public PassageWithBlanks()
        {
            Init();
        }

        public PassageWithBlanks(int id)
        {
            Init(id);
        }

        public override void UpdateQuestIndices(int startQuestIdxLabel)
        {
            base.UpdateQuestIndices(startQuestIdxLabel);
            List<string> replaced_labels = AutoDetectQuestIdxLabel_in_Passage();
            if (replaced_labels == null)
                return;
            foreach (string replaced_label in replaced_labels)
            {
                Passage = Passage.Replace(replaced_label, "(" + startQuestIdxLabel + ")");
                ++startQuestIdxLabel;
            }
        }

        protected List<string> AutoDetectQuestIdxLabel_in_Passage()
        {
            MatchCollection matches = Regex.Matches(Passage, "\\(\\d+\\)");
            if (matches.Count != Questions.Count)
            {
                System.Windows.MessageBox.Show("Cannot AutoDetectQuestIdxLabel_in_Passage: matches.Count = " +
                    matches.Count + ", Questions.Count = " + Questions.Count);
                return null;
            }

            List<string> replaced_labels = new List<string>();
            foreach (Match match in matches)
            {
                replaced_labels.Add(match.Value);
            }
            return replaced_labels;
        }

        public override object Clone()
        {
            PassageWithBlanks newSection = new PassageWithBlanks(ID);
            newSection.Requirements = Requirements;
            newSection.Passage = Passage;
            foreach (Question q in Questions)
                newSection.Questions.Add(q.DeepCopy());
            return newSection;
        }
    }
}
