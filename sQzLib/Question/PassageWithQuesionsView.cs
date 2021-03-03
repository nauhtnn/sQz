using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.Generic;

namespace sQzLib
{
    public class PassageWithQuestionsView: StackPanel
    {
        public TextBlock IdxBarView { get; private set; }
        public List<SingleQuestionView> QuestionsViews;

        void CreateIdxView(int startQuestionIdx, int endQuestionIdx)
        {
            IdxBarView = new TextBlock();
            IdxBarView.Text = "Read the following passage and answer question number " +
                startQuestionIdx + " to " + endQuestionIdx;
            IdxBarView.Foreground = Theme.s._[(int)BrushId.QID_Color];
            IdxBarView.Background = Theme.s._[(int)BrushId.QID_BG];
            IdxBarView.TextWrapping = TextWrapping.Wrap;
            IdxBarView.TextAlignment = TextAlignment.Center;
            IdxBarView.Margin = new Thickness(0, SystemParameters.ScrollWidth, 0, SystemParameters.ScrollWidth);
        }

        UIElement CreatePassageView(string passage)
        {
            TextBlock passageView = new TextBlock();
            passageView.TextWrapping = TextWrapping.Wrap;
            passageView.TextAlignment = TextAlignment.Justify;
            passageView.Text = passage;
            return passageView;
        }

        public PassageWithQuestionsView(PassageWithQuestions passage, ref int questionIdx, byte[] optionStatusArray)
        {
            CreateIdxView(questionIdx + 1, questionIdx + passage.Questions.Count);
            Orientation = Orientation.Horizontal;
            double outsideStemWidth = SingleQuestionView.StemWidth;
            SingleQuestionView.StemWidth = outsideStemWidth / 2 - SystemParameters.ScrollWidth;
            ScrollViewer sw = new ScrollViewer();
            sw.MaxHeight = 500;
            sw.Width = SingleQuestionView.StemWidth - SystemParameters.ScrollWidth;
            sw.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            sw.Content = CreatePassageView(passage.Passage);
            sw.Margin = new Thickness(0, 0, SystemParameters.ScrollWidth, 0);
            Children.Add(sw);
            sw = new ScrollViewer();
            sw.Width = SingleQuestionView.StemWidth + SingleQuestionView.IdxWidth + SystemParameters.ScrollWidth;
            sw.MaxHeight = 500;
            sw.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            QuestionsViews = new List<SingleQuestionView>();
            StackPanel questions_panel = new StackPanel();
            foreach (Question q in passage.Questions)
            {
                SingleQuestionView qView = new SingleQuestionView(q, questionIdx++, optionStatusArray);
                questions_panel.Children.Add(qView);
                QuestionsViews.Add(qView);
            }
            sw.Content = questions_panel;
            Children.Add(sw);
            SingleQuestionView.StemWidth = outsideStemWidth;
        }
    }
}
