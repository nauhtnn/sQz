using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace sQzLib
{
    public class PassageWithQuestionsView: StackPanel
    {
        UIElement CreateIdxView(int idx)
        {
            TextBlock idxView = new TextBlock();
            idxView.Text = "Passage " + (++idx) + ":";
            return idxView;
        }

        UIElement CreatePassageView(string passage)
        {
            TextBlock passageView = new TextBlock();
            passageView.Text = passage;
            return passageView;
        }

        public PassageWithQuestionsView(PassageWithQuestions passage, int passageIdx, ref int questionIdx, bool[] optionStatusArray)
        {
            Children.Add(CreateIdxView(passageIdx));
            Children.Add(CreatePassageView(passage.Passage));
            foreach (Question q in passage.Questions)
                Children.Add(new SingleQuestionView(q, questionIdx++, optionStatusArray));
        }
    }
}
