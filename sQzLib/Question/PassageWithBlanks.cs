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
            Stack<Tuple<int, int>> replaced_labels = AutoDetectQuestIdxLabel_in_Passage();
            if (replaced_labels == null)
                return;
            Stack<string> reversedNewPassage = new Stack<string>();
            int endQuestIdxLabel = startQuestIdxLabel + Questions.Count - 1;
            while(replaced_labels.Count > 0)
            {
                Tuple<int, int> label = replaced_labels.Pop();
                if(label.Item1 + label.Item2 < Passage.Length)
                    reversedNewPassage.Push(Passage.Substring(label.Item1 + label.Item2));
                reversedNewPassage.Push("(" + endQuestIdxLabel + ")");
                Passage = Passage.Substring(0, label.Item1);
                --endQuestIdxLabel;
            }
            StringBuilder newPassage = new StringBuilder();
            while (reversedNewPassage.Count > 0)
                newPassage.Append(reversedNewPassage.Pop());
            Passage = newPassage.ToString();
        }

        protected Stack<Tuple<int, int>> AutoDetectQuestIdxLabel_in_Passage()
        {
            MatchCollection matches = Regex.Matches(Passage, "\\(\\d+\\)");
            if (matches.Count != Questions.Count)
            {
                System.Windows.MessageBox.Show("Cannot AutoDetectQuestIdxLabel_in_Passage: matches.Count = " +
                    matches.Count + ", Questions.Count = " + Questions.Count);
                return null;
            }

            Stack<Tuple<int, int>> replaced_labels = new Stack<Tuple<int, int>>();
            foreach (Match match in matches)
            {
                replaced_labels.Push(Tuple.Create<int, int>(match.Index, match.Value.Length));
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
