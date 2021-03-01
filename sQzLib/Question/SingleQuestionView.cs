using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace sQzLib
{
    public class SingleQuestionView: StackPanel
    {
        public static Thickness staticMargin = new Thickness(0, 12, 0, 12);
        public static double IdxWidth;
        public static double StemWidth;

        public ListBox optionsView;

        private Label CreateIdxView(int idx_start0)
        {
            Label idxView = new Label();//to optimize: use Border
            idxView.HorizontalAlignment = HorizontalAlignment.Left;
            idxView.VerticalAlignment = VerticalAlignment.Top;
            idxView.Content = (++idx_start0).ToString();
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

        public SingleQuestionView(Question question, int idx_start0, byte[] optionStatusArray)
        {
            Orientation = Orientation.Horizontal;
            Margin = staticMargin;

            Children.Add(CreateIdxView(idx_start0));

            StackPanel stemAndOptionsView = new StackPanel();
            //stemAndOptionsView.Children.Add(CreateStemView(question.Stem));
            optionsView = CreateOptionsView(question.vAns, optionStatusArray, idx_start0);

            
            stemAndOptionsView.Children.Add(optionsView);
            Children.Add(stemAndOptionsView);
            Background = Theme.s._[(int)BrushId.BG];
        }

        public ListBox CreateOptionsView(string[] options, byte[] optionStatusArray, int questionIdx_start0)
        {
            ListBox optionsView = new ListBox();
            optionsView.Width = StemWidth;
            optionsView.BorderBrush = Theme.s._[(int)BrushId.Ans_TopLine];
            optionsView.BorderThickness = new Thickness(0, 4, 0, 0);
            int idx = 0;
            int answerIdx = questionIdx_start0 * Question.NUMBER_OF_OPTIONS;
            foreach(string text in options)
            {
                OptionView option = new OptionView(text, idx++, StemWidth);
                if (optionStatusArray != null && optionStatusArray[answerIdx++] != 0)//update view from log
                    option.IsSelected = true;
                optionsView.Items.Add(option);
            }
            return optionsView;
        }
    }
}
