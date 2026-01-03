using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace sQzLib
{
    public class OptionViewFactory
    {
        static OptionViewFactory Singleton;
        public static OptionViewFactory S()
        {
            if(Singleton == null)
            {
                Singleton = new OptionViewFactory();
            }
            return Singleton;
        }

        public double StemWidth;

        public OptionViewFactory()
        {
            StemWidth = 100;
        }

        public UIElement CreateOptionsView(AnswerType questionType, string[] options, byte[] optionStatusArray, int questionIdx_start0)
        {
            if(questionType == AnswerType.MultipleTrueFalse)
            {
                return CreateOptionsView_MTF(options, optionStatusArray, questionIdx_start0);
            }
            else
            {
                return CreateOptionsView_SingleAnswer(options, optionStatusArray, questionIdx_start0);
            }
        }

        public ListBox CreateOptionsView_SingleAnswer(string[] options, byte[] optionStatusArray, int questionIdx_start0)
        {
            ListBox optionsView = new ListBox();
            optionsView.Width = StemWidth;
            optionsView.BorderBrush = Theme.s._[(int)BrushId.Ans_TopLine];
            optionsView.BorderThickness = new Thickness(0, 4, 0, 0);
            int idx = 0;
            int answerIdx = questionIdx_start0 * OptionSelectAnswer.OPTION_COUNT;
            foreach (string text in options)
            {
                OptionView option = new OptionView(text, idx++, StemWidth);
                if (optionStatusArray != null && optionStatusArray[answerIdx++] != 0)//update view from log
                    option.IsSelected = true;
                optionsView.Items.Add(option);
            }
            return optionsView;
        }

        public StackPanel CreateOptionsView_MTF(string[] options, byte[] optionStatusArray, int questionIdx_start0)
        {
            StackPanel optionsView = new StackPanel();
            optionsView.Width = StemWidth;
            int idx = 0;
            int answerIdx = questionIdx_start0 * OptionSelectAnswer.OPTION_COUNT;
            foreach (string text in options)
            {
                TextBlock option = new TextBlock();
                option.Text = text;
                optionsView.Children.Add(option);
                RadioButton trueSelect = new RadioButton();
                trueSelect.GroupName = questionIdx_start0.ToString() + "_TrueFalse";
                trueSelect.Content = "Đúng";
                trueSelect.Name = "True";
                optionsView.Children.Add(trueSelect);
                RadioButton falseSelect = new RadioButton();
                falseSelect.GroupName = questionIdx_start0.ToString() + "_TrueFalse";
                falseSelect.Content = "Sai";
                falseSelect.Name = "False";
                optionsView.Children.Add(falseSelect);
            }
            return optionsView;
        }
    }
}
