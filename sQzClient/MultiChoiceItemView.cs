using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using sQzLib;

namespace sQzClient
{
    class MultiChoiceItemView
    {
        static double IdxHeight;
        static double StemWidth;

        public static void SetSize(double backgroundSize, double margin)
        {
            IdxHeight = 3 * margin;
            StemWidth = backgroundSize - margin - margin;
        }
        public static void RenderQuestWithIdxToViewer(MultiChoiceItem question, int idx, StackPanel questionsView)
        {
            RenderIndexToViewer(idx, questionsView);
            RenderIndexLineToViewer(questionsView);
            questionsView.Children.Add(NonnullRichTextView.Render(question.Stem));
            RenderOptionsLineToViewer(questionsView);
            ListBox optionsView = new ListBox();
            foreach (NonnullRichText richText in question.Options)
                optionsView.Items.Add(NonnullRichTextView.Render(richText));
            questionsView.Children.Add(optionsView);
        }

        static void RenderIndexToViewer(int idx, StackPanel questionsView)
        {
            Label idxLabel = new Label();
            idxLabel.HorizontalAlignment = HorizontalAlignment.Left;
            idxLabel.VerticalAlignment = VerticalAlignment.Top;
            idxLabel.Content = idx;
            idxLabel.Background = Theme.Singleton.DefinedColors[(int)BrushId.QID_BG];
            idxLabel.Foreground = Theme.Singleton.DefinedColors[(int)BrushId.QID_Color];
            idxLabel.Width = IdxHeight;
            idxLabel.Height = IdxHeight;
            idxLabel.HorizontalContentAlignment = HorizontalAlignment.Center;
            idxLabel.VerticalContentAlignment = VerticalAlignment.Center;
            idxLabel.Padding = new Thickness(0);
            idxLabel.Margin = new Thickness(0, IdxHeight, 0, 0);
            questionsView.Children.Add(idxLabel);
        }

        static void RenderIndexLineToViewer(StackPanel questionsView)
        {
            //TextBlock stmt = new TextBlock();
            //stmt.Text = "xx";// question.Stem;
            //stmt.TextWrapping = TextWrapping.Wrap;
            //stmt.Width = StemWidth;
            //stmt.Background = Theme.Singleton.DefinedColors[(int)BrushId.Q_BG];
            Label stmtBox = new Label();
            //stmtBox.Content = stmt;
            stmtBox.BorderBrush = Theme.Singleton.DefinedColors[(int)BrushId.QID_BG];
            stmtBox.BorderThickness = new Thickness(0, 4, 0, 0);
            Thickness zero = new Thickness(0);
            stmtBox.Margin = stmtBox.Padding = zero;
            questionsView.Children.Add(stmtBox);
        }

        static void RenderOptionsLineToViewer(StackPanel questionsView)
        {
            //TextBlock stmt = new TextBlock();
            //stmt.Text = "xx";// question.Stem;
            //stmt.TextWrapping = TextWrapping.Wrap;
            //stmt.Width = StemWidth;
            //stmt.Background = Theme.Singleton.DefinedColors[(int)BrushId.Q_BG];
            Label stmtBox = new Label();
            //stmtBox.Content = stmt;
            stmtBox.BorderBrush = Theme.Singleton.DefinedColors[(int)BrushId.Ans_TopLine];
            stmtBox.BorderThickness = new Thickness(0, 4, 0, 0);
            Thickness zero = new Thickness(0);
            stmtBox.Margin = stmtBox.Padding = zero;
            questionsView.Children.Add(stmtBox);
        }

        StackPanel CreateQuestBox(int idx)
        {
            throw new NotImplementedException();
            //StackPanel questBox = new StackPanel();
            //questBox.Orientation = Orientation.Horizontal;
            //questBox.Margin = qMrg;
            //questBox.Background = Theme.Singleton.DefinedColors[(int)BrushId.BG];

            //StackPanel questBoxInside = new StackPanel();

            //MultiChoiceItem question = mQuestSheet.Q(idx - 1);

            //questBoxInside.Children.Add(CreateStmtInsideQuestBox(question));

            //questBoxInside.Children.Add(mExaminee.mAnsSheet.vlbxAns[idx - 1]);

            //questBox.Children.Add(questBoxInside);

            //return questBox;
        }
    }
}
