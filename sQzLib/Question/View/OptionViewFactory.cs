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
                if (optionStatusArray != null && optionStatusArray[answerIdx++] == OptionSelectAnswer.TRUE)//update view from log
                    option.IsSelected = true;
                optionsView.Items.Add(option);
            }
            return optionsView;
        }

        public StackPanel CreateOptionsView_MTF(string[] options, byte[] choiceCodes, int questionIdx_start0)
        {
            StackPanel optionsView = new StackPanel();
            optionsView.Width = StemWidth;
            int answerIdx = -1;
            int choiceCodeIdx = questionIdx_start0 * OptionSelectAnswer.OPTION_COUNT - 1;
            foreach (string text in options)
            {
                answerIdx++;
                choiceCodeIdx++;

                TextBlock option = new TextBlock();
                option.Text = text;
                optionsView.Children.Add(option);
                
                RadioButton trueButton = new RadioButton();
                trueButton.GroupName = questionIdx_start0.ToString() + "_" + answerIdx + "_TrueFalse";
                trueButton.Content = "Đúng";
                trueButton.Name = "True";
                optionsView.Children.Add(trueButton);
                
                RadioButton falseButton = new RadioButton();
                falseButton.GroupName = questionIdx_start0.ToString() + "_" + answerIdx + "_TrueFalse";
                falseButton.Content = "Sai";
                falseButton.Name = "False";
                optionsView.Children.Add(falseButton);

                if (choiceCodes[choiceCodeIdx] == OptionSelectAnswer.TRUE)
                    trueButton.IsChecked = true;
                else if (choiceCodes[choiceCodeIdx] == OptionSelectAnswer.FALSE)
                    falseButton.IsChecked = true;
            }
            return optionsView;
        }
    }
}
