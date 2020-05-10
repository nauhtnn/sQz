using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using sQzLib;

namespace sQzLib
{
    class MultiChoiceItemView
    {
        double IdxHeight;
        double QuestionWidth;
        MultiChoiceItemController Controller;
        StackPanel UI_Container;

        public static MultiChoiceItemView NewWith(double idxHeight, double questionWidth, StackPanel UI_container, MultiChoiceItemController controller)
        {
            OptionView.InitLabelCircle();

            MultiChoiceItemView question = new MultiChoiceItemView();
            question.IdxHeight = idxHeight;
            question.QuestionWidth = questionWidth;
            question.Controller = controller;
            question.UI_Container = UI_container;
            return question;
        }

        public void RenderModel(MultiChoiceItem question, int idx)
        {
            RenderIndex(idx);
            UI_Container.Children.Add(NonnullRichTextView.Render(question.Stem));
            RenderOptions(question, idx);
        }

        void RenderOptions(MultiChoiceItem question, int idx)
        {
            ListBox optionsView = new ListBox();
            optionsView.Width = QuestionWidth;
            optionsView.Name = "_" + idx;
            optionsView.SelectionChanged += Controller.Options_SelectionChanged;
            optionsView.BorderBrush = Theme.Singleton.DefinedColors[(int)BrushId.Ans_TopLine];
            optionsView.BorderThickness = new Thickness(0, 4, 0, 0);
            int optionIdx = 0;
            foreach (NonnullRichText richText in question.Options)
            {
                OptionView option = new OptionView();
                optionsView.Items.Add(option.Render(richText, optionIdx++, QuestionWidth));
            }
            UI_Container.Children.Add(optionsView);
        }

        void RenderIndex(int idx)
        {
            Label idxLabel = new Label();
            idxLabel.HorizontalAlignment = HorizontalAlignment.Left;
            idxLabel.Content = idx;
            idxLabel.Background = Theme.Singleton.DefinedColors[(int)BrushId.QID_BG];
            idxLabel.Foreground = Theme.Singleton.DefinedColors[(int)BrushId.QID_Color];
            idxLabel.Width = IdxHeight;
            idxLabel.Height = IdxHeight;
            idxLabel.HorizontalContentAlignment = HorizontalAlignment.Center;
            idxLabel.VerticalContentAlignment = VerticalAlignment.Center;
            idxLabel.Padding = new Thickness(0);
            idxLabel.Margin = new Thickness(0, IdxHeight, 0, 0);
            UI_Container.Children.Add(idxLabel);

            RenderIndexLine();
        }

        void RenderIndexLine()
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
            UI_Container.Children.Add(stmtBox);
        }
    }
}
