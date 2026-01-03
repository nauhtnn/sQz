using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Documents;

namespace sQzLib
{
    public class SingleAnswerMCQView: StackPanel
    {
        public static Thickness staticMargin = new Thickness(0, 12, 0, 12);
        public static double IdxWidth;
        public static double StemWidth;

        public int Idx { get; private set; }

        public UIElement optionsView;

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

        private UIElement CreateStemView(string text)
        {
            if (Utils.IsRichText(text))
                return CreateStemView_RichText(text);
            return CreateStemView_PlainText(text);
        }

        private RichTextBox CreateStemView_RichText(string text)
        {
            RichTextBox richText = Utils.CreateRichTextBox(text);
            richText.Focusable = false;
            richText.Width = StemWidth;
            richText.Background = Theme.s._[(int)BrushId.Q_BG];
            richText.BorderBrush = Theme.s._[(int)BrushId.QID_BG];
            richText.BorderThickness = new Thickness(0, 4, 0, 0);
            Thickness zero = new Thickness(0);
            richText.Margin = richText.Padding = zero;
            TextRange range = new TextRange(richText.Document.ContentStart, richText.Document.ContentEnd);
            range.ApplyPropertyValue(RichTextBox.FontSizeProperty, (double)16);

            return richText;
        }

        private Label CreateStemView_PlainText(string text)
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

        public SingleAnswerMCQView(Question question, int idx_start0, byte[] optionStatusArray, bool isEnabled = true)
        {
            Idx = idx_start0;
            Orientation = Orientation.Horizontal;
            Margin = staticMargin;

            Children.Add(CreateIdxView(idx_start0));

            StackPanel stemAndOptionsView = new StackPanel();
            if(question.Stem.Trim().Length > 0)
                stemAndOptionsView.Children.Add(CreateStemView(question.Stem));
            optionsView = OptionViewFactory.S().CreateOptionsView(question.QuestionType, question.Options, optionStatusArray, idx_start0);
            optionsView.IsEnabled = isEnabled;
            
            stemAndOptionsView.Children.Add(optionsView);
            Children.Add(stemAndOptionsView);
            Background = Theme.s._[(int)BrushId.BG];
        }
    }
}
