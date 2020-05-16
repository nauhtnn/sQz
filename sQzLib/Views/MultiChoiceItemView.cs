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
		MultiChoiceItem Model;
		int IdxInQuestSheet;
        double IdxHeight;
        double QuestionWidth;
        StackPanel UI_Container;
		ListBox Options;

        public static MultiChoiceItemView NewWith(MultiChoiceItem model, int idx, double idxHeight, double questionWidth, StackPanel UI_container)
        {
            MultiChoiceItemView question = new MultiChoiceItemView();
            question.IdxInQuestSheet = idx;
			question.IdxHeight = idxHeight;
            question.QuestionWidth = questionWidth;
            question.Controller = controller;
            question.UI_Container = UI_container;
            return question;
        }

        public void Render()
        {
            RenderIndex();
            UI_Container.Children.Add(NonnullRichTextView.Render(Model.Stem));
            RenderOptions();
        }

        void RenderOptions()
        {
            ListBox Options = new ListBox();
            Options.Width = QuestionWidth;
            Options.Name = "_" + IdxInQuestSheet;
            Options.SelectionChanged += Controller.Options_SelectionChanged;
            Options.BorderBrush = Theme.Singleton.DefinedColors[(int)BrushId.Ans_TopLine];
            Options.BorderThickness = new Thickness(0, 4, 0, 0);
            int optionIdx = 0;
            foreach (NonnullRichText richText in Model.Options)
            {
                OptionView option = new OptionView();
                Options.Items.Add(option.Render(richText, optionIdx++, QuestionWidth));
            }
            UI_Container.Children.Add(Options);
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
