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
        public static double IdxHeight;
        public static double QuestionWidth;
        public static TakeExam Controller;

        public static void RenderModelToViewer(MultiChoiceItem question, int idx, StackPanel viewer)
        {
            RenderIndexToViewer(idx, viewer);
            RenderIndexLineToViewer(viewer);
            viewer.Children.Add(NonnullRichTextView.Render(question.Stem));
            RenderOptionsToViewer(question, idx, viewer);
        }

        static void RenderOptionsToViewer(MultiChoiceItem question, int idx, StackPanel viewer)
        {
            ListBox optionsView = new ListBox();
            optionsView.Width = QuestionWidth;
            optionsView.Name = "_" + idx;
            optionsView.SelectionChanged += Controller.Options_SelectionChanged;
            optionsView.BorderBrush = Theme.Singleton.DefinedColors[(int)BrushId.Ans_TopLine];
            optionsView.BorderThickness = new Thickness(0, 4, 0, 0);
            foreach (NonnullRichText richText in question.Options)
            {
                ListBoxItem option = new ListBoxItem();
                option.Content = NonnullRichTextView.Render(richText);
                optionsView.Items.Add(option);
            }
            viewer.Children.Add(optionsView);
        }

        static void RenderIndexToViewer(int idx, StackPanel viewer)
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
            viewer.Children.Add(idxLabel);
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
    }
}
