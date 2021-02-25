using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace sQzLib
{
    public class SingleQuestionView: StackPanel
    {
        public static Thickness staticMargin;
        public static double IdxWidth;
        public static double StemWidth;

        public ListBox optionsView;

        private Label CreateIdxView(int idx)
        {
            Label idxView = new Label();//to optimize: use Border
            idxView.HorizontalAlignment = HorizontalAlignment.Left;
            idxView.VerticalAlignment = VerticalAlignment.Top;
            idxView.Content = (++idx).ToString();
            idxView.Background = Theme.s._[(int)BrushId.QID_BG];
            idxView.Foreground = Theme.s._[(int)BrushId.QID_Color];
            idxView.Width = idxView.Height = IdxWidth;
            idxView.HorizontalContentAlignment = HorizontalAlignment.Center;
            idxView.VerticalContentAlignment = VerticalAlignment.Center;
            idxView.Padding = new Thickness(0);
            return idxView;
        }

        private Label CreateStemView(string text)
        {
            TextBlock insideText = new TextBlock();
            insideText.Text = text;
            insideText.TextWrapping = TextWrapping.Wrap;
            insideText.Width = StemWidth;
            insideText.Background = Theme.s._[(int)BrushId.Q_BG];
            Label stemView = new Label();
            stemView.Content = insideText;
            stemView.BorderBrush = Theme.s._[(int)BrushId.QID_BG];
            stemView.BorderThickness = new Thickness(0, 4, 0, 0);
            Thickness zero = new Thickness(0);
            stemView.Margin = stemView.Padding = zero;
            return stemView;
        }

        public SingleQuestionView(Question question, int idx, bool[] optionStatusArray)
        {
            Orientation = Orientation.Horizontal;
            Margin = staticMargin;

            Children.Add(CreateIdxView(idx));

            StackPanel stemAndOptionsView = new StackPanel();
            stemAndOptionsView.Children.Add(CreateStemView(question.Stem));
            optionsView = CreateOptionsView(question.vAns, optionStatusArray, idx);

            
            stemAndOptionsView.Children.Add(optionsView);
            Children.Add(stemAndOptionsView);
            Background = Theme.s._[(int)BrushId.BG];
        }

        public ListBox CreateOptionsView(string[] options, bool[] optionStatusArray, int questionIdx)
        {
            ListBox optionsView = new ListBox();
            optionsView.Width = StemWidth;
            optionsView.BorderBrush = Theme.s._[(int)BrushId.Ans_TopLine];
            optionsView.BorderThickness = new Thickness(0, 4, 0, 0);
            int idx = 0;
            int answerIdx = questionIdx * Question.NUMBER_OF_OPTIONS;
            foreach(string text in options)
            {
                OptionView option = new OptionView(text, idx++, StemWidth);
                if (optionStatusArray[answerIdx++] == true)//update view from log
                    option.IsSelected = true;
                optionsView.Items.Add(option);
            }
            return optionsView;
        }
    }
}
