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
            foreach(QSheetSection section in qsheet.Sections)
            {
                if(section.ischildof indePen)
            }
            AddListOfSingleQuestions(qsheet.ShallowCopyIndependentQuestions(), ref idx, optionStatusArray, isEnabled);
            ++idx;
            foreach (BasicPassageSection p in qsheet.Passages.Values)
            {
                PassageWithQuestionsView view = new PassageWithQuestionsView(p, ref idx, optionStatusArray, isEnabled);
                Children.Add(view.IdxBarView);
                Children.Add(view);
            }
        }

        void AddListOfSingleQuestions(List<Question> questions, ref int idx_start0, byte[] optionStatusArray = null, bool isEnabled = true)
        {
            foreach (Question q in questions)
                Children.Add(new SingleQuestionView(q, ++idx_start0, optionStatusArray, isEnabled));
        }
    }
}
