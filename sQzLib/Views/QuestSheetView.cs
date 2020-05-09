using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using sQzLib;

namespace sQzClient
{
    public class QuestSheetView
    {
        MultiChoiceItemView QuestionViewer;

        public static QuestSheetView NewWith(double backgroundSize, double margin, StackPanel viewer, MultiChoiceItemController controller)
        {
            QuestSheetView questSheetViewer = new QuestSheetView();
            questSheetViewer.QuestionViewer = MultiChoiceItemView.NewWith(3 * margin, backgroundSize - margin - margin, viewer, controller);
            return questSheetViewer;
        }

        public void RenderModelToView(QuestSheet model)
        {
            int questIdx = 1;
            foreach (MultiChoiceItem question in model.Questions)
                QuestionViewer.RenderModelToViewer(question, questIdx++);
        }
    }
}
