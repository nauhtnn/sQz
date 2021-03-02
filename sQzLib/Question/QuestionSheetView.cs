using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;

namespace sQzLib
{
    public class QuestionSheetView: StackPanel
    {
        public QuestionSheetView(QuestSheet qsheet, byte[] optionStatusArray, double idxWidth, double stemWidth, bool isEnabled = true)
        {
            SingleQuestionView.IdxWidth = idxWidth;
            SingleQuestionView.StemWidth = stemWidth;
            int idx = -1;
            AddListOfSingleQuestions(qsheet.ShallowCopyIndependentQuestions(), ref idx);
            foreach (PassageWithQuestions p in qsheet.Passages.Values)
            {
                TextBlock passageText = new TextBlock();
                passageText.Text = "\n\n" + p.Passage + "\n\n";
                passageText.Width = stemWidth;
                passageText.TextWrapping = TextWrapping.Wrap;
                passageText.TextAlignment = TextAlignment.Justify;
                Children.Add(passageText);
                AddListOfSingleQuestions(p.Questions, ref idx);
            }
        }

        void AddListOfSingleQuestions(List<Question> questions, ref int idx_start0, byte[] optionStatusArray = null, bool isEnabled = true)
        {
            foreach (Question q in questions)
                Children.Add(new SingleQuestionView(q, ++idx_start0, optionStatusArray));
        }
    }
}
