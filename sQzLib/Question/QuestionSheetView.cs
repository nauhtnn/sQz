using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;

namespace sQzLib
{
    public class QuestionSheetView: StackPanel
    {
        public QuestionSheetView(QuestSheet qsheet, byte[] optionStatusArray, double idxWidth, double stemWidth, bool isEnabled = true)
        {
            SingleQuestionView.IdxWidth = idxWidth;
            SingleQuestionView.StemWidth = stemWidth;
            int idx = -1;
            foreach(QSheetSection s in qsheet.Sections)
            {
                IndependentQSection ind_section = s as IndependentQSection;
                if (ind_section != null)
                {
                    Children.Add(QSheetSection.CreateRequirementTextBlock(ind_section.Requirements));
                    AddListOfSingleQuestions(ind_section.Questions, ref idx, optionStatusArray, isEnabled);//todo: shallow copy questions
                }
                else
                {
                    BasicPassageSection passage_section = s as BasicPassageSection;
                    if (passage_section != null)
                    {
                        Children.Add(QSheetSection.CreateRequirementTextBlock(passage_section.Requirements));
                        BasicPassageSectionView view = new BasicPassageSectionView(passage_section, ref idx, optionStatusArray, isEnabled);
                        Children.Add(view);
                    }
                }
            }
        }

        void AddListOfSingleQuestions(List<Question> questions, ref int idx_start0, byte[] optionStatusArray = null, bool isEnabled = true)
        {
            foreach (Question q in questions)
                Children.Add(new SingleQuestionView(q, ++idx_start0, optionStatusArray, isEnabled));
        }
    }
}
