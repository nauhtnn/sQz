using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.Generic;

namespace sQzLib
{
    public class BasicPassageSectionView: StackPanel
    {
        public List<SingleQuestionView> QuestionsViews;

        UIElement CreatePassageView(string passage)
        {
            TextBlock passageView = new TextBlock();
            passageView.TextWrapping = TextWrapping.Wrap;
            passageView.TextAlignment = TextAlignment.Justify;
            passageView.Text = passage;
            return passageView;
        }

        public BasicPassageSectionView(BasicPassageSection passage, ref int questionIdx, byte[] optionStatusArray, bool isEnabled = true)
        {
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
                SingleQuestionView qView = new SingleQuestionView(q, ++questionIdx, optionStatusArray, isEnabled);
                questions_panel.Children.Add(qView);
                QuestionsViews.Add(qView);
            }
            sw.Content = questions_panel;
            Children.Add(sw);
            SingleQuestionView.StemWidth = outsideStemWidth;
        }
    }
}
