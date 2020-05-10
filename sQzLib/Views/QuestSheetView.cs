using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using sQzLib;

namespace sQzLib
{
    public class QuestSheetView
    {
        MultiChoiceItemView QuestionViewer;

        public static QuestSheetView NewWith(double backgroundSize, double margin, StackPanel UI_container, MultiChoiceItemController controller)
        {
            QuestSheetView questSheetViewer = new QuestSheetView();
            questSheetViewer.QuestionViewer = MultiChoiceItemView.NewWith(3 * margin, backgroundSize - margin - margin, UI_container, controller);
            OptionView.InitLabelCircle();
            return questSheetViewer;
        }

        public void RenderModelToView(QuestSheet model)
        {
            int questIdx = 1;
            foreach (MultiChoiceItem question in model.Questions)
                QuestionViewer.RenderModel(question, questIdx++);
        }
    }
}
